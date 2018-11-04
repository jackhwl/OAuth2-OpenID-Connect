/* Form1.cs */

//
// Mandelbrot generation with managed C#
//

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;


namespace DotNetMandelbrot
{
	public class Form1 : System.Windows.Forms.Form
	{
		#region Windows Forms boilerplate

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public Form1()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			_imagePanel.Paint += new System.Windows.Forms.PaintEventHandler(this.imagePanel_Paint);

			_colorTable = new Color[8];  // for seeing which threads did which part of image:
			_colorTable[0] = Color.Red;
			_colorTable[1] = Color.Blue;
			_colorTable[2] = Color.Yellow;
			_colorTable[3] = Color.Green;
			_colorTable[4] = Color.LightBlue;
			_colorTable[5] = Color.Fuchsia;
			_colorTable[6] = Color.Sienna;
			_colorTable[7] = Color.SpringGreen;
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#endregion


		#region Windows Form Designer generated code
		private Button _goButton;
		private Panel _imagePanel;
		private Label _versionLabel;
		private Label _timeLabel;
		private Label label2;
		private Label label1;

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
			this._goButton = new System.Windows.Forms.Button();
			this._imagePanel = new System.Windows.Forms.Panel();
			this._versionLabel = new System.Windows.Forms.Label();
			this._timeLabel = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// _goButton
			// 
			this._goButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._goButton.Location = new System.Drawing.Point(510, 13);
			this._goButton.Name = "_goButton";
			this._goButton.Size = new System.Drawing.Size(104, 49);
			this._goButton.TabIndex = 11;
			this._goButton.Text = "Go";
			this._goButton.UseVisualStyleBackColor = true;
			this._goButton.Click += new System.EventHandler(this._goButton_Click);
			// 
			// _imagePanel
			// 
			this._imagePanel.BackColor = System.Drawing.Color.White;
			this._imagePanel.Location = new System.Drawing.Point(14, 78);
			this._imagePanel.Name = "_imagePanel";
			this._imagePanel.Size = new System.Drawing.Size(600, 600);
			this._imagePanel.TabIndex = 12;
			// 
			// _versionLabel
			// 
			this._versionLabel.AutoSize = true;
			this._versionLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._versionLabel.Location = new System.Drawing.Point(105, 13);
			this._versionLabel.Name = "_versionLabel";
			this._versionLabel.Size = new System.Drawing.Size(18, 20);
			this._versionLabel.TabIndex = 16;
			this._versionLabel.Text = "?";
			// 
			// _timeLabel
			// 
			this._timeLabel.AutoSize = true;
			this._timeLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._timeLabel.Location = new System.Drawing.Point(105, 42);
			this._timeLabel.Name = "_timeLabel";
			this._timeLabel.Size = new System.Drawing.Size(18, 20);
			this._timeLabel.TabIndex = 15;
			this._timeLabel.Text = "?";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label2.Location = new System.Drawing.Point(22, 42);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(52, 20);
			this.label2.TabIndex = 14;
			this.label2.Text = "Time:";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label1.Location = new System.Drawing.Point(22, 13);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(75, 20);
			this.label1.TabIndex = 13;
			this.label1.Text = "Version:";
			// 
			// Form1
			// 
			this.AcceptButton = this._goButton;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.BackColor = System.Drawing.Color.PaleGreen;
			this.ClientSize = new System.Drawing.Size(628, 690);
			this.Controls.Add(this._versionLabel);
			this.Controls.Add(this._timeLabel);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this._imagePanel);
			this.Controls.Add(this._goButton);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
			//this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "Form1";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Sequential.NET Mandelbrot";
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
			this.Load += new System.EventHandler(this.Form1_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion

		//
		// Fields:
		//
		private Mandelbrot _mandelbrot = null;
		private BackgroundWorker _worker = null;
		private Bitmap _image = null;
		private bool _running = false;
		private Color[] _colorTable;

		//
		// Called when form is first loaded for display:
		//
		private void Form1_Load(object sender, EventArgs e)
		{
			String version, platform;

#if DEBUG
			version = "debug";
#else
			version = "release";
#endif

#if _WIN64
	platform = "64-bit";
#elif _WIN32
	platform = "32-bit";
#else
			platform = "any-cpu";
#endif

			//
			// Initialize form:
			//
			_versionLabel.Text = String.Format("Sequential.NET, {0} {1}", platform, version);
			Top = 20;
		}


		//
		// User clicks the "Go" button to start the mandelbrot calculation, or "Cancel"
		// to stop:
		//
		private void _goButton_Click(object sender, EventArgs e)
		{
			//
			// button operates in two states, "Go" or "Cancel"...
			//
			try
			{
				if (_running == false)  // Go:
				{
					//
					// parameters for a standard Mandelbrot computation and graphic display:
					//
					double x = -0.70;
					double y = 0;
					double size = 2.5;
					int pixels = 600;

					_mandelbrot = new Mandelbrot(x, y, size, pixels);

					//
					// Reset bitmap and display:
					//
					if (_image != null)
						_image.Dispose();

					// Size panel:
					_imagePanel.Width = pixels;
					_imagePanel.Height = pixels;

					_image = new Bitmap(pixels, pixels);
					_imagePanel.Invalidate();

					_goButton.Text = "Cancel";

					_running = true;

					// 
					// Okay, here we go, kick off the Mandelbrot computation on a worker (background) thread.
					// We use a separate thread for computation so that the main thread can monitor the UI and
					// be responsive to the user (e.g. cancel) and show progres...
					//
					_worker = new BackgroundWorker();

					_worker.WorkerReportsProgress = true;
					_worker.WorkerSupportsCancellation = true;

					// setup event handler for the actual Mandelbrot computation:
					_worker.DoWork += new DoWorkEventHandler(_mandelbrot.Calculate);
					// setup event handler for progress reports (each row of Mandelbrot set):
					_worker.ProgressChanged += new ProgressChangedEventHandler(this.OnProgress);
					// setup event handler for completion notification:
					_worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(this.OnComplete);

					//
					// Ok, start the worker running!
					//
					_worker.RunWorkerAsync(null);  // start worker thread running!
				}
				else  // Cancel:
				{
					//
					// Button was in 'Cancel' mode...  Tell worker to stop, and wait for completion callback.
					// For now, just gray out the button and change the mouse pointer...
					//
					_goButton.Enabled = false;
					this.Cursor = Cursors.WaitCursor;

					_worker.CancelAsync();
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("Error in Form1.goButton_Click.Cancel: " + ex.Message);
				System.Environment.Exit(1);
			}
		}


		//
		// Computation has completed, we get called back by the worker thread 
		// when this happens.
		//
		void OnComplete(Object sender, RunWorkerCompletedEventArgs e)
		{
			try
			{
				if (e.Error != null)  // worker failed:
				{
					_timeLabel.Text += "  (ERROR)";

					MessageBox.Show("Error: " + e.Error.ToString());
				}
				else if (e.Cancelled)  // worker was told to cancel:
				{
					_timeLabel.Text += "  (CANCELED)";
				}
				else  // worker successfully ran to completion:
				{
					_timeLabel.Text += "  (DONE)";
				}

				//
				// No matter what, reset the UI:
				//
				_goButton.Enabled = true;
				_goButton.Text = "Go";
				this.Cursor = Cursors.Default;

				_running = false;
			}
			catch (Exception ex)
			{
				MessageBox.Show("Error in Form1.OnComplete: " + ex.Message);
				System.Environment.Exit(1);
			}
		}


		//
		// Called each time we have new data to display:  this is called each time
		// a new row of the Mandelbrot set has been computed.
		//
		void OnProgress(Object sender, ProgressChangedEventArgs e)
		{
			try
			{
				// e.ProgressPercentage -- ignored

				object[] args = (object[]) e.UserState;

				int row      = (int)args[0];
				int[] values = (int[])args[1];

				for (int x = 0; x < values.Length; x++)
				{
					int v = values[x];

					Color c;

					if (v >= 0)  // standard Mandelbrot pixel value:
						c = Color.FromArgb(255 - (v * 3 % 256), 255 - (v * 7 % 256), 255 - (v * 13 % 256));
					else  // -v is the thread id, use as index into color table:
					{
						v = -v;  // first, turn into a positive, zero-based index:
						if (v > 0)
							v--;

						v = v % 8;  // make sure we don't fall outside the table:

						c = _colorTable[v];
					}

					_image.SetPixel(x, row, c);
				}

				_imagePanel.Invalidate(new Rectangle(0, row, _image.Width, 1));
				_timeLabel.Text = _mandelbrot.TimeTaken().ToString("F4") + "s";
			}
			catch (Exception ex)
			{
				MessageBox.Show("Error in Form1.OnProgress: " + ex.Message);
				System.Environment.Exit(1);
			}
		}


		//
		// The PictureBox control isn't designed to deal with bitmaps that change, 
		// so it is inefficient keeping its display up to date as the drawing is 
		// created. So we just draw the bitmap manually by handling a Panel control's 
		// Paint event.
		//
		private void imagePanel_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
		{
			if (_image != null)
				e.Graphics.DrawImage(_image, 0, 0);
		}


		//
		// User is closing the form, stop any work if we have some running:
		//
		private void Form1_FormClosed(object sender, FormClosedEventArgs e)
		{
			if (_running)
			{
				_worker.CancelAsync();
				System.Threading.Thread.Sleep(1000);  // give worker time to process:
			}
		}

	}//class
}//namespace
