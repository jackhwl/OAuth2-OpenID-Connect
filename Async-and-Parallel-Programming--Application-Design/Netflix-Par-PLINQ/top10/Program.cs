/* Program.cs*/

//
// Parallel LINQ Version
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

			char[] separators = { ',' };

			var ReviewsByUser = File.ReadLines(infile).AsParallel().
				Select(line => parse(line)).
				GroupBy(userid => { return userid; }).
				Select(g => new { UserId = g.Key, NumReviews = g.Count() });

			//
			// Sort pairs by num reviews, descending order, and take top 10:
			//
			var Top10 = (from user in ReviewsByUser.AsParallel()
								 orderby user.NumReviews descending, user.UserId ascending
								 select user).Take(10).ToList();

			long timems = sw.ElapsedMilliseconds;

			//
			// Write out the results:
			//
			Console.WriteLine();
			Console.WriteLine("** Top 10 users reviewing movies:");

			foreach (var user in Top10)
				Console.WriteLine("{0}: {1}", user.UserId, user.NumReviews);

			// 
			// Done:
			//
			double time = timems / 1000.0;  // convert milliseconds to secs

			Console.WriteLine();
			Console.WriteLine("** Done! Time: {0:0.000} secs", time);
			Console.WriteLine();
			Console.WriteLine();
		    Console.ReadLine();
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

			Console.WriteLine("** Parallel, PLINQ Top-10 Netflix Data Mining App [{0}, {1}] **", platform, version);
			Console.Write("   Infile:  '{0}' ({1:#,##0.00 MB})", infile, sizeinMB);
			Console.WriteLine();
		}

	}//class
}//namespace
