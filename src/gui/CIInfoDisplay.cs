
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace CraftImport
{
		
	class CIInfoDisplay
	{

		//Singleton

		private static CIInfoDisplay instance = null;

		public static CIInfoDisplay Instance {
			get {
				if (instance == null)
					instance = new CIInfoDisplay ();
				return instance;
			}
		}

		//Properties


		public static bool infoDisplayActive = false;
		public static bool infoDisplayMinimized = false;
		public static bool infoDisplayDetailed = false;
		public static bool infoDisplayOptions = false;
	}
}
