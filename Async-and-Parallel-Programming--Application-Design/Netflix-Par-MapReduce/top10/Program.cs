/* Program.cs*/

//
// Parallel C# Version using MapReduce-like approach  [ End result?  Fast, near-linear speedup ]
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

			ProcessCmdLineArgs(args, out infile);

			// 
			// Foreach record, parse and aggregate the pairs <userid, num reviews>:
			//
			sw.Restart();

			Dictionary<int, int> ReviewsByUser = new Dictionary<int, int>();

			//
			//foreach (string line in File.ReadLines(infile))
			//
			Parallel.ForEach(File.ReadLines(infile),

				//
				// Initializer:  create task-local Dictionary:
				//
				() => { return new Dictionary<int, int>(); },


				//
				// Loop-body: work with TLS which represents a local Dictionary,
				// mapping our results into this local dictionary:
				//
				(line, loopControl, localD) =>
				{
					//
					// movie id, user id, rating (1..5), date (YYYY-MM-DD)
					//
					int userid = parse(line);

					if (!localD.ContainsKey(userid))  // first review:
						localD.Add(userid, 1);
					else  // another review by same user:
						localD[userid]++;

					return localD;  // return out so it can be passed back in later:
				},

				//
				// Finalizer: reduce individual local dictionaries into global dictionary:
				//
				(localD) =>
				{
					lock (ReviewsByUser)
					{
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
					}
				}

			);

			//
			// Sort pairs by num reviews, descending order, and take top 10:
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
			Console.WriteLine();
			
			Console.Write("Press a key to exit...");
			Console.ReadKey();
		}


		/// <summary>
		/// Parses one line of the netflix data file, and returns the userid who reviewed the movie.
		/// </summary>
		/// <param name="line"></param>
		/// <returns></returns>
		private static int parse(string line)
		{
			char[] separators = { ',' };

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

			Console.WriteLine("** Parallel, MapReduce Top-10 Netflix Data Mining App [{0}, {1}] **", platform, version);
			Console.Write("   Infile:  '{0}' ({1:#,##0.00 MB})", infile, sizeinMB);
			Console.WriteLine();
		}

	}//class
}//namespace
