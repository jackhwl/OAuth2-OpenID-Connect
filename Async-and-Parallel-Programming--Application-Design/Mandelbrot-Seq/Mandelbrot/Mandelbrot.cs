/* Mandelbrot.cs */

//
// Mandelbrot generation with managed C#
//

using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;



namespace DotNetMandelbrot
{

	public class Mandelbrot
	{
		private int _startTime = 0;
		private BackgroundWorker _worker = null;

		private double _x;  // parameters of Mandelbrot computation:
		private double _y;
		private double _size;
		private int _pixels;
	    private CancellationTokenSource _cts;

		public Mandelbrot(double x, double y, double size, int pixels)
		{
			_x = x;  // parameters of Mandelbrot computation: 
			_y = y;
			_size = size;
			_pixels = pixels;
		}

		public double TimeTaken()
		{
			int curTime;
			double time;

			curTime = System.Environment.TickCount;
			time = (curTime - _startTime) / 1000.0;

			return time;
		}

		//
		// Returns a color reflecting the value of the Mandelbrot set element at this position.
		//
		private int MandelbrotColor(double yp, double xp, double y, double x, double size, int pixels)
		{
			//
			// compute pixel position:
			//
			double ypos = y + size * (yp - pixels / 2) / ((double)pixels);
			double xpos = x + size * (xp - pixels / 2) / ((double)pixels);

			//
			// now setup for color computation:
			//
			// Reference: http://en.wikipedia.org/wiki/Mandelbrot_set
			//
			y = ypos;
			x = xpos;

			double y2 = y * y;
			double x2 = x * x;

			int color = 1;

			const int MAXCOLOR = 69887; // affects rendering color

			// This magic number happens to produce a colour approximating black with my 
			// colour picker calculation.  It also makes things pretty slow, which is handy.

			//
			// Repeat until we know pixel is not in Mandelbrot set, or until we have reached max # of
			// iterations, in which case pixel is probably in the set.  In the latter, color will be
			// black.
			//
			while ((y2 + x2) <= 4 && color < MAXCOLOR)
			{
				y = 2 * x * y + ypos;
				x = x2 - y2 + xpos;

				y2 = y * y;
				x2 = x * x;

				color++;
			}

			return color;
		}

		//
		// Designed to be called asynchronously as part of BackgroundWorker object.  
		// Args for Mandelbrot computation are passed via constructor, and use worker's
		// ReportProgress event to update UI as it computes each row of the Mandelbrot
		// set.
		//
		public void Calculate(Object sender, DoWorkEventArgs e)
		{
			_worker = (BackgroundWorker)sender;

			_startTime = System.Environment.TickCount;

			//
			// now start computing Mandelbrot set, row by row:
			//
			//for (int r = 0; r < _pixels; r++)
            _cts = new CancellationTokenSource();
            var options = new ParallelOptions();
		    options.CancellationToken = _cts.Token;
		    try
		    {
		        Parallel.For(0, _pixels, options, (r) =>
		        {
		            // Did the user cancel?  If so, stop loop:
		            if (_worker.CancellationPending)
		                //break;
		                return;

		            //
		            // Since we need to pass the new pixel values to the UI thread for display,
		            // we allocate a new array so we can keep running in parallel (versus having
		            // to wait for the array to become available for the next set of values).
		            //
		            int[] values = new int[_pixels]; // one row:

		            //for (int c = 0; c < _pixels; ++c)
		            Parallel.For(0, _pixels, options, (c) =>
		                values[c] = MandelbrotColor(r, c, _y, _x, _size, _pixels)
		            );

		            //
		            // Set value in last 5 pixels of each row to a thread id so we can "see" who
		            // computed this row:
		            //
		            int threadID = System.Threading.Thread.CurrentThread.ManagedThreadId; // .NET thread id:

		            for (int c = _pixels - 5; c < _pixels; c++)
		                values[c] = -threadID;

		            //
		            // we've generated a row, report this as progress for display:
		            //
		            _worker.ReportProgress(r, new object[] {r, values});
		        });
		    }
		    catch (OperationCanceledException oce)
		    {
                /*ignore*/
		    }

		    // did user cancel?  If so, set background worker's flag:
			if (_worker.CancellationPending)
				e.Cancel = true;
		}

	    public void CancelCalulation()
	    {
            _cts.Cancel();
	    }

	}//class
}//namespace

