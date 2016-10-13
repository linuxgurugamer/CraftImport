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
		//public string ckanExecPath { get; set; }
		public string uid {get; set;}
		public string pswd { get; set;}
		public bool showWarning { get; set; }

		public int vabResolution, sphResolution;
		public float vabElevation, sphElevation;
		public float vabAzimuth, sphAzimuth;
		public float vabPitch, sphPitch;
		public float vabHeading, sphHeading;
		public float vabFov, sphFov;

		internal Boolean BlizzyToolbarIsAvailable = false;

		public Color backgroundcolor; 


		public void setDefaultVABResolution()
		{
			vabResolution = 1024;
			vabElevation = 35;
			vabAzimuth = 135;
			vabPitch = 35;
			vabHeading = 135;
			vabFov = 0.9F;

		}
		public void setDefaultSPHResolution()
		{

			sphResolution = 1024;
			sphElevation = 45;
			sphAzimuth = 45;
			sphPitch = 45;
			sphHeading = 45;
			sphFov = 0.9F;

		}
		public Configuration ()
		{
#if (DEBUG)
			logLevel = Log.LEVEL.INFO;
#else
			logLevel = Log.LEVEL.WARNING;
#endif
			useBlizzyToolbar = false;
			lastImportDir = "";
			//ckanExecPath = "";
			showWarning = true;
			showDrives = true;
			pswd = "";
			uid = "";
			setDefaultSPHResolution ();
			setDefaultVABResolution ();
			backgroundcolor = Color.gray;
		}

		public static Configuration Instance {
			get {
				return instance; 
			}
		}
			
		public void Save ()
		{
			FileOperations.SaveConfiguration (this, FILE_NAME);
			//CI.changeCallbacks = true;
		}

		public void Load ()
		{
			FileOperations.LoadConfiguration (this, FILE_NAME);
		}

	}
}
