
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
		public static readonly String ROOT_PATH = KSPUtil.ApplicationRootPath;
		private static readonly String CONFIG_BASE_FOLDER = ROOT_PATH + "GameData/";
		private static String CI_BASE_FOLDER = CONFIG_BASE_FOLDER + "CraftImport/";
		private const String CI_NODENAME = "CraftImport";
		private static readonly String CI_CFG_FILE = CI_BASE_FOLDER + "CI_Settings.cfg";

		private static ConfigNode configFile = null;
		private static ConfigNode configFileNode = null;



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
			//configFileNode.SetValue ("ckanExecPath", configuration.ckanExecPath, true);
			configFile.Save (CI_CFG_FILE);
		}

		//
		// The following functions are used when loading data from the config file
		// They make sure that if a value is missing, that the old value will be used.
		// 

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
			configFile = ConfigNode.Load (CI_CFG_FILE);
	
			if (configFile != null) {
				configFileNode = configFile.GetNode (CI_NODENAME);
				if (configFileNode != null) {
					configuration.useBlizzyToolbar = bool.Parse (SafeLoad(configFileNode.GetValue ("useBlizzyToolbar"),configuration.useBlizzyToolbar));
					configuration.lastImportDir = SafeLoad(configFileNode.GetValue ("lastImportDir"),configuration.lastImportDir);
					configuration.showDrives = bool.Parse (SafeLoad(configFileNode.GetValue ("showDrives"),configuration.showDrives));
					//configuration.ckanExecPath = SafeLoad(configFileNode.GetValue("ckanExecPath"), configuration.ckanExecPath);
				}
			}
		}
	}
}
