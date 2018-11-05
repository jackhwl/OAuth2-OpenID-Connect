/* Program.cs*/

//
// Parallel Version based on Producer-Consumer pattern  [ End Result?  Faster, but not linear ]
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
			var sw = System.Diagnostics.Stopwatch.StartNew();

			//
			// Process cmd-line args, welcome msg:
			//
			string infile;
			int maxCapacity = 10000;  // size of collection between producer and consumers:

			ProcessCmdLineArgs(args, maxCapacity, out infile);


			//
			// (1) Read infile using a separate task, aka "producer":
			//
			sw.Restart();

			// shared work queue between producer and consumers:
			BlockingCollection<string> workQ = new BlockingCollection<string>(maxCapacity);

			// Note: inform .NET scheduler that tasks are long-running:
			var tf = new TaskFactory(TaskCreationOptions.LongRunning, TaskContinuationOptions.None);

			Task producer = tf.StartNew(() =>
				{
					//
					// Producer simply puts records from file into work queue:
					//
					foreach (string record in File.ReadLines(infile))
						workQ.Add(record);

					// Signal that we are done adding to the queue:
					workQ.CompleteAdding();
				}
			);


			// 
			// (2) Foreach record, parse and aggregate the pairs <userid, num reviews>,
			// using a set of "consumer" tasks, one per core:
			//
			Dictionary<int, int> ReviewsByUser = new Dictionary<int, int>();

			//
			// We need to keep track of consumers, and each consumer needs a private
			// dictionary for collecting its results.  This private dictionary is 
			// then returned as the result of the task when it completes:
			//
			int numCores = System.Environment.ProcessorCount;

			Task<Dictionary<int, int>>[] consumers = new Task<Dictionary<int, int>>[numCores];

			for (int i = 0; i < numCores; i++)  // 1 consumer per core:
			{

				// each consumer will return a dictionary of its pairs:
				consumers[i] = tf.StartNew<Dictionary<int, int>>(() =>
					{
						Dictionary<int, int> localD = new Dictionary<int, int>();

						//
						// while producer is not completed and queue is not empty:
						//
						while (!workQ.IsCompleted)
						{
							try
							{
								//
								// wait for a record, throws exception if producer has 
								// completed and none will become available:
								//
								string line = workQ.Take();
								int userid = parse(line);

								if (!localD.ContainsKey(userid))  // first review:
									localD.Add(userid, 1);
								else  // another review by same user:
									localD[userid]++;
							}
							catch(ObjectDisposedException)
							{
								/* ignore -- no record */
							}
							catch(InvalidOperationException)
							{
								/* ignore --- no record */
							}
						}//while

						//
						// return our dictionary for main thread to aggregate:
						//
						return localD;
					}
				);

			}


			//
			// (3) WaitAllOneByOne:
			//
			// Now wait for the consumers to finish...  As they do, we 
			// aggregate their individual results into global dictionary:
			//
			int completed = 0;

			while (completed < numCores)
			{
				// 
				// wait for one to finish, then grab their result:
				//
				int tid = Task.WaitAny(consumers);

				Dictionary<int, int> localD = consumers[tid].Result;

				//
				// merge into global data structure:
				//
				foreach (int userid in localD.Keys)
				{
					int numreviews = localD[userid];

					if (!ReviewsByUser.ContainsKey(userid))  // first review:
						ReviewsByUser.Add(userid, numreviews);
					else  // another review by same user:
						ReviewsByUser[userid] += numreviews;
				}

				//
				// remove completed task from array and repeat:
				//
				completed++;
				consumers = consumers.Where((t) => t != consumers[tid]).ToArray();
			}//while


			//
			// (4) Sort pairs by num reviews, descending order, and take top 10:
			//
			var sort = from user in ReviewsByUser
								 orderby user.Value descending, user.Key ascending
								 select new { Userid = user.Key, NumReviews = user.Value };

			var top10 = sort.Take(10).ToList();

			long timems = sw.ElapsedMilliseconds;

			//
			// Write out the results:
			//
			Console.WriteLine();
			Console.WriteLine("** Top 10 users reviewing movies:");

			foreach (var user in top10)
				Console.WriteLine("{0}: {1}", user.Userid, user.NumReviews);

			// 
			// Done:
			//
			double time = timems / 1000.0;  // convert milliseconds to secs

			Console.WriteLine();
			Console.WriteLine("** Done! Time: {0:0.000} secs", time);
			Console.WriteLine();
			Console.WriteLine();
		    Console.ReadLine();

			//
			// As per TPL guidelines, wait for producer to finish so we can 
			// check for any exceptions:
			//
			try
			{
				producer.Wait();
			}
			catch (AggregateException ae)
			{
				ae = ae.Flatten();
				foreach (Exception e in ae.InnerExceptions)
					Console.WriteLine("NOTE: unexpected error from producer: '{0}'.", e.Message);
			}
			catch (Exception ex)
			{
				Console.WriteLine("NOTE: unexpected error from producer: '{0}'.", ex.Message);
			}
		}


		/// <summary>
		/// Parses one line of the netflix data file, and returns the userid who reviewed the movie.
		/// </summary>
		/// <param name="line"></param>
		/// <returns></returns>
		private static int parse(string line)
		{
			char[] separators = { ',' };

			//
			// movie id, user id, rating (1..5), date (YYYY-MM-DD)
			//
			string[] tokens = line.Split(separators);

			int movieid = Convert.ToInt32(tokens[0]);
			int userid = Convert.ToInt32(tokens[1]);
			int rating = Convert.ToInt32(tokens[2]);
			DateTime date = Convert.ToDateTime(tokens[3]);

			return userid;
		}


		/// <summary>
		/// Processes any command-line args (currently there are none), returning
		/// the input file to read from.
		/// </summary>
		/// <param name="args">cmd-line args to program</param>
		/// <param name="infile">input file to read from</param>
		///
		static void ProcessCmdLineArgs(string[] args, int maxCapacity, out string infile)
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

			Console.WriteLine("** Parallel, Producer-Consumer Top-10 Netflix Data Mining App [{0}, {1}] **", platform, version);
			Console.WriteLine("   Infile:     '{0}' ({1:#,##0.00 MB})", infile, sizeinMB);
			Console.WriteLine("   Collection:  {0:#,##0} elements", maxCapacity);
		}

	}//class
}//namespace
