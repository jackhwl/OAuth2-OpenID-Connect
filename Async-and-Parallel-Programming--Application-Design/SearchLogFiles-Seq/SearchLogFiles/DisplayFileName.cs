/* DisplayFileName.cs */

//
// Filename + Full pathname pair so we can store both together in a listbox.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace SearchLogFiles
{
	/// <summary>
	/// A simple class so we can display just the filename (aka "safe" filename) in a listbox, but hold onto 
	/// the full path in the object.
	/// </summary>
	class DisplayFileName
	{
		public string SafeFileName;
		public string FullFileName;

		public DisplayFileName(string sfn, string ffn)
		{
			SafeFileName = sfn;
			FullFileName = ffn;
		}

		public override string ToString()
		{
			return SafeFileName;
		}
	}//class

}//namespace
