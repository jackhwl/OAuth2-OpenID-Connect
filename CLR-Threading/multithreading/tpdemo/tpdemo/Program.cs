using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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
            for (int n = 0; n < 10; n++)
            {
                ThreadPool.QueueUserWorkItem(SayHello, n);
            }

            Thread.Sleep(rng.Next(1500, 2000));
            Console.WriteLine("[{0}] Main done", Thread.CurrentThread.ManagedThreadId);
            Console.ReadLine();
        }

        static Random rng = new Random();

        static void SayHello(object arg)
        {
            Thread.Sleep(rng.Next(250, 500));
            int n = (int) arg;
            Console.WriteLine("[{0}] Hello, world {1}! ({2})",
                Thread.CurrentThread.ManagedThreadId,
                n,
                Thread.CurrentThread.IsBackground);
        }
    }
}
