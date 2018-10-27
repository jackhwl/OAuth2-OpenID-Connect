using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace DelegatesAndEvents
{
    public delegate int BizRulesDelegate(int x, int y);
    class Program
    {
        static void Main(string[] args)
        {
            //WorkPerformedHandler del1 = new WorkPerformedHandler(WorkPerformed1);
            //WorkPerformedHandler del2 = new WorkPerformedHandler(WorkPerformed2);
            //WorkPerformedHandler del3 = new WorkPerformedHandler(WorkPerformed3);

            //del1(5, WorkType.Golf);
            //del2(10, WorkType.GenerateReports);
            BizRulesDelegate addDel = (x, y) => x + y;
            BizRulesDelegate multiplyDel = (x, y) => x * y;

            Func<int, int, int> funcAddDel = (x, y) => x + y;
            Func<int, int, int> funcMultiplyDel = (x, y) => x * y;

            Action<int, int> myAction = (x, y) => Console.WriteLine(x + y * 2);
            Action<int, int> mymAction = (x, y) => Console.WriteLine(x * y * 2);
            var data = new ProcessData();
            data.Process(2,3, addDel);
            data.Process(2,3, multiplyDel);
            data.ProcessAction(2,3,myAction);
            data.ProcessAction(2,3,mymAction);

            data.ProcessFunc(2,3,funcAddDel);
            data.ProcessFunc(2,3,funcMultiplyDel);

            var custs = new List<Customer>
            {
                new Customer {City = "Phoenix", FirstName = "John", LastName = "Doe", ID = 1},
                new Customer {City = "Phoenix", FirstName = "Jane", LastName = "Doe", ID = 500},
                new Customer {City = "Seattle", FirstName = "Suki", LastName = "Pizzoro", ID = 3},
                new Customer {City = "New Your City", FirstName = "Michelle", LastName = "Smith", ID = 4}
            };

            var phxCusts = custs
                .Where(c => c.City == "Phoenix")
                .OrderBy(c=>c.FirstName);

            foreach (var cust in phxCusts)
            {
                Console.WriteLine(cust.FirstName);
            }

            var worker = new Worker();
            //worker.WorkPerformed += new EventHandler<WorkPerformedEventArgs>(Work_WorkPerformed);
            //worker.WorkCompleted += new EventHandler(Work_WorkCompleted);
            // Delegate Inference
            worker.WorkPerformed +=  (s,e) => Console.WriteLine(e.Hours + " " + e.WorkType);
            worker.WorkCompleted +=  (s,e) => Console.WriteLine("Done! ");

            worker.DoWork(4, WorkType.GenerateReports);
            //worker.WorkPerformed -= Work_WorkPerformed; 
            //worker.WorkCompleted -= Work_WorkCompleted;

            //del1 += del2;
            //del1 += del3;

            //del1 += del2 + del3;

            //int finalHours = del1(10, WorkType.GoToMeetings);
            //Console.WriteLine(finalHours);


            Console.ReadLine();
        }

        //static void Work_WorkPerformed(object sender, WorkPerformedEventArgs e)
        //{
        //    Console.WriteLine(e.Hours + " " + e.WorkType);
        //}

        //static void Work_WorkCompleted(object sender, EventArgs e)
        //{
        //    Console.WriteLine("Done! ");
        //}

        //static void DoWork(WorkPerformedHandler del)
        //{
        //    del(5, WorkType.GenerateReports);
        //}

        //static int WorkPerformed1(int hours, WorkType workType)
        //{
        //    Console.WriteLine("WorkPerformed1 called " + hours.ToString());
        //    return hours + 1;
        //}

        //static int WorkPerformed2(int hours, WorkType workType)
        //{
        //    Console.WriteLine("WorkPerformed2 called " + hours.ToString());
        //    return hours + 2;
        //}

        //static int WorkPerformed3(int hours, WorkType workType)
        //{
        //    Console.WriteLine("WorkPerformed3 called " + hours.ToString());
        //    return hours + 3;
        //}
    }

    public enum WorkType
    {
        GoToMeetings,
        Golf,
        GenerateReports
    }
}
