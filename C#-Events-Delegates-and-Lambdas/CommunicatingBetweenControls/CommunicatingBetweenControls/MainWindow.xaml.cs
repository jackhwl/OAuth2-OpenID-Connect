using CommunicatingBetweenControls.UserControls;
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

namespace CommunicatingBetweenControls
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            LoadControls();
        }

        private void LoadControls()
        {
            var jobs = new Jobs();
            jobs.SetValue(Grid.RowProperty, 1);
            LayoutRoot.Children.Add(jobs);

            var empsOnJob = new EmployeesOnJob();
            var jobDetails = new JobDetails();
            jobDetails.Margin = new Thickness(25, 0, 0, 0);
            jobDetails.SetValue(Grid.ColumnProperty, 1);

            ContainerGrid.Children.Add(empsOnJob);
            ContainerGrid.Children.Add(jobDetails);
        }
    }
}
