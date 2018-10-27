using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DelegatesAndEvents
{
    public delegate int WorkPerformedHandler(int hours, WorkType workType);

    public class Worker
    {
        public event WorkPerformedHandler WorkPerformed;
        public event EventHandler WorkCompleted;

        public virtual void DoWork(int hours, WorkType workType)
        {
            // Do work here and notify consumer that work has been performed
            //OnWorkPerformed(hours, workType);
            for (int i = 0; i < hours; i++)
            {
                OnWorkPerformed(i + 1, workType);
            }
            OnWorkCompleted();
        }

        protected virtual void OnWorkPerformed(int hours, WorkType workType)
        {
            // method 1
            //if (WorkPerformed != null)
            //{
            //    WorkPerformed(hours, workType)
            //}

            // method 2
            //WorkPerformedHandler del = WorkPerformed as WorkPerformedHandler;
            //if (del != null)   //Listensers are attached
            //{
            //    del(hours, workType);   <----  // Raise Event
            //}

            // method 3
            //if (WorkPerformed is WorkPerformedHandler del)
            //{
            //    del(hours, workType);
            //}

            // method 4 
            (WorkPerformed as WorkPerformedHandler)?.Invoke(hours, workType);

        }

        protected virtual void OnWorkCompleted()
        {
            WorkCompleted?.Invoke(this, EventArgs.Empty);
        }
    }
}
