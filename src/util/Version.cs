using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
#if false
// Thanks to nightingale for this file
// Original file: A https://github.com/jrossignol/ContractConfigurator/blob/master/source/ContractConfigurator/Util/Version.cs#L29
//
namespace CraftImport
{
	/// <summary>
	/// Utility class with version checking functionality.
	/// </summary>
	public static class Version
	{


		/// <summary>
		/// Verify the loaded assembly meets a minimum version number.
		/// </summary>
		/// <param name="name">Assembly name</param>
		/// <param name="version">Minium version</param>
		/// <param name="silent">Silent mode</param>
		/// <returns>The assembly if the version check was successful.  If not, logs and error and returns null.</returns>
		public static Assembly VerifyAssemblyVersion (string name, string version, bool silent = false)
		{
			Log.Info ("Entering VerifyAssemblyVersion");
			// Logic courtesy of DMagic
			var assembly = AssemblyLoader.loadedAssemblies.SingleOrDefault (a => a.assembly.GetName ().Name == name);
			if (assembly != null) {
				string receivedStr;

				// First try the informational version
				var ainfoV = Attribute.GetCustomAttribute (assembly.assembly, typeof(AssemblyInformationalVersionAttribute)) as AssemblyInformationalVersionAttribute;
				if (ainfoV != null) {
					receivedStr = ainfoV.InformationalVersion;
				}
				// If that fails, use the product version
				else {
					receivedStr = FileVersionInfo.GetVersionInfo (assembly.assembly.Location).ProductVersion;
				}

				System.Version expected = ParseVersion (version);
				System.Version received = ParseVersion (receivedStr);

				if (received >= expected) {
					Log.Info ("Version check for '" + name + "' passed.  Minimum required is " + version + ", version found was " + receivedStr);
					return assembly.assembly;
				} else {
					Log.Error ("Version check for '" + name + "' failed!  Minimum required is " + version + ", version found was " + receivedStr);
					return null;
				}
			} else {
				Log.Error ("Couldn't find assembly for '" + name + "'!");
				return null;
			}
		}

		private static System.Version ParseVersion (string version)
		{
			Match m = Regex.Match (version, @"^[vV]?(\d+)(.(\d+)(.(\d+)(.(\d+))?)?)?");
			int major = m.Groups [1].Value.Equals ("") ? 0 : Convert.ToInt32 (m.Groups [1].Value);
			int minor = m.Groups [3].Value.Equals ("") ? 0 : Convert.ToInt32 (m.Groups [3].Value);
			int build = m.Groups [5].Value.Equals ("") ? 0 : Convert.ToInt32 (m.Groups [5].Value);
			int revision = m.Groups [7].Value.Equals ("") ? 0 : Convert.ToInt32 (m.Groups [7].Value);

			return new System.Version (major, minor, build, revision);
		}
			
	}

// Leaving this here in case I need it in some future mod
#if (false)
		/// <summary>
		/// Checks if running KSP on Win64.
		/// </summary>
		/// <returns></returns>
		public static bool IsWin64 ()
		{
			if (isWin64 == null) {
				IntPtr intPtr = new IntPtr (long.MaxValue);
				isWin64 = (intPtr.ToInt64 () > uint.MaxValue) && (Environment.OSVersion.Platform == PlatformID.Win32NT);
			}
			return isWin64.Value;
		}

		private static bool? isWin64 = null;
#endif

}
#endif