using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DelegatesAndEvents
{

    class Program
    {
        static void Main(string[] args)
        {
            //WorkPerformedHandler del1 = new WorkPerformedHandler(WorkPerformed1);
            //WorkPerformedHandler del2 = new WorkPerformedHandler(WorkPerformed2);
            //WorkPerformedHandler del3 = new WorkPerformedHandler(WorkPerformed3);

            //del1(5, WorkType.Golf);
            //del2(10, WorkType.GenerateReports);
            var worker = new Worker();
            //worker.WorkPerformed += new EventHandler<WorkPerformedEventArgs>(Work_WorkPerformed);
            //worker.WorkCompleted += new EventHandler(Work_WorkCompleted);
            // Delegate Inference
            worker.WorkPerformed += Work_WorkPerformed; 
            worker.WorkCompleted += Work_WorkCompleted;

            worker.DoWork(10, WorkType.GenerateReports);
            worker.WorkPerformed -= Work_WorkPerformed; 
            worker.WorkCompleted -= Work_WorkCompleted;

            //del1 += del2;
            //del1 += del3;

            //del1 += del2 + del3;

            //int finalHours = del1(10, WorkType.GoToMeetings);
            //Console.WriteLine(finalHours);


            Console.ReadLine();
        }

        static void Work_WorkPerformed(object sender, WorkPerformedEventArgs e)
        {
            Console.WriteLine(e.Hours + " " + e.WorkType);
        }

        static void Work_WorkCompleted(object sender, EventArgs e)
        {
            Console.WriteLine("Done! ");
        }

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
