using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace ChangeManagementSystem.ConsoleTests
{
    class ConcurrencyTest
    {
        static int numberOfThreads = 2;
        static Barrier _barrier = new Barrier(numberOfThreads);

        public void signal()
        {
            _barrier.SignalAndWait();
        }
    }
}
