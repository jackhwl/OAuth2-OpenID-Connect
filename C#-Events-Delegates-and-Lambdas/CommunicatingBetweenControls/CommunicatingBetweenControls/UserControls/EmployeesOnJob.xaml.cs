using CommunicatingBetweenControls.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CommunicatingBetweenControls.UserControls
{
    /// <summary>
    /// Interaction logic for EmployeesOnJob.xaml
    /// </summary>
    public partial class EmployeesOnJob : UserControl
    {
        List<Employee> _Employees = new List<Employee> {
            new Employee { ID = 1, Name = "John Doe", Jobs = new List<Job>
                            {
                                new Job { ID = 1, Title = "Area 1 Maintenance" },
                                new Job { ID = 2, Title = "Edge Park" },
                                new Job { ID = 3, Title = "Paint Benches" },
                                new Job { ID = 4, Title = "Build New Wall" }
                            }
            },
            new Employee { ID = 2, Name = "Jane Doe", Jobs = new List<Job>
                            {
                                new Job { ID = 3, Title = "Paint Benches" },
                                new Job { ID = 4, Title = "Build New Wall" }
                            }
            },
            new Employee { ID = 3, Name = "Michelle Davis", Jobs = new List<Job>
                            {
                                new Job { ID = 1, Title = "Area 1 Maintenance" },
                                new Job { ID = 3, Title = "Paint Benches" }
                            }
            },
        };

        public EmployeesOnJob()
        {
            InitializeComponent();
            Mediator.GetInstance().JobChanged += (s, e) => { BindData(e.Job); };
        }

        private void BindData(Job job)
        {
            this.DataContext = job;
            var emps = _Employees.Where(e => e.Jobs.Any(j => j.ID == job.ID));
            EmployeesListView.ItemsSource = emps;
        }
    }
}
