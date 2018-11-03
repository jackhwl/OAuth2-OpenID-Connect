using Microsoft.EntityFrameworkCore;
using System;
using ChangeManagementSystem.Models;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Threading;

namespace ChangeManagementSystem.ConsoleTests
{
    class Program
    {
        static int numberOfThreads = 2;
        static Barrier _barrier = new Barrier(numberOfThreads);

        static void Main(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDataContext>();
            optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=ChangeManagementDB;Trusted_Connection=True;MultipleActiveResultSets=true");
            AppDataContext context = new AppDataContext(optionsBuilder.Options);

            ChangeRepository _changeRepository = new ChangeRepository(context);
            //ChangeRequestTask task = new ChangeRequestTask();
            //task.Name = "Task1";
            //task.Summary = "Test Task for Pessimistic Concurrency Check";
            //task.ChangeRequestID = 5016;
            //int taskid = _changeRepository.CreateChangeRequestTask(task, "user0");

            //// place pessimistic lock on record and lock for user0
            //_changeRepository.GetChangeRequestTaskbyIdForEdit(taskid, "user0");



            // sleep so the lock can expire (3 seconds)
            //Thread.Sleep(4000);
            int taskid = 13;

            for (int i = 0; i < numberOfThreads; i++)
            {
                ThreadPool.QueueUserWorkItem(ThreadProc, new ConcurrencyTestOptions(taskid, "user" + i));
            }

            Console.ReadKey();
        }


        static void ThreadProc(Object stateInfo)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDataContext>();
            optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=ChangeManagementDB;Trusted_Connection=True;MultipleActiveResultSets=true");

            ConcurrencyTestOptions testOptions = (ConcurrencyTestOptions)stateInfo;

            try
            {
                using (var context = new AppDataContext(optionsBuilder.Options))
                {
                    ChangeRepository _changeRepository = new ChangeRepository(context);
                    //Console.WriteLine("Before Barrier, user={0}", testOptions.UserName);
                    Console.WriteLine("Before get for edit: {0}", testOptions.UserName);

                    _changeRepository.GetChangeRequestTaskbyIdForEdit(testOptions.RecordId, testOptions.UserName);

                    //Console.WriteLine("After Barrier, user={0}", testOptions.UserName);
                    Console.WriteLine("After get for edit: {0}", testOptions.UserName);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }

    public class ConcurrencyTestOptions
    {
        public int RecordId { get; set; }
        public string UserName { get; set; }

        public ConcurrencyTestOptions(int id, string userName)
        {
            this.RecordId = id;
            this.UserName = userName;
        }
    }
}
