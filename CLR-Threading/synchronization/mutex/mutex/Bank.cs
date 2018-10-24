using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace mutex
{
// The following constants control the simulation performed
// in this activity.
//
    class SimulationParameters
    {
        public const double INITIAL_DEPOSIT = 1000; // Dollar amount to initialize each account with.
        public const double MIN_TRANSFER_AMOUNT = 25; // Minimum dollar amount to transfer between accounts.
        public const double MAX_TRANSFER_AMOUNT = 250; // Maximum dollar amount to transfer between accounts.
        public const int NUMBER_OF_ACCOUNTS = 10; // Number of accounts in the "bank".
        public const int NUMBER_OF_TRANSFER_THREADS = 4; // Number of threads transfering funds around between accounts.

        public const int
            TRANSFER_THREAD_PERIOD = 200; // Amount of time (in MS) each transfer thread sleeps between transfers.

        public const int SIMULATION_LENGTH = 10000; // Duration (in MS) of entire simulation.
    }

    class Bank
    {
        static BankAccount[] s_bankAccounts = new BankAccount[SimulationParameters.NUMBER_OF_ACCOUNTS];
        static volatile bool s_simulationOver = false;

        static void Main()
        {
            Thread.CurrentThread.Name = "Main";

            Console.WriteLine("[Main] Starting program.  Total funds on deposit should always be {0:C0}",
                SimulationParameters.NUMBER_OF_ACCOUNTS * SimulationParameters.INITIAL_DEPOSIT);

            // An array of references to Account objects (bankAccounts) is
            // initialized, with each account starting out with the
            // same amount of money (INITIAL_DEPOSIT).
            //
            for (int n = 0; n < SimulationParameters.NUMBER_OF_ACCOUNTS; n++)
            {
                s_bankAccounts[n] = new BankAccount(n, SimulationParameters.INITIAL_DEPOSIT);
            }

            // The array below will hold references to the threads
            // that are doing the funds transfers.
            //
            Thread[] transferThreads = new Thread[SimulationParameters.NUMBER_OF_TRANSFER_THREADS];
            ThreadStart threadProc = new ThreadStart(TransferThreadProc);

            // Start the transfer threads.
            //
            for (int n = 0; n < SimulationParameters.NUMBER_OF_TRANSFER_THREADS; n++)
            {
                transferThreads[n] = new Thread(threadProc);
                transferThreads[n].Name = string.Format("TX-{0}", n);
                transferThreads[n].Start();
            }

            // Let the simulation run the prescribed amount of time.
            //
            Thread.Sleep(SimulationParameters.SIMULATION_LENGTH);

            // Signal to everyone that the simulation is over.
            //
            Console.WriteLine("[Main] Shutting down simulation.");
            s_simulationOver = true;

            // Wait for everyone to acknowledge the simulation is complete.
            //
            for (int n = 0; n < transferThreads.Length; n++)
            {
                transferThreads[n].Join();
            }

            // Perform consistency check.
            //
            Console.WriteLine("[Main] Simulation complete, verifying accounts.");
            VerifyAccounts();
        }

        // TransferThreadProc
        //
        // This method represents the code for the threads that
        // perform transfers from one account to another throughout
        // the life of the simulation.
        //
        static void TransferThreadProc()
        {
            string threadName = Thread.CurrentThread.Name;

            while (!s_simulationOver)
            {
                // Choose a random transfer amount.
                //
                double transferAmount = GetRandomTranferAmount();

                // Randomly choose two accounts to transfer funds between.
                //
                int debitAccount = GetRandomAccountIndex();
                int creditAccount = GetRandomAccountIndex();

                // Make sure we're actually transferring money
                // between different accounts.
                //
                while (creditAccount == debitAccount)
                {
                    creditAccount = GetRandomAccountIndex();
                }

                // Transfer funds between the two chosen accounts.
                //
                s_bankAccounts[creditAccount].TransferFrom(s_bankAccounts[debitAccount], transferAmount);

                Thread.Sleep(SimulationParameters.TRANSFER_THREAD_PERIOD);
            }
        }

        // VerifyAccounts
        //
        // This method verifies that the total amount of money "on deposit"
        // in our simulation is consistent.  In other words, we should find
        // that the total funds on deposit at the end of the simulation
        // is the same as it was when the program started (because we're
        // just shuffling funds between accounts in the same bank).
        // If we detect otherwise, we have a problem.
        //
        static void VerifyAccounts()
        {
            string threadName = Thread.CurrentThread.Name;
            double totalDepositsIfNoErrors =
                SimulationParameters.INITIAL_DEPOSIT * SimulationParameters.NUMBER_OF_ACCOUNTS;
            double totalDeposits = 0;

            // Iterate over accounts, adding each account's balance
            // to a running total.
            //
            for (int n = 0; n < SimulationParameters.NUMBER_OF_ACCOUNTS; n++)
            {
                totalDeposits += s_bankAccounts[n].Balance;
            }

            // Display the results of this audit.
            //
            if (totalDeposits == totalDepositsIfNoErrors)
            {
                Console.WriteLine("[{0}] Audit result: bank accounts are consistent ({1:C0} on deposit)",
                    threadName, totalDeposits);
            }
            else
            {
                Console.WriteLine("[{0}] Audit result: *** inconsistencies detected ({1:C0} total deposits)",
                    threadName, totalDeposits);
            }

            Console.ReadLine();
        }

        // GetRandomAccountIndex
        //
        // Returns a random number between 0 and (NUMBER_OF_ACCOUNTS - 1).  The returned
        // value will be used as an index into bankAccounts to determine which
        // account to modify.
        //
        static Random rngAccountIndices = new Random((int) DateTime.Now.Ticks);

        static int GetRandomAccountIndex()
        {
            lock (rngAccountIndices)
            {
                return rngAccountIndices.Next(SimulationParameters.NUMBER_OF_ACCOUNTS - 1);
            }
        }

        // GetRandomTranferAmount
        //
        // Returns a random number representing between MIN_TRANSFER_AMOUNT and
        // MAX_TRANSFER_AMOUNT representing the amount of money to transfer
        // between any two accounts.
        //
        static Random rngTransferAmounts = new Random((int) DateTime.Now.Ticks + 13);

        static double GetRandomTranferAmount()
        {
            lock (rngTransferAmounts)
            {
                return rngTransferAmounts.Next((int) SimulationParameters.MIN_TRANSFER_AMOUNT,
                    (int) SimulationParameters.MAX_TRANSFER_AMOUNT);
            }
        }
    }
}
