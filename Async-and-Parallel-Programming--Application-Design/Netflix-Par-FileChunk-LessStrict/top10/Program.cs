/* Program.cs*/

//
// Less-strict chunk-based version:  chop file into N chunks, one per task

// "Less-strict" improves performance by yielding the top reviewers, but
// not necessarily the top-10 in ALL cases --- if the reviews are evenly 
// distributed throughout the file you'll get the same results as the 
// sequential version, but it is possible to create pathological input files
// where this parallel version may not produce the same results as the 
// sequential version.
//
// Why do this?  Two reasons:  (1) to obtain scalable performance we need to
// loosen up application requirements, and (2) do we really need the exact
// top 10?  Having the top reviewers, whether top 10 or top 20, is probably
// sufficient for intended purpose (i.e. to reward top reviewers).
//
// In parallel, processes a text file of movie reviews, one per line, 
// the format of which are:
//
//   movie id, user id, rating (1..5), date (YYYY-MM-DD)
//
// The output are the top 10 users who reviewed the most movies, in 
// descending order (so the user with the most reviews is listed 
// first).
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;


namespace top10
{
	class Program
	{

		static void Main(string[] args)
		{
			//
			// Process cmd-line args, welcome msg:
			//
			string infile;

			ProcessCmdLineArgs(args, out infile);

			int N = 10;  // top-10:

			//
			// (1) Read infile:
			//
			var sw = System.Diagnostics.Stopwatch.StartNew();

			//string[] records = File.ReadAllLines(infile);
			//
			// Instead of reading first and then processing, each task will read and process
			// a chunk of the file in parallel.  See below.
			//

			long readms = sw.ElapsedMilliseconds;
			sw.Restart();

			// 
			// (2) Foreach record, parse and aggregate the pairs <userid, num reviews>,
			// sort, and concurrently store into a shared dictionary:
			//
			ConcurrentDictionary<int, int> __ReviewsByUser = new ConcurrentDictionary<int, int>();

			char[] separators = {','};

			int numCores = System.Environment.ProcessorCount;

			Task[] tasks = new Task[numCores];

			//
			// Start tasks, one per core, where each task processes one chunk of file:
			//
			for (int i = 0; i < numCores; i++)
			{
				//
				// NOTE: we need to pass each task a unique task id in the range 0, 1, 2, ...
				// so they will process different sections of the file.  Notice that to do
				// this correctly, we pass loop index "i" as an arg to task:
				//
				tasks[i] = Task.Factory.StartNew((arg) =>
					{
						int myTaskID = (int) arg;  // 0, 1, 2, ...

						// we use a local dictionary to save results from our chunk:
						Dictionary<int, int> ReviewsByUser = new Dictionary<int, int>();

						//
						// open file and position at start of our chunk:
						//
						using (FileStream file = new FileStream(infile, FileMode.Open, FileAccess.Read, FileShare.Read))
						{
							long chunkSize = file.Length / numCores;
							long offsetStart = chunkSize * myTaskID;
							long offsetEnd = offsetStart + chunkSize;

							file.Position = offsetStart;

							//
							//Console.WriteLine("{0}: {1:#,##0}, {2:#,##0}", myTaskID, offsetStart, offsetEnd);
							//

							StreamReader reader = new StreamReader(file);

							//
							// Now process our chunk:
							//
							while (file.Position < offsetEnd)
							{
								string record = reader.ReadLine();
								string[] tokens = record.Split(separators);

								if (tokens.Length == 4)  // valid record:
								{
									int movieid = Convert.ToInt32(tokens[0]);
									int userid = Convert.ToInt32(tokens[1]);
									int rating = Convert.ToInt32(tokens[2]);
									DateTime date = Convert.ToDateTime(tokens[3]);

									if (!ReviewsByUser.ContainsKey(userid))  // first review:
										ReviewsByUser.Add(userid, 1);
									else  // another review by same user:
										ReviewsByUser[userid]++;
								}
							}
						}//using

						//
						// Now sort our local dictionary and add our top-10 into shared 
						// dictionary:
						//
						// NOTE: this is where parallel version breaks in pathological input
						// cases.  For example, imagine an input file where user 123 is 11th
						// in every chunk, while the top 10 is different in every chunk.
						// At the end, user 123 will be left off the final top-10 list, yet
						// will probably have the most reviews.
						//
						var query = (from user in ReviewsByUser
												 orderby user.Value descending, user.Key ascending
												 select user).Take(N);

						foreach (var user in query)
							__ReviewsByUser.AddOrUpdate(user.Key,
								user.Value,
								(key, value) => { return value + user.Value; });
					},

					i  // each task needs a unique task id (0, 1, 2, ...):
				);

			}//for

			Task.WaitAll(tasks);

			long minems = sw.ElapsedMilliseconds;
			sw.Restart();

			//
			// (3) Sort pairs by num reviews, descending order, and take top 10:
			//
			// NOTE: the dictionary contains 10 or so entries, so sort is fast.
			//
			var __query = (from user in __ReviewsByUser
									 orderby user.Value descending, user.Key ascending
									 select user).Take(N);
			
			// execute the query and capture the results:
			List<Tuple<int, int>> finaltop = new List<Tuple<int,int>>(N);

			foreach (var user in __query)
				finaltop.Add(new Tuple<int, int>(user.Key, user.Value));

			long sortms = sw.ElapsedMilliseconds;

			//
			// Write out the results:
			//
			Console.WriteLine();
			Console.WriteLine("** Top 10 users reviewing movies:");

			foreach (var user in finaltop)
				Console.WriteLine("{0}: {1}", user.Item1, user.Item2);

			// 
			// Done:
			//
			double readtime = readms / 1000.0;  // convert milliseconds to secs
			double minetime = minems / 1000.0;  
			double sorttime = sortms / 1000.0;
			double time = readtime + minetime + sorttime;

			Console.WriteLine();
			Console.WriteLine("** Done! Time: {0:0.000} secs", time);
			Console.WriteLine("**       Read: {0:0.000} secs", readtime);
			Console.WriteLine("**       Mine: {0:0.000} secs", minetime);
			Console.WriteLine("**       Sort: {0:0.000} secs", sorttime);
			Console.WriteLine();
			Console.WriteLine();
		    Console.ReadLine();
		}


