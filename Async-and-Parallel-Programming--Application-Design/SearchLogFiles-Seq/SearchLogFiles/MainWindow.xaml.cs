/* MainWindow.xaml.cs */

//
// Sequential Search Version
//
//   Given a set of log files and a regular expression, searches the files and outputs the
// total number of search hits.  Tasks are used in an async way to keep the UI responsive 
// while the search is carried out; the search itself, however, is done sequentially in
// this version.
//
//   By default, the demo is setup to search for IP addresses of the form 202.187.*.*.  The
// idea is to search the log files for IP ranges of known hacker machines.
//
// NOTE: the log files are assumed to be just text files, no particular format is assumed
// during the search.  To generate log files for performance testing, see the accompanying
// program "datafiles\GenRandomLogFiles".
//
// NOTE: this version will fail on really large files (e.g. 1GB).  This is a known problem,
// a trade-off to keep the code simpler in SearchFiles().  A more robust version of this
// app is available in the exercise-files.
//

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Win32;


namespace SearchLogFiles
{

	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
		}


		/// <summary>
		/// Triggered when form is first loaded into memory; init here.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			this.txtPattern.Focus();
			this.txtPattern.SelectAll();

			//
			// add default logfiles in the listbox for demo purposes:
			//
			string initialdir = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "logfiles");

			if (Directory.Exists(initialdir))
			{
				string[] FNs = System.IO.Directory.GetFiles(initialdir);

				foreach (string fullFN in FNs)
					this.lstFiles.Items.Add(new DisplayFileName(System.IO.Path.GetFileName(fullFN), fullFN));
			}
		}


		/// <summary>
		/// User is selecting files to search...
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void SelectLogFiles_Click(object sender, RoutedEventArgs e)
		{
			// 
			// Prep open file dialog to get filename(s) from user:
			//
			var dialog = new OpenFileDialog();
			dialog.CheckFileExists = true;
			dialog.CheckPathExists = true;
			dialog.Filter = "*.*|*.*";
			dialog.Multiselect = true;

			string initialdir = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "logfiles");
			if (Directory.Exists(initialdir))
				dialog.InitialDirectory = initialdir;

			// 
			// show --- did user select files and click OK?
			//
			bool? ok = dialog.ShowDialog();

			if (ok.HasValue && ok.Value == true)  // user selected one or more log files:
			{
				// so fill list box with filenames:
				this.lstFiles.Items.Clear();

				foreach (string fullFN in dialog.FileNames)
					this.lstFiles.Items.Add(new DisplayFileName(System.IO.Path.GetFileName(fullFN), fullFN));
			}
		}


		/// <summary>
		/// User has clicked button to search the log files based on the given pattern, so 
		/// let's up an set of tasks to run this in the background so we don't lock the UI.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void cmdSearch_Click(object sender, RoutedEventArgs e)
		{
			this.txtblkResults.Text = "";      // clear previous results...

			this.cmdSearch.IsEnabled = false;  // disable button until we are done:
			this.spinnerWait.Visibility = System.Windows.Visibility.Visible;
			this.spinnerWait.Spin = true;

			var sw = new System.Diagnostics.Stopwatch();  // start clock:
			sw.Restart();

			//
			// search all the files listed in the listbox:
			//
			List<string> filenames = new List<string>();

			foreach (DisplayFileName dfn in this.lstFiles.Items)
				filenames.Add(dfn.FullFileName);

			string pattern = this.txtPattern.Text;

			// create a task to make the call so we don't lock up the UI:
			Task<int> search = Task.Factory.StartNew<int>(() =>
				{
					return SearchFiles(filenames, pattern);
				}
			);

			//
			// When the search task finishes, update the UI.  We another task to wait for the result 
			// so that we don't lock up the UI waiting --- but this has to be a separate task since
			// any UI work must be done by the thread that owns the UI (the main thread):
			//
			Task UpdateUI = search.ContinueWith((antecedent) =>
				{
					try
					{
						int hits = antecedent.Result;

						var timeinMS = sw.ElapsedMilliseconds;  // stop clock:
						double time = timeinMS / 1000.0;  // convert to secs:

						string results = string.Format("Hits:\t{0:#,##0}\nFiles:\t{1:#,##0}\nTime:\t{2:#,##0.00} secs\n",
							hits,
							lstFiles.Items.Count,
							time);

						this.txtblkResults.Text = results;
					}
					catch (AggregateException ae)
					{
						this.txtblkResults.Text = "";

						ae = ae.Flatten();
						foreach (Exception ex in ae.InnerExceptions)
							this.txtblkResults.Text += string.Format("**Error: '{0}'\n", ex.Message);
					}
					catch (Exception ex)
					{
						this.txtblkResults.Text = string.Format("**Error: '{0}'\n", ex.Message);
					}

					// reset UI:
					this.spinnerWait.Spin = false;
					this.spinnerWait.Visibility = System.Windows.Visibility.Collapsed;

					this.cmdSearch.IsEnabled = true;  // re-enable button:
				},

				TaskScheduler.FromCurrentSynchronizationContext()  // must run on current (UI) thread:
			);
		}


		/// <summary>
		/// Does the actual work of searching the list of log files, returning the total number 
		/// of hits.
		/// </summary>
		/// <param name="filenames"></param>
		/// <param name="pattern"></param>
		/// <returns># of search hits</returns>
		private int SearchFiles(List<string> filenames, string pattern)
		{
			int hits = 0;

			// we apply same reg expr to each file, so create 1 compiled RE and reuse:
			Regex re = new Regex(pattern, RegexOptions.Compiled | RegexOptions.Multiline);

			//
			// For each file f, search it:
			//
            List<Task> tasks = new List<Task>();
			foreach (string f in filenames)
			{
			    Task t = Task.Factory.StartNew(() =>
			    {
			        //
			        // read the file:
			        //
			        byte[] bytes = File.ReadAllBytes(f);

			        // 
			        // convert to string for RE processing:
			        //
			        string contents = System.Text.Encoding.UTF8.GetString(bytes);

			        //
			        // apply pattern repeatedly as a regular expression:
			        //
			        Match m = re.Match(contents);

			        while (m.Success) // repeat for each successive match:
			        {
			            hits++;
			            m = m.NextMatch();
			        }
			    });
                tasks.Add(t);
			}

		    Task.WaitAll(tasks.ToArray());
			//
			// done, return total # of search hits:
			//
			return hits;
		}

	}//class
}//namespace