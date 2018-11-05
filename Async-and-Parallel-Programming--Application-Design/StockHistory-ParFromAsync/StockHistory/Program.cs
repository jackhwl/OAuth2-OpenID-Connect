using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.Net;
using System.Threading.Tasks;


//
// This app takes one or more stock symbols, downloads historial data, and does some
// simple analysis in parallel:
//
//    min price
//    max price
//    avg price
//    standard deviation
//    standard error
//
// This version uses 3 web sites (yahoo, nasdaq, and msn) for redundancy, and
// downloads the data in parallel --- the data displayed is from the first 
// site that returns.  In this version we use the async web methods BeginGetResponse
// and EndGetResponse to access the web sites, with a Task-based facade on top.
// The Async methods have the advantages that (1) worker threads are not dedicated to 
// request (i.e. different threads can initiate vs. handle callback), and (2) easier 
// to cancel outstanding requests.
//
// Usage:  StockHistory.exe  msft,intc,...
//

namespace StockHistory
{
	class Program
	{

		static void Main(string[] args)
		{
			String version, platform, symbolsAsCSV;
			int numYearsOfHistory;

			ProcessCmdLineArgs(args, out version, out platform, out symbolsAsCSV, out numYearsOfHistory);

			//
			// Process stock symbols(s), in parallel:
			//
			char[] separators = { ',' };
			string[] symbols = symbolsAsCSV.Split(separators);

			List<Task> tasks = new List<Task>();

			foreach (string SYMBOL in symbols)
			{
				Task t = Task.Factory.StartNew((arg) =>
				{
					string symbol = (string)arg;
					ProcessStockSymbol(symbol, numYearsOfHistory);
				},

					SYMBOL  // pass symbol as an arg so task receives proper value to process:
				);

				tasks.Add(t);
			}

			// 
			// Wait for tasks to complete:
			//
			Task.WaitAll(tasks.ToArray());

			Console.WriteLine();
			Console.WriteLine("** Done **");
			Console.WriteLine();

			Console.Write("\n\nPress a key to exit...");
			Console.ReadKey();
		}


