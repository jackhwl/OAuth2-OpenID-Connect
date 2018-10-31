/* Program.cs */

//
// Generates N random web server log files, in W3C common log format.  Note that only some of
// the values are random: host ip address, date, and the bytes.  
//
// Common Log Format
//
// host ident authuser date request status bytes
//
// 127.0.0.1 - frank [10/Oct/2000:13:55:36 -0700] "GET /apache_pb.gif HTTP/1.0" 200 2326
//

using System;
using System.IO;


namespace GenRandomLogFiles
{
	class Program
	{

		static void Main(string[] args)
		{
			var filenameFormatString = "logfile-{0}.txt";
			var numLogEntries = 250000;  // 1 entry == 100 bytes; e.g. 10,000 entries => 1MB
			int numLogFiles   = 25;

			if (args.Length > 0)  // did user specify how many files to generate?
			{
				bool success = int.TryParse(args[0], out numLogFiles);
				if (!success)
				{
					Console.WriteLine("\n**Error: specify argument should be an integer (number of log files to generate)\n");
					Environment.Exit(-1);
				}
			}

			Console.WriteLine();
			Console.WriteLine("** Generating Random Web Log Files **");
			Console.WriteLine("   files:      {0}", numLogFiles);

			if (numLogFiles == 1)
				Console.WriteLine("   filename:  '{0}'", string.Format(filenameFormatString, 1));
			else
				Console.WriteLine("   filenames: '{0}', '{1}', ...",
					string.Format(filenameFormatString, 1),
					string.Format(filenameFormatString, 2));

			Console.WriteLine("   entries:    {0:#,##0}", numLogEntries);
			Console.WriteLine();

			//
			// Create logfile with given number of entries, overwrite if already exists:
			//
			var today = DateTime.Now;
			var rand = new Random();

			for (int i = 1; i <= numLogFiles; i++)
			{
				string fn = string.Format(filenameFormatString, i);

				using (StreamWriter writer = new StreamWriter(fn, false /*overwrite*/))
				{
					for (long l = 0; l < numLogEntries; l++)
					{
						// host ident authuser date request status bytes:
						var rdate = today.AddDays(-rand.Next(0, 1000));  // a random date from today backwards:

						writer.WriteLine("{0} {1} {2} [{3}] \"{4}\" {5} {6}",
							GetRandomIPAddress(rand),
							"-",
							"someuser",
							rdate.ToString("d/MMM/yyyy:H:m:s zzz"),
							"GET /somepage.html HTTP/1.0",
							200,
							rand.Next(100, 100000)  // random bytes 100 <= 100000:
						);
					}
				}//using
			}

			//
			// done -- how big is resulting file?
			//
			string filename = string.Format(filenameFormatString, 1);

			FileInfo fi = new FileInfo(filename);
			double sizeinMB = fi.Length / 1048576.0;

			Console.WriteLine("   filesize:   {0:#,##0.00 MB}", sizeinMB);
			Console.WriteLine("** Done! **");
			Console.WriteLine();
			Console.WriteLine();
			Console.WriteLine("Press a key to exit...");
			Console.ReadKey();
		}


		static string GetRandomIPAddress(Random rand)
		{
			int a, b, c, d;

			a = rand.Next(100, 255);  // 100 <= i < 255:
			b = rand.Next(0, 255);    // 0 <= i < 255:
			c = rand.Next(0, 255);    // 0 <= i < 255:
			d = rand.Next(0, 255);    // 0 <= i < 255:

			return string.Format("{0}.{1}.{2}.{3}", a, b, c, d);
		}

	}//class
}//namespace
