
// just uncomment this line to restrict file access to KSP installation folder
#define _UNLIMITED_FILE_ACCESS

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;



namespace CraftImport
{
	public  class FileOperations
	{
		public static readonly String ROOT_PATH = KSPUtil.ApplicationRootPath.Substring (0, KSPUtil.ApplicationRootPath.Length - 16);
		//public static readonly String ROOT_PATH = KSPUtil.ApplicationRootPath;
		private static readonly String CONFIG_BASE_FOLDER = ROOT_PATH + "GameData/";
		private static String CI_BASE_FOLDER = CONFIG_BASE_FOLDER + "CraftImport/";
		private const String CI_NODENAME = "CraftImport";
		private static readonly String CI_CFG_FILE = CI_BASE_FOLDER + "CI_Settings.cfg";

		private static ConfigNode configFile = null;
		private static ConfigNode configFileNode = null;

		static void showStatics()
		{
			Log.Info ("ROOT_PATH: " + ROOT_PATH);
			Log.Info ("CONFIG_BASE_FOLDER: " + CONFIG_BASE_FOLDER);
			Log.Info ("CI_BASE_FOLDER: " + CI_BASE_FOLDER);
			Log.Info ("CI_BASE_FOLDER: " + CI_BASE_FOLDER);
		}

#if (!_UNLIMITED_FILE_ACCESS)
		public static bool InsideApplicationRootPath(String path)
		{
			if (path == null) return false;
			try
			{
				String fullpath = Path.GetFullPath(path);
				return fullpath.StartsWith(Path.GetFullPath(ROOT_PATH));
			}
			catch
			{
				return false;
			}
		}
#endif
		public static bool ValidPathForWriteOperation(String path)
		{
#if (_UNLIMITED_FILE_ACCESS)
			return true;
#else
			String fullpath = Path.GetFullPath(path);
			return InsideApplicationRootPath(fullpath);
#endif
		}


		public static void SaveConfiguration (Configuration configuration, String file)
		{
			showStatics ();

			if (configFile == null) {
				configFile = new ConfigNode ();
			}
			if (!configFile.HasNode (CI_NODENAME)) {
				configFileNode = new ConfigNode (CI_NODENAME);
				configFile.SetNode (CI_NODENAME, configFileNode, true);
			} else {
				if (configFileNode == null) {
					configFileNode = configFile.GetNode (CI_NODENAME);
				}
			}

			configFileNode.SetValue ("useBlizzyToolbar", configuration.useBlizzyToolbar.ToString (), true);
			configFileNode.SetValue ("lastImportDir", configuration.lastImportDir, true);
			configFileNode.SetValue ("showDrives", configuration.showDrives.ToString (), true);
			configFileNode.SetValue ("userid", configuration.uid, true);
			configFileNode.SetValue ("password", configuration.pswd, true);
			configFileNode.SetValue ("showWarning", configuration.showWarning.ToString(), true);
			//configFileNode.SetValue ("ckanExecPath", configuration.ckanExecPath, true);

			configFileNode.SetValue ("vabResolution", configuration.vabResolution.ToString(), true);
			configFileNode.SetValue ("vabElevation", configuration.vabElevation.ToString(), true);
			configFileNode.SetValue ("vabAzimuth", configuration.vabAzimuth.ToString(), true);
			configFileNode.SetValue ("vabPitch", configuration.vabPitch.ToString(), true);
			configFileNode.SetValue ("vabHeading", configuration.vabHeading.ToString(), true);
			configFileNode.SetValue ("vabFov", configuration.vabFov.ToString(), true);

			configFileNode.SetValue ("sphResolution", configuration.sphResolution.ToString(), true);
			configFileNode.SetValue ("sphElevation", configuration.sphElevation.ToString(), true);
			configFileNode.SetValue ("sphAzimuth", configuration.sphAzimuth.ToString(), true);
			configFileNode.SetValue ("sphPitch", configuration.sphPitch.ToString(), true);
			configFileNode.SetValue ("sphHeading", configuration.sphHeading.ToString(), true);
			configFileNode.SetValue ("sphFov", configuration.sphFov.ToString(), true);

			configFileNode.SetValue ("backgroundR", configuration.backgroundcolor.r.ToString (), true);
			configFileNode.SetValue ("backgroundG", configuration.backgroundcolor.g.ToString (), true);
			configFileNode.SetValue ("backgroundB", configuration.backgroundcolor.b.ToString (), true);

			configFile.Save (CI_CFG_FILE);
		}

		//
		// The following functions are used when loading data from the config file
		// They make sure that if a value is missing, that the old value will be used.
		// 

		static string SafeLoad (string value, int oldvalue)
		{
			if (value == null)
				return oldvalue.ToString();
			return value;
		}

