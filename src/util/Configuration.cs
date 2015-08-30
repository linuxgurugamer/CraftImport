using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace CraftImport
{
	public class Configuration
	{
		private static readonly Configuration instance = new Configuration ();

		private static readonly String FILE_NAME = "CraftImport.dat";

		[Persistent] public Log.LEVEL logLevel { get; set; }

//		public bool screenshotAtIntervals { get; set; }
		public bool useBlizzyToolbar { get; set; }
		public string lastImportDir { get; set; }
		public bool showDrives { get; set; }

		internal Boolean BlizzyToolbarIsAvailable = false;


		public Configuration ()
		{
#if (DEBUG)
			logLevel = Log.LEVEL.INFO;
#else
			logLevel = Log.LEVEL.WARNING;
#endif
			useBlizzyToolbar = false;
			lastImportDir = "";
			showDrives = true;
		}

		public static Configuration Instance {
			get {
				return instance; 
			}
		}
			
		public void Save ()
		{
			FileOperations.SaveConfiguration (this, FILE_NAME);
			CI.changeCallbacks = true;
		}

		public void Load ()
		{
			FileOperations.LoadConfiguration (this, FILE_NAME);
		}

	}
}
