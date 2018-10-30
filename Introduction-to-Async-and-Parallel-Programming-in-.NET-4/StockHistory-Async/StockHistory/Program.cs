using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.Net;
using System.Threading.Tasks;


//
// This app takes one stock symbol, downloads historial data, and does some
// simple analysis:
//
//    min price
//    max price
//    avg price
//    standard deviation
//    standard error
//
// This version uses 3 web sites (yahoo, nasdaq, and msn) for redundancy.  The
// sites are accessed asynchronously, and the data presented is from the first
// web site that responds; we use the async web methods BeginGetResponse
// and EndGetResponse to implement this functionality.  
//
// Usage:  StockHistory.exe  msft
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
			// Process stock symbol:
			//
		    char[] separators = {','};
		    string[] symbols = symbolsAsCSV.Split(separators);
            List<Task>  tasks = new List<Task>();
		    foreach (string symbol in symbols)
		    {
		        Task t = Task.Factory.StartNew((arg) =>
		        {
		            ProcessStockSymbol((string)arg, numYearsOfHistory);
		        }, symbol);
                tasks.Add(t);
		    }

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
		        Task t_error = Task.Factory.StartNew(() =>
		        {
		            int i = 10;
		            int j = 10 / i;
		        });

		        StockData data = DownloadData.GetHistoricalData(symbol, numYearsOfHistory);

		        int N = data.Prices.Count;

		        Task<decimal> t_min = Task.Factory.StartNew(() =>
		        {
		            decimal min = data.Prices.Min();
		            return min;
		        });
		        Task<decimal> t_max = Task.Factory.StartNew(() =>
		        {
		            decimal max = data.Prices.Max();
		            return max;
		        });
		        Task<decimal> t_avg = Task.Factory.StartNew(() =>
		        {
		            decimal avg = data.Prices.Average();
		            return avg;
		        });

		        // Standard deviation:

		        Task<double> t_stddev = Task.Factory.StartNew(() =>
		        {

		            double sum = 0.0;
		            decimal l_avg = data.Prices.Average();
		            foreach (decimal value in data.Prices)
		                sum += Math.Pow(Convert.ToDouble(value - l_avg), 2.0);
		            int i = 10;
		            int j = 10 / i;

		            double stddev = Math.Sqrt(sum / N);
		            return stddev;
		        });

		        // Standard error:
		        Task<double> t_stderr = t_stddev.ContinueWith((antecedent) =>
		        {
		            //t_stddev.Wait();
		            double stderr = antecedent.Result / Math.Sqrt(N);
		            return stderr;
		        });
		        //
		        // Output:
		        //
		        //t_min.Wait();
		        //t_max.Wait();
		        //t_avg.Wait();
		        //t_stddev.Wait();
		        //t_stderr.Wait();
		        Task[] tasks = {t_min, t_max, t_avg, t_stddev, t_stderr, t_error};
		        Task.WaitAll(tasks);
		        //lock (Console.Out)
		        //{
		        //    Console.WriteLine();
		        //    Console.WriteLine("** {0} **", symbol);
		        //    Console.WriteLine("   Data source:  '{0}'", data.DataSource);
		        //    Console.WriteLine("   Data points:   {0:#,##0}", N);
		        //    Console.WriteLine("   Min price:    {0:C}", t_min.Result);
		        //    Console.WriteLine("   Max price:    {0:C}", t_max.Result);
		        //    Console.WriteLine("   Avg price:    {0:C}", t_avg.Result);
		        //    Console.WriteLine("   Std dev/err:   {0:0.000} / {1:0.000}", t_stddev.Result, t_stderr.Result);
		        //}

		        decimal min0 = t_min.Result;
		        decimal max0 = t_max.Result;
		        decimal avg0 = t_avg.Result;
		        double stddev0 = t_stddev.Result;
		        double stderr0 = t_stderr.Result;

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
		        output = string.Format("{0}\n   Min price:    {1:C}", output, min0);
		        output = string.Format("{0}\n   Max price:    {1:C}", output, max0);
		        output = string.Format("{0}\n   Avg price:    {1:C}", output, avg0);
		        output = string.Format("{0}\n   Std dev/err:   {1:0.000} / {2:0.000}", output, stddev0, stderr0);

		        Console.WriteLine(output);		    }
		    catch (AggregateException ae)
		    {
		        //lock (Console.Out)
		        //{
		        //    Console.WriteLine();
		        //    ae = ae.Flatten();
		        //    foreach (var ex in ae.InnerExceptions)
		        //    {
		        //        Console.WriteLine("Tasking Error: {0}", ex.Message);
		        //    }
		        //}
		        string output = string.Format("\n** {0} **", symbol);

		        ae = ae.Flatten();  // could have a tree of exceptions, so flatten first:
		        foreach (Exception ex in ae.InnerExceptions)
		            output = string.Format("{0}\nError: {1}", output, ex.Message);
          
		        Console.WriteLine(output);
		    }
			catch (Exception ex)
			{
			    //lock (Console.Out)
			    //{
			    //    Console.WriteLine();
			    //    Console.WriteLine("** {0} **", symbol);
			    //    Console.WriteLine("Error: {0}", ex.Message);
			    //}
			    string output = string.Format("\n** {0} **\nError: {1}", symbol, ex.Message);
			    Console.WriteLine(output);
			}
		}


		/// <summary>
		/// Processes command-line arguments, and outputs to the user.
		/// </summary>
		/// 
		static void ProcessCmdLineArgs(string[] args, out string version, out string platform, out string symbol, out int numYearsOfHistory)
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

			symbol = "";  // in case user does not supply:
			numYearsOfHistory = 10;

			string usage = "Usage: StockHistory.exe [-? /? symbol ]";

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
					symbol = arg;
				}
			}//for

			if (symbol == "")
			{
				Console.WriteLine();
				Console.Write("Please enter stock symbol (e.g. 'msft'): ");
				symbol = Console.ReadLine();
			}

			symbol = symbol.Trim();  // delete any leading/trailing spaces:
			if (symbol == "")
			{
				Console.WriteLine();
				Console.WriteLine("** Error: you must enter a stock symbol, e.g. 'msft'");
				Console.WriteLine(usage);
				Console.WriteLine();
				System.Environment.Exit(-1);
			}

			Console.WriteLine();
			Console.WriteLine("** Async Stock History App [{0}, {1}] **", platform, version);
			Console.WriteLine("   Stock symbol:     {0}", symbol);
			Console.WriteLine("   Time period:      last {0} years", numYearsOfHistory);
			Console.WriteLine("   Internet access?  {0}", DownloadData.IsConnectedToInternet());
		}

	}//class
}//namespace