		static string SafeLoad (string value, float oldvalue)
		{
			if (value == null)
				return oldvalue.ToString();
			return value;
		}

		static string SafeLoad (string value, bool oldvalue)
		{
			if (value == null)
				return oldvalue.ToString();
			return value;
		}
		static string SafeLoad (string value, string oldvalue)
		{
			if (value == null)
				return oldvalue;
			return value;
		}
		public static void LoadConfiguration (Configuration configuration, String file)
		{
			showStatics ();

			configFile = ConfigNode.Load (CI_CFG_FILE);
	
			if (configFile != null) {
				configFileNode = configFile.GetNode (CI_NODENAME);
				if (configFileNode != null) {
					configuration.useBlizzyToolbar = bool.Parse (SafeLoad(configFileNode.GetValue ("useBlizzyToolbar"),configuration.useBlizzyToolbar));
					configuration.lastImportDir = SafeLoad(configFileNode.GetValue ("lastImportDir"),configuration.lastImportDir);
					configuration.showDrives = bool.Parse (SafeLoad(configFileNode.GetValue ("showDrives"),configuration.showDrives));
					configuration.pswd = SafeLoad(configFileNode.GetValue("password"), "");
					configuration.uid = SafeLoad(configFileNode.GetValue("userid"), "");
					configuration.showWarning = bool.Parse (SafeLoad(configFileNode.GetValue ("showWarning"),configuration.showWarning));

					configFileNode.SetValue ("vabResolution", configuration.vabResolution.ToString(), true);
					configFileNode.SetValue ("vabElevation", configuration.vabElevation.ToString(), true);
					configFileNode.SetValue ("vabAzimuth", configuration.vabAzimuth.ToString(), true);
					configFileNode.SetValue ("vabPitch", configuration.vabPitch.ToString(), true);
					configFileNode.SetValue ("vabHeading", configuration.vabHeading.ToString(), true);
					configFileNode.SetValue ("vabFov", configuration.vabFov.ToString(), true);

					configuration.vabResolution = int.Parse(SafeLoad(configFileNode.GetValue("vabResolution"), configuration.vabResolution));
					configuration.vabElevation = float.Parse(SafeLoad(configFileNode.GetValue("vabElevation"), configuration.vabElevation));
					configuration.vabAzimuth = float.Parse(SafeLoad(configFileNode.GetValue("vabAzimuth"), configuration.vabAzimuth));
					configuration.vabPitch = float.Parse(SafeLoad(configFileNode.GetValue("vabPitch"), configuration.vabPitch));
					configuration.vabHeading = float.Parse(SafeLoad(configFileNode.GetValue("vabHeading"), configuration.vabHeading));
					configuration.vabFov = float.Parse(SafeLoad(configFileNode.GetValue("vabFov"), configuration.vabFov));

					configuration.sphResolution = int.Parse(SafeLoad(configFileNode.GetValue("sphResolution"), configuration.sphResolution));
					configuration.sphElevation = float.Parse(SafeLoad(configFileNode.GetValue("sphElevation"), configuration.sphElevation));
					configuration.sphAzimuth = float.Parse(SafeLoad(configFileNode.GetValue("sphAzimuth"), configuration.sphAzimuth));
					configuration.sphPitch = float.Parse(SafeLoad(configFileNode.GetValue("sphPitch"), configuration.sphPitch));
					configuration.sphHeading = float.Parse(SafeLoad(configFileNode.GetValue("sphHeading"), configuration.sphHeading));
					configuration.sphFov = float.Parse(SafeLoad(configFileNode.GetValue("sphFov"), configuration.sphFov));

					configFileNode.SetValue ("backgroundR", configuration.backgroundcolor.r.ToString (), true);
					configFileNode.SetValue ("backgroundG", configuration.backgroundcolor.g.ToString (), true);
					configFileNode.SetValue ("backgroundB", configuration.backgroundcolor.b.ToString (), true);

					float r = configuration.backgroundcolor.r;
					float g = configuration.backgroundcolor.g;
					float b = configuration.backgroundcolor.b;
					r = float.Parse(SafeLoad(configFileNode.GetValue("backgroundR"), r));				
					g = float.Parse(SafeLoad(configFileNode.GetValue("backgroundR"), g));				
					b = float.Parse(SafeLoad(configFileNode.GetValue("backgroundR"), b));				
					configuration.backgroundcolor.r = r;
					configuration.backgroundcolor.g = g;
					configuration.backgroundcolor.b = b;

					//configuration.ckanExecPath = SafeLoad(configFileNode.GetValue("ckanExecPath"), configuration.ckanExecPath);
				}
			}
		}
	}
}
