using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Annotations;
using CommunicatingBetweenControls.Model;

namespace CommunicatingBetweenControls
{
    public sealed class Mediator
    {
        // static members handle Singleton functionality
        private  static readonly Mediator _Instance = new Mediator();
        // hide constractor, so cannot use new, have to use the method we provided
        private Mediator() {}

        public static Mediator GetInstance()
        {
            return _Instance;
        }

        // Instance functionality
        public event EventHandler<JobChangedEventArgs> JobChanged;

        public void OnJobChanged(object sender, Job job)
        {
            //var jobChangeDelegate = JobChanged as EventHandler<JobChangedEventArgs>;
            //if (jobChangeDelegate != null)
            //{
            //    jobChangeDelegate(sender, new JobChangedEventArgs{Job = job});
            //}
            var jobChangeDelegate = JobChanged;
            jobChangeDelegate?.Invoke(sender, new JobChangedEventArgs{Job = job});
        }

    }
}
