using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ThreadsAndDelegates
{
    public partial class DirectorySearcherForm : Form
    {
        public DirectorySearcherForm()
        {
            InitializeComponent();
        }

        [STAThread]
        static void Main()
        {
            Application.Run(new DirectorySearcherForm());
        }

        private void directorySearcher_SearchComplete(object sender, System.EventArgs e)
        {
            SearchLabel.Text = string.Empty;
        }

        private void SearchButton_Click_1(object sender, EventArgs e)
        {
            directorySearcher.SearchCriteria = searchText.Text;
            SearchLabel.Text = "Searching...";
            directorySearcher.BeginSearch();
        }
    }
}
