using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace tpdemo
{
    class Program
    {
        static void Main()
        {
            Console.WriteLine("[{0}] Main called", Thread.CurrentThread.ManagedThreadId);

            Thread t = new Thread(SayHello);
            t.IsBackground = true;
            t.Start(10);
            t.Join();

            Console.WriteLine("[{0}] Main done", Thread.CurrentThread.ManagedThreadId);
            Console.ReadLine();
        }

        static void SayHello(object arg)
        {
            int iterations = (int)arg;

            for (int n = 0; n < iterations; n++)
            {
                Console.WriteLine("[{0}] Hello, world {1}! ({2})",
                    Thread.CurrentThread.ManagedThreadId,
                    n,
                    Thread.CurrentThread.IsBackground);
            }
        }
    }
}
