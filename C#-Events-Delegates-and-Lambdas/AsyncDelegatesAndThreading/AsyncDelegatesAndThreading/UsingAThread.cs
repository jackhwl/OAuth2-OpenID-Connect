using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ThreadsAndDelegates
{
    public partial class UsingAThread : Form
    {
        int _Max;

        public UsingAThread()
        {
            InitializeComponent();
        }

        [STAThread]
        static void Main()
        {
            Application.Run(new UsingAThread());
        }

        private void StartButton_Click(object sender, System.EventArgs e)
        {
            _Max = 100;
            //Start thread

            MessageBox.Show("Done with operation!!");
        }

        private void StartProcess()
        {

                this.Refresh();
                this.pbStatus.Maximum = _Max;
                for (int i = 0; i <= _Max; i++)
                {
                    Thread.Sleep(10);
                    this.lblOutput.Text = i.ToString();
                    this.pbStatus.Value = i;
                }

        }
    }
}
