using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace mtadd
{
    class Program
    {
        // this version only works(sum=10) when run in single core mathine
        static int sum = 0;

        static void Main()
        {
            Thread[] threads = new Thread[10];

            for (int n = 0; n < threads.Length; n++)
            {
                threads[n] = new Thread(AddOne);
                threads[n].Start();
            }

            for (int n = 0; n < threads.Length; n++)
            {
                threads[n].Join();
            }

            Console.WriteLine(
                "[{0}] sum = {1}",
                Thread.CurrentThread.ManagedThreadId,
                sum
            );
            Console.ReadLine();
        }

#if (false)
// Buggy version.
    static void AddOne()
    {
        Console.WriteLine("[{0}] AddOne called",
                          Thread.CurrentThread.ManagedThreadId);
        int temp = sum;
        temp++;
        Thread.Sleep(1);
        sum = temp;
    }
#else
        // Thread-safe version.
        static void AddOne()
        {
            Console.WriteLine("[{0}] AddOne called",
                Thread.CurrentThread.ManagedThreadId);
            sum++;
            //Interlocked.Increment(ref sum);
        }
#endif
    }
}