		/// <summary>
		/// Processes any command-line args (currently there are none), returning
		/// the input file to read from.
		/// </summary>
		/// <param name="args">cmd-line args to program</param>
		/// <param name="infile">input file to read from</param>
		///
		static void ProcessCmdLineArgs(string[] args, out string infile)
		{
			String version, platform;

#if DEBUG
			version = "debug";
#else
			version = "release";
#endif

#if _WIN64
	platform = "64-bit";
#elif _WIN32
	platform = "32-bit";
#else
			platform = "any-cpu";
#endif

			//
			// Defaults:
			//
			infile = "ratings.txt";

			//
			// There should be no command-line args:
			//
			if (args.Length != 0)
			{
				Console.WriteLine("Usage: top10.exe");
				Console.WriteLine();
				Console.WriteLine();
				System.Environment.Exit(-1);
			}

			if (!File.Exists(infile))
			{
				Console.WriteLine("** Error: infile '{0}' does not exist.", infile);
				Console.WriteLine();
				Console.WriteLine();
				System.Environment.Exit(-1);
			}

			//
			// Process command-line args to get infile and outfile:
			//
			FileInfo fi = new FileInfo(infile);
			double sizeinMB = fi.Length / 1048576.0;
			double chunkSize = (fi.Length / System.Environment.ProcessorCount) / 1048576.0;

			Console.WriteLine("** Parallel (File-Chunking, Less-Strict) Top-10 Netflix Data Mining App [{0}, {1}] **", platform, version);
			Console.WriteLine("   Infile:    '{0}' ({1:#,##0.00 MB})", infile, sizeinMB);
			Console.WriteLine("   Num tasks:  {0}", System.Environment.ProcessorCount);
			Console.WriteLine("   Chunksize:  {0:#,##0.00 MB}", chunkSize);
			Console.WriteLine();
		}

	}//class
}//namespace