		/// <summary>
		/// Downloads and processes historical data for given stock symbol.
		/// </summary>
		/// <param name="symbol">stock symbol, e.g. "msft"</param>
		/// <param name="numYearsOfHistory">years of history > 0, e.g. 10</param>
		private static void ProcessStockSymbol(string symbol, int numYearsOfHistory)
		{
			try
			{
				StockData data = DownloadData.GetHistoricalData(symbol, numYearsOfHistory);

				int N = data.Prices.Count;

				Task<decimal> T_min = Task.Factory.StartNew<decimal>(() =>
					{
						return data.Prices.Min();
					}
				);

				Task<decimal> T_max = Task.Factory.StartNew<decimal>(() =>
					{
						return data.Prices.Max();
					}
				);

				Task<decimal> T_avg = Task.Factory.StartNew<decimal>(() =>
					{
						return data.Prices.Average();
					}
				);

				Task<double> T_stddev = Task.Factory.StartNew(() =>
					{
						// Standard deviation:
						double sum = 0.0;
						decimal l_avg = data.Prices.Average();

						foreach (decimal value in data.Prices)
							sum += Math.Pow(Convert.ToDouble(value - l_avg), 2.0);

						//
						// NOTE: want to test exception handling?  Uncomment the following to trigger
						// divide-by-zero, and notice you get 2 task exceptions, one from this code
						// and one from the continuation task (stderr) that fails because we fail.
						//
						//int abc = 0;
						//int x = 1000 / abc;

						return Math.Sqrt(sum / N);
					}
				);

				Task<double> T_stderr = T_stddev.ContinueWith((antecedent) =>
					{
						return antecedent.Result / Math.Sqrt(N);
					}
				);

				//
				// Wait and harvest results when done:
				//
				// NOTE: even though WaitAll is not required for correctness (calls to .Result do
				// an implicit .Wait), we use WaitAll for efficiency so that we process tasks in
				// order as they finish (versus an arbitrary order implied by calls to .Result).
				//
				try
				{
					Task.WaitAll(new Task[] { T_min, T_max, T_avg, T_stddev, T_stderr });
				}
				catch (AggregateException)  // tasking errors:
				{
					throw;  // re-throw, output error messages below:
				}

				decimal min = T_min.Result;
				decimal max = T_max.Result;
				decimal avg = T_avg.Result;
				double stddev = T_stddev.Result;
				double stderr = T_stderr.Result;

				//
				// Output:
				//
				// NOTE: we build a single output string and then write to the console in one op.
				// Otherwise, if we output one value at a time, our output might get intermixed
				// with the output from another, parallel task.  Note that Console class is thread-
				// safe, so this works as long as we make only a single call for our output.
				//
				string output = string.Format("\n** {0} **", symbol);
				output = string.Format("{0}\n   Data source:  '{1}'", output, data.DataSource);
				output = string.Format("{0}\n   Data points:   {1:#,##0}", output, N);
				output = string.Format("{0}\n   Min price:    {1:C}", output, min);
				output = string.Format("{0}\n   Max price:    {1:C}", output, max);
				output = string.Format("{0}\n   Avg price:    {1:C}", output, avg);
				output = string.Format("{0}\n   Std dev/err:   {1:0.000} / {2:0.000}", output, stddev, stderr);

				Console.WriteLine(output);
			}
			catch (AggregateException ae)
			{
				string output = string.Format("\n** {0} **", symbol);

        ae = ae.Flatten();  // could have a tree of exceptions, so flatten first:
        foreach (Exception ex in ae.InnerExceptions)
          output = string.Format("{0}\nError: {1}", output, ex.Message);
          
        Console.WriteLine(output);
			}
			catch (Exception ex)
			{
				string output = string.Format("\n** {0} **\nError: {1}", symbol, ex.Message);
        Console.WriteLine(output);
			}
		}


		/// <summary>
		/// Processes command-line arguments, and outputs to the user.
		/// </summary>
		/// 
		static void ProcessCmdLineArgs(string[] args, out string version, out string platform, out string symbols, out int numYearsOfHistory)
		{
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

			symbols = "";  // in case user does not supply:
			numYearsOfHistory = 10;

			string usage = "Usage: StockHistory.exe [-? /? symbol[,symbol,symbol,...] ]";

			if (args.Length > 1)
			{
				Console.WriteLine("** Error: incorrect number of arguments (found {0}, expecting 1)", args.Length);
				Console.WriteLine(usage);
				System.Environment.Exit(-1);
			}

			for (int i = 0; i < args.Length; i++)
			{
				string arg = args[i];

				if (arg == "-?" || arg == "/?")
				{
					Console.WriteLine(usage);
					System.Environment.Exit(-1);
				}
				else  // assume arg is stock symbol:
				{
					symbols = arg;
				}
			}//for

			if (symbols == "")
			{
				Console.WriteLine();
				Console.Write(">> Please enter stock symbol(s) (e.g. 'msft,intc,...'): ");
				symbols = Console.ReadLine();
			}

			symbols = symbols.Replace(" ", "");  // delete all spaces...
			if (symbols == "")
			{
				Console.WriteLine();
				Console.WriteLine("** Error: you must enter at least one stock symbol, e.g. 'msft'");
				Console.WriteLine(usage);
				Console.WriteLine();
				System.Environment.Exit(-1);
			}

			Console.WriteLine();
			Console.WriteLine("** Parallel CSV Stock History App [{0}, {1}] **", platform, version);
			Console.WriteLine("   Stock symbol(s):  '{0}'", symbols);
			Console.WriteLine("   Time period:       last {0} years", numYearsOfHistory);
			Console.WriteLine("   Internet access?   {0}", DownloadData.IsConnectedToInternet());
		}

	}//class
}//namespace
