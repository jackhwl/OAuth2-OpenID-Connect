using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

//
// Creates 100 long-running tasks to see how .NET 4 responds.  In this case,
// we create 100 standard tasks all at once, which causes worker thread pool
// to grow to N threads --- and CPU to thrash due to over-subscription and 
// too many context switches.  Not to mention cost of creating these extra
// threads, and memory for their stack space...
//
// To run:  run without debugging (ctrl+F5), open task manager, view processes,
// and add "threads" column to the view.  Watch number of threads grow, slowly
// but surely.
//
namespace LongRunning
{
  class Program
  {

    public static void Main(string[] args)
    {
			int N = 100;
			int durationInMins = 0;
			int durationInSecs = 5;

			Welcome(N, durationInMins, durationInSecs);

			//
			// Create 100 tasks all at once, and then wait for them to finish:
			//
			List<Task> tasks = new List<Task>();
        int numCores = System.Environment.ProcessorCount;
			for (int i = 0; i < numCores; i++)
			{
				Task t = CreateOneLongRunningTask(durationInMins, durationInSecs, TaskCreationOptions.None);
				tasks.Add(t);
			}

			//Task.WaitAll(tasks.ToArray());
        while (tasks.Count > 0)
        {
            int index = Task.WaitAny(tasks.ToArray());
            tasks.RemoveAt(index);
            N--;
            if (N > 0)
            {
                Task t = CreateOneLongRunningTask(durationInMins, durationInSecs, TaskCreationOptions.None);
                tasks.Add(t);
            }
        }

			//
			// done:
			//
			Console.WriteLine();
			Console.WriteLine();
			Console.WriteLine("** Done!");
		}


		//
		// chews on CPU for give mins and secs:
		//
		static Task CreateOneLongRunningTask(int durationInMins, int durationInSecs, TaskCreationOptions options)
		{
			long durationInMilliSecs = durationInMins * 60 * 1000;
			durationInMilliSecs += (durationInSecs * 1000);

			Task t = Task.Factory.StartNew(() =>
				{
				  Console.WriteLine("starting task...");
				  
					var sw = System.Diagnostics.Stopwatch.StartNew();
					long count = 0;

					while (sw.ElapsedMilliseconds < durationInMilliSecs)
					{
						count++;
						if (count == 1000000000)
							count = 0;
					}
					
				  Console.WriteLine("task finished.");
				}, 
				options
			);

			return t;
		}


		//
		// Welcome the user:
		//
		static void Welcome(int N, int durationInMins, int durationInSecs)
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

			Console.WriteLine("** Long-running Tasks App -- All at once [{0}, {1}] **", platform, version);
			Console.WriteLine("   Number of tasks: {0:#,##0}", N);
			Console.WriteLine("   Number of cores: {0:#,##0}", System.Environment.ProcessorCount);
			Console.WriteLine("   Task duration:   {0:#,##0} mins, {1:#,##0} secs", durationInMins, durationInSecs);
			Console.WriteLine();
		}

   }//class
}//namespace
