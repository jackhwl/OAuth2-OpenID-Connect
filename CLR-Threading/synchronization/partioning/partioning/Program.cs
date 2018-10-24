using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace partioning
{
    class Program
    {
        static void Main()
        {
            // Determine how many cores/processors there are on this machine.
            //
            int coreCount = Environment.ProcessorCount;

            Console.WriteLine("Process/core count = {0}", coreCount);

            // Get some data to work with.
            //
            double[] data = GetData();

            Stopwatch sw = Stopwatch.StartNew();

            // Process the entire data set using one thread
            // for comparison purposes.
            //
            ArrayProcessor wholeArray = new ArrayProcessor(data, 0, data.Length - 1);
            wholeArray.ComputeSum();
            sw.Stop();

            Console.WriteLine(
                "1 thread computed {0:n0} in {1:n0} ms",
                wholeArray.Sum, sw.ElapsedMilliseconds
            );
            Console.ReadLine();
        }

        static double[] GetData()
        {
            double[] data = new double[5000];

            for (int n = 0; n < data.Length; n++)
            {
                data[n] = n;
            }

            return (data);
        }
    }
}
