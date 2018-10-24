using ConcurrencyTestingEFCore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ConcurrencyTestingEFCore
{
    public delegate void MyThreadingDelegate(Object s);

    public class App
    {
        static int numberOfThreads = 2;

        #region ctor
        public App() { }
        #endregion

        #region Main Method to Invoke Tests

        public void Run()
        {
            PrintMenu();

            while (true)
            {
                var key = Console.ReadKey();
                Console.WriteLine();
                if (key.Key == ConsoleKey.Escape)
                {
                    Environment.Exit(0);
                }
                else
                {
                    switch (key.Key)
                    {
                        case ConsoleKey.D1:
                            Run((MyThreadingDelegate)ReadUncommitted);
                            break;
                        case ConsoleKey.D2:
                            Run((MyThreadingDelegate)ReadCommitted);
                            break;
                        case ConsoleKey.D3:
                            Run((MyThreadingDelegate)RepeatableRead);
                            break;
                        case ConsoleKey.D4:
                            Run((MyThreadingDelegate)Serializable);
                            break;
                        case ConsoleKey.D5:
                            Run((MyThreadingDelegate)Snapshot);
                            break;
                        case ConsoleKey.D6:
                            Run((MyThreadingDelegate)RepeatableReadWithUPDLOCK);
                            break;
                        case ConsoleKey.D7:
                            Run((MyThreadingDelegate)SerializableWithUPDLOCK);
                            break;
                        case ConsoleKey.D8:
                            Run((MyThreadingDelegate)SerializableWithDeadlockRetryLogic);
                            break;
                    }
                }
            }
        }

        public void Run(MyThreadingDelegate methodToCall)
        {
            // create record to update
            TestItem item = new TestItem
            {
                Name = "New Item: " + Guid.NewGuid().ToString(),
                Value = 0,
                Modified = DateTime.Now,
                ModifiedBy = "User0"
            };

            AppDataContext _context = new AppDataContext();
            //_context.Database.Migrate();
            // uncomment to enable logging Entity Framework SQL to Console Window.
            // _context.GetService<ILoggerFactory>().AddProvider(new MyLoggerProvider());

            _context.TestItems.Add(item);
            _context.SaveChanges();

            Console.WriteLine("Test Record Created: Id={0}, Value={1}", item.Id, item.Value);

            int recordIdToTest = item.Id;

            // CountdownEvent is used so each thread can signal when it's work is complete
            using (var countdownEvent = new CountdownEvent(numberOfThreads))
            {

                for (int i = 1; i <= numberOfThreads; i++)
                {
                    // create a new thread to simulate multiple users accessing record simultaneously
                    ThreadPool.QueueUserWorkItem(new WaitCallback(methodToCall), new TestInput(recordIdToTest, "user" + i, countdownEvent));
                }

                // block until all threads signal that they've completed
                countdownEvent.Wait();
            }

            try
            {
                // retrieve record with new DbContext so previously created (tracked) item isn't returned
                using (var context = new AppDataContext())
                {
                    
                    TestItem itemUpdated = context.TestItems.Find(recordIdToTest);

                    Console.WriteLine("{0} Last Updated Record, Value = {1}", itemUpdated.ModifiedBy, itemUpdated.Value);
                    Console.WriteLine();

                }

                // print menu each time the threads complete
                PrintMenu();

            }
            catch (Exception ex)
            {
                Console.WriteLine(GetExceptionMessage(ex));
            }
        }

        #region PrintMenu
        public void PrintMenu()
        {
            Console.WriteLine("Please choose Isolation Level:");
            Console.WriteLine("1. ReadUncommitted");
            Console.WriteLine("2. ReadCommitted");
            Console.WriteLine("3. RepeatableRead");
            Console.WriteLine("4. Serializable");
            Console.WriteLine("5. Snapshot");
            Console.WriteLine("6. RepeatableRead with UPDLOCK");
            Console.WriteLine("7. Serializable with UPDLOCK");
            Console.WriteLine("8. Serializable with deadlock retry logic");
            Console.WriteLine("ESC End Program");
            Console.WriteLine("--------------------------------------------");
        }

        #endregion

        #endregion

        #region shared get methods for TaskItem

        private async Task<TestItem> GetItem(int id, AppDataContext _context)
        {
            return await _context.TestItems.FirstAsync(i => i.Id == id);
        }

        private async Task<TestItem> GetItemWithUPDLOCK(int id, AppDataContext _context)
        {
            return await _context.TestItems
                .FromSql("SELECT Id, Name, Value, Modified, ModifiedBy FROM TESTITEMS WITH (UPDLOCK) WHERE Id = " + id)
                .FirstAsync();
        }

        #endregion

        #region ReadUncommitted

        public void ReadUncommitted(Object stateInfo)
        {

            TestInput testData = (TestInput)stateInfo;

            Console.WriteLine("{0} Running Transaction: IsolationLevel.ReadUncommitted", testData.UserName);

            using (var _context = new AppDataContext())
            {
                using (var transaction = _context.Database.BeginTransaction(System.Data.IsolationLevel.ReadUncommitted))
                {
                    try
                    {
                        TestItem item = _context.TestItems.Find(testData.RecordId);
                        int originalValue = item.Value;

                        Console.WriteLine("{0} has read record {1}", testData.UserName, item.Id);

                        item.Value += 2;
                        item.ModifiedBy = testData.UserName;
                        item.Modified = DateTime.Now;

                        Console.WriteLine("{0} is updating record {1}, setting Value:{2} + 2", testData.UserName, item.Id, originalValue);

                        // now try to update the read lock to an exclusive lock
                        _context.SaveChanges();

                        Console.WriteLine("{0} about to Commit Transaction", testData.UserName);

                        transaction.Commit();

                        Console.WriteLine("{0} has Committed Transaction", testData.UserName);

                    }
                    catch (System.Data.SqlClient.SqlException ex)
                    {
                        Console.WriteLine("{0} - Error Info: {1}", testData.UserName, GetExceptionMessage(ex));

                        try
                        {
                            transaction.Rollback();

                        }
                        catch (Exception ex1)
                        {
                            Console.WriteLine("{0} - Error trying to roll back transaction: {1}", testData.UserName, GetExceptionMessage(ex1));
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("{0} Transaction failed. Error Info: {1}", testData.UserName, GetExceptionMessage(ex));
                        try
                        {
                            transaction.Rollback();
                            Console.WriteLine("{0} Transaction Rolled Back", testData.UserName);

                        }
                        catch (Exception ex1)
                        {
                            Console.WriteLine("{0} - Error Info: ", testData.UserName, GetExceptionMessage(ex1));
                        }
                    }
                    finally
                    {
                        testData.countDownEvent.Signal();
                    }
                }
            }
        }
         
        #endregion

        #region ReadCommitted

        /// <summary>
        /// Note: when creating the database with Code First, EF Core sets the following on the database:
        /// ALTER DATABASE ConcurrencyTestingEFCoreDb SET READ_COMMITTED_SNAPSHOT ON;
        /// </summary>
        /// <param name="stateInfo"></param>
        public void ReadCommitted(Object stateInfo)
        {
            TestInput testData = (TestInput)stateInfo;

            Console.WriteLine("{0} Running Transaction: IsolationLevel.ReadCommitted", testData.UserName);

            using (var _context = new AppDataContext())
            {
                using (var transaction = _context.Database.BeginTransaction(System.Data.IsolationLevel.ReadCommitted))
                {
                    try
                    {
                        TestItem item = _context.TestItems.Find(testData.RecordId);
                        int originalValue = item.Value;
                        Console.WriteLine("{0} has read record {1}", testData.UserName, item.Id, item.Value);

                        item.Value += 2;
                        item.ModifiedBy = testData.UserName;
                        item.Modified = DateTime.Now;

                        Console.WriteLine("{0} is updating record {1}, setting Value:{2} + 2", testData.UserName, item.Id, originalValue);

                        // now try to update the read lock to an exclusive lock
                        _context.SaveChanges();

                        Console.WriteLine("{0} about to Commit Transaction", testData.UserName);

                        transaction.Commit();

                        Console.WriteLine("{0} has Committed Transaction", testData.UserName);

                    }
                    catch (System.Data.SqlClient.SqlException ex)
                    {
                        Console.WriteLine("{0} - Error Info: {1}", testData.UserName, GetExceptionMessage(ex));

                        try
                        {
                            transaction.Rollback();

                        }
                        catch (Exception ex1)
                        {
                            Console.WriteLine("{0} - Error trying to roll back transaction: {1}", testData.UserName, GetExceptionMessage(ex1));
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("{0} Transaction failed. Error Info: {1}", testData.UserName, GetExceptionMessage(ex));
                        try
                        {
                            transaction.Rollback();
                            Console.WriteLine("{0} Transaction Rolled Back", testData.UserName);

                        }
                        catch (Exception ex1)
                        {
                            Console.WriteLine("{0} - Error Info: ", testData.UserName, GetExceptionMessage(ex1));
                        }
                    }
                    finally
                    {
                        testData.countDownEvent.Signal();
                    }
                }
            }
        }

        #endregion

        #region RepeatableRead

        public void RepeatableRead(Object stateInfo)
        {
            TestInput testData = (TestInput)stateInfo;

            Console.WriteLine("{0} Running Transaction: IsolationLevel.RepeatableRead", testData.UserName);

            using (var _context = new AppDataContext())
            {
                using (var transaction = _context.Database.BeginTransaction(System.Data.IsolationLevel.RepeatableRead))
                {
                    try
                    {
                        TestItem item = _context.TestItems.Find(testData.RecordId);
                        int originalValue = item.Value;
                        Console.WriteLine("{0} has read record {1}", testData.UserName, item.Id, item.Value);

                        item.Value += 2;
                        item.ModifiedBy = testData.UserName;
                        item.Modified = DateTime.Now;

                        Console.WriteLine("{0} is updating record {1}, setting Value:{2} + 2", testData.UserName, item.Id, originalValue);

                        // now try to update the read lock to an exclusive lock
                        _context.SaveChanges();

                        Console.WriteLine("{0} about to Commit Transaction", testData.UserName);

                        transaction.Commit();

                        Console.WriteLine("{0} has Committed Transaction", testData.UserName);

                    }
                    catch (System.Data.SqlClient.SqlException ex)
                    {
                        Console.WriteLine("{0} - Error Info: {1}", testData.UserName, GetExceptionMessage(ex));

                        try
                        {
                            transaction.Rollback();

                        }
                        catch (Exception ex1)
                        {
                            Console.WriteLine("{0} - Error trying to roll back transaction: {1}", testData.UserName, GetExceptionMessage(ex1));
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("{0} Transaction failed. Error Info: {1}", testData.UserName, GetExceptionMessage(ex));
                        try
                        {
                            transaction.Rollback();
                            Console.WriteLine("{0} Transaction Rolled Back", testData.UserName);

                        }
                        catch (Exception ex1)
                        {
                            Console.WriteLine("{0} - Error Info: ", testData.UserName, GetExceptionMessage(ex1));
                        }
                    }
                    finally
                    {
                        testData.countDownEvent.Signal();
                    }
                }
            }
        }

        #endregion

        #region RepeatableRead with UPDLOCK

        public void RepeatableReadWithUPDLOCK(Object stateInfo)
        {
            TestInput testData = (TestInput)stateInfo;

            Console.WriteLine("{0} Running Transaction: IsolationLevel.RepeatableRead with UPDLOCK Table Hint", testData.UserName);

            using (var _context = new AppDataContext())
            {
                using (var transaction = _context.Database.BeginTransaction(System.Data.IsolationLevel.RepeatableRead))
                {
                    try
                    {
                        TestItem item = GetItemWithUPDLOCK(testData.RecordId, _context).Result;
                        int originalValue = item.Value;
                        Console.WriteLine("{0} has read record {1} with UPDLOCK", testData.UserName, item.Id, item.Value);

                        item.Value += 2;
                        item.ModifiedBy = testData.UserName;
                        item.Modified = DateTime.Now;

                        Console.WriteLine("{0} is updating record {1}, setting Value:{2} + 2", testData.UserName, item.Id, originalValue);

                        // now try to update the read lock to an exclusive lock
                        _context.SaveChanges();
                        Console.WriteLine("{0} about to Commit Transaction", testData.UserName);
                        transaction.Commit();
                        Console.WriteLine("{0} has Committed Transaction", testData.UserName);

                    }
                    catch (System.Data.SqlClient.SqlException ex)
                    {
                        Console.WriteLine("{0} - Error Info: {1}", testData.UserName, GetExceptionMessage(ex));

                        try
                        {
                            transaction.Rollback();

                        }
                        catch (Exception ex1)
                        {
                            Console.WriteLine("{0} - Error trying to roll back transaction: {1}", testData.UserName, GetExceptionMessage(ex1));
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("{0} Transaction failed. Error Info: {1}", testData.UserName, GetExceptionMessage(ex));
                        try
                        {
                            transaction.Rollback();
                            Console.WriteLine("{0} Transaction Rolled Back", testData.UserName);

                        }
                        catch (Exception ex1)
                        {
                            Console.WriteLine("{0} - Error Info: ", testData.UserName, GetExceptionMessage(ex1));
                        }
                    }
                    finally
                    {
                        testData.countDownEvent.Signal();
                    }
                }
            }
        }

        #endregion

        #region Serializable

        public void Serializable(Object stateInfo)
        {
            TestInput testData = (TestInput)stateInfo;

            Console.WriteLine("{0} Running Transaction: IsolationLevel.Serializable", testData.UserName);

            using (var _context = new AppDataContext())
            {
                using (var transaction = _context.Database.BeginTransaction(System.Data.IsolationLevel.Serializable))
                {
                    try
                    {
                        TestItem item = _context.TestItems.Find(testData.RecordId);
                        int originalValue = item.Value;
                        Console.WriteLine("{0} has read record {1}", testData.UserName, item.Id, item.Value);
                        
                        item.Value += 2;
                        item.ModifiedBy = testData.UserName;
                        item.Modified = DateTime.Now;

                        Console.WriteLine("{0} is updating record {1}, setting Value:{2} + 2", testData.UserName, item.Id, originalValue);

                        // now try to update the read lock to an exclusive lock
                        _context.SaveChanges();

                        Console.WriteLine("{0} about to Commit Transaction", testData.UserName);

                        transaction.Commit();

                        Console.WriteLine("{0} has Committed Transaction", testData.UserName);

                    }
                    catch (System.Data.SqlClient.SqlException ex)
                    {
                        Console.WriteLine("{0} - Error Info: {1}", testData.UserName, GetExceptionMessage(ex));

                        try
                        {
                            transaction.Rollback();

                        }
                        catch (Exception ex1)
                        {
                            Console.WriteLine("{0} - Error trying to roll back transaction: {1}", testData.UserName, GetExceptionMessage(ex1));
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("{0} Transaction failed. Error Info: {1}", testData.UserName, GetExceptionMessage(ex));
                        try
                        {
                            transaction.Rollback();
                            Console.WriteLine("{0} Transaction Rolled Back", testData.UserName);

                        }
                        catch (Exception ex1)
                        {
                            Console.WriteLine("{0} - Error Info: ", testData.UserName, GetExceptionMessage(ex1));
                        }
                    }
                    finally
                    {
                        testData.countDownEvent.Signal();
                    }
                }
            }
        }

        #endregion

        #region Serializable with UPDLOCK

        public void SerializableWithUPDLOCK(Object stateInfo)
        {
            TestInput testData = (TestInput)stateInfo;

            Console.WriteLine("{0} Running Transaction: IsolationLevel.Serializable with UPDLOCK Table Hint", testData.UserName);

            using (var _context = new AppDataContext())
            {
                using (var transaction = _context.Database.BeginTransaction(System.Data.IsolationLevel.Serializable))
                {
                    try
                    {
                        TestItem item = GetItemWithUPDLOCK(testData.RecordId, _context).Result;
                        int originalValue = item.Value;
                        Console.WriteLine("{0} has read record {1} with UPDLOCK", testData.UserName, item.Id, item.Value);

                        item.Value += 2;
                        item.ModifiedBy = testData.UserName;
                        item.Modified = DateTime.Now;

                        Console.WriteLine("{0} is updating record {1}, setting Value:{2} + 2", testData.UserName, item.Id, originalValue);

                        // now try to update the read lock to an exclusive lock
                        _context.SaveChanges();
                        Console.WriteLine("{0} about to Commit Transaction", testData.UserName);
                        transaction.Commit();
                        Console.WriteLine("{0} has Committed Transaction", testData.UserName);

                    }
                    catch (System.Data.SqlClient.SqlException ex)
                    {
                        Console.WriteLine("{0} - Error Info: {1}", testData.UserName, GetExceptionMessage(ex));

                        try
                        {
                            transaction.Rollback();

                        }
                        catch (Exception ex1)
                        {
                            Console.WriteLine("{0} - Error trying to roll back transaction: {1}", testData.UserName, GetExceptionMessage(ex1));
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("{0} Transaction failed. Error Info: {1}", testData.UserName, GetExceptionMessage(ex));
                        try
                        {
                            transaction.Rollback();
                            Console.WriteLine("{0} Transaction Rolled Back", testData.UserName);

                        }
                        catch (Exception ex1)
                        {
                            Console.WriteLine("{0} - Error Info: ", testData.UserName, GetExceptionMessage(ex1));
                        }
                    }
                    finally
                    {
                        testData.countDownEvent.Signal();
                    }
                }
            }
        }

        #endregion

        #region Snapshot

        /// <summary>
        /// Note: you must first run the following SQL on your database for SNAPSHOT ISOLATION to be enabled:
        /// ALTER DATABASE ConcurrencyTestingEFCoreDb SET ALLOW_SNAPSHOT_ISOLATION ON
        /// </summary>
        /// <param name="stateInfo"></param>
        public void Snapshot(Object stateInfo)
        {
            TestInput testData = (TestInput)stateInfo;

            Console.WriteLine("{0} Running Transaction: IsolationLevel.Snapshot", testData.UserName);

            using (var _context = new AppDataContext())
            {
                using (var transaction = _context.Database.BeginTransaction(System.Data.IsolationLevel.Snapshot))
                {
                    try
                    {
                        TestItem item = _context.TestItems.Find(testData.RecordId);
                        int originalValue = item.Value;
                        Console.WriteLine("{0} has read record {1}", testData.UserName, item.Id, item.Value);

                        item.Value += 2;
                        item.ModifiedBy = testData.UserName;
                        item.Modified = DateTime.Now;

                        Console.WriteLine("{0} is updating record {1}, setting Value:{2} + 2", testData.UserName, item.Id, originalValue);

                        // now try to update the read lock to an exclusive lock
                        _context.SaveChanges();

                        Console.WriteLine("{0} about to Commit Transaction", testData.UserName);

                        transaction.Commit();

                        Console.WriteLine("{0} has Committed Transaction", testData.UserName);

                    }
                    catch (System.Data.SqlClient.SqlException ex)
                    {
                        Console.WriteLine("{0} - Error Info: {1}", testData.UserName, GetExceptionMessage(ex));

                        try
                        {
                            transaction.Rollback();

                        }
                        catch (Exception ex1)
                        {
                            Console.WriteLine("{0} - Error trying to roll back transaction: {1}", testData.UserName, GetExceptionMessage(ex1));
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("{0} Transaction failed. Error Info: {1}", testData.UserName, GetExceptionMessage(ex));
                        try
                        {
                            transaction.Rollback();
                            Console.WriteLine("{0} Transaction Rolled Back", testData.UserName);

                        }
                        catch (Exception ex1)
                        {
                            Console.WriteLine("{0} - Error Info: ", testData.UserName, GetExceptionMessage(ex1));
                        }
                    }
                    finally
                    {
                        testData.countDownEvent.Signal();
                    }
                }
            }
        }

        #endregion

        #region Serializable with Retry Logic

        /// <summary>
        /// To Test, first replace optionsBuilder code in AppDataContext.OnConfiguring to use options.EnableRetryOnFailure()
        /// </summary>
        /// <param name="stateInfo"></param>
        public void SerializableWithDeadlockRetryLogic(Object stateInfo)
        {
            TestInput testData = (TestInput)stateInfo;

            Console.WriteLine("{0} Running Transaction: IsolationLevel.Serializable with Retry", testData.UserName);

            using (var _context = new AppDataContext())
            {
                var strategy = _context.Database.CreateExecutionStrategy();

                strategy.Execute(() =>
                {

                    using (var transaction = _context.Database.BeginTransaction(System.Data.IsolationLevel.Serializable))
                    {
                        try
                        {
                            TestItem item = _context.TestItems.Find(testData.RecordId);
                            int originalValue = item.Value;
                            Console.WriteLine("{0} has read record {1}", testData.UserName, item.Id, item.Value);

                            item.Value += 2;
                            item.ModifiedBy = testData.UserName;
                            item.Modified = DateTime.Now;

                            Console.WriteLine("{0} is updating record {1}, setting Value:{2} + 2", testData.UserName, item.Id, originalValue);

                            // now try to update the read lock to an exclusive lock
                            _context.SaveChanges();

                            Console.WriteLine("{0} about to Commit Transaction", testData.UserName);

                            transaction.Commit();

                            Console.WriteLine("{0} has Committed Transaction", testData.UserName);

                        }
                        catch (System.Data.SqlClient.SqlException ex)
                        {
                            Console.WriteLine("{0} Transaction failed. - Error Info: {1}", testData.UserName, GetExceptionMessage(ex));
                            
                            throw (ex);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("{0} Transaction failed. Error Info: {1}", testData.UserName, GetExceptionMessage(ex));

                            throw (ex);
                        }
                    }
                });
            }
            testData.countDownEvent.Signal();
        }

        #endregion

        #region Helper to parse Exception Message for display

        private string GetExceptionMessage(Exception ex)
        {
            if(ex.InnerException != null)
            {
                if(ex.InnerException.InnerException != null)
                {
                    return ex.InnerException.InnerException.Message;
                }
                else
                {
                    return ex.InnerException.Message;
                }
            }
            else
            {
                return ex.Message;
            }
        }
    }

    #endregion

    #region TestInput class to hold variables for test

    public class TestInput
    {
        public int RecordId { get; set; }
        public string UserName { get; set; }
        public CountdownEvent countDownEvent { get; set; }

        public TestInput(int id, string userName, CountdownEvent _countDownEvent)
        {
            this.RecordId = id;
            this.UserName = userName;
            this.countDownEvent = _countDownEvent;
        }
    }

    #endregion

}
