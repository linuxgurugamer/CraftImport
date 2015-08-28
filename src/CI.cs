

using System;
using UnityEngine;
using KSP.IO;


//using System.Globalization;

namespace CraftImport
{
	
	public enum UIOnScreenshot : ushort
	{
		show = 0,
		hide,
		both}

	;

	[KSPAddon (KSPAddon.Startup.MainMenu, true)]
	public partial class CI : MonoBehaviour
	{

		public const String TITLE = "Craft Import";

		//			public Configuration configuration = Configuration.Instance;
		//			public static AS Instance { get; private set;}

		//		private float snapshotInterval = 5.0f;


		public static bool changeCallbacks;
		public static Configuration configuration = new Configuration ();
		public static KeyCode activeKeycode;
		static private UICLASS uiVisiblity;
		public MainMenuGui gui = null;


		static CI ()
		{
		}

		public CI ()
		{
			Log.Info ("New instance of Craft Import: CI constructor");
		}

		public void Awake ()
		{
			Log.Info ("Awake");
			uiVisiblity = new UICLASS ();
			uiVisiblity.Awake ();

		}

		public void Start ()
		{

			Log.Info ("Start");
			DontDestroyOnLoad (this);
			configuration.Load ();
#if (DEBUG)
			Log.SetLevel (Log.LEVEL.INFO);
#else
			Log.SetLevel (configuration.logLevel);
#endif
			configuration.BlizzyToolbarIsAvailable = ToolbarManager.ToolbarAvailable;
			Log.Info ("BlizzyToolbarIsAvailable: " + configuration.BlizzyToolbarIsAvailable.ToString ());

			if (configuration.BlizzyToolbarIsAvailable) {
				InitToolbarButton ();
				if (configuration.useBlizzyToolbar) {
					setToolbarButtonVisibility (true);
				} else {
					setToolbarButtonVisibility (false);
				}
			}

		}

		public void Update ()
		{
			if (this.gui == null) {
				Log.Info ("this.gui == null");
				this.gui = this.gameObject.AddComponent<MainMenuGui> ();
				this.gui.SetVisible (false);
			}

			if (HighLogic.LoadedScene == GameScenes.MAINMENU) {
				if (!configuration.BlizzyToolbarIsAvailable || !configuration.useBlizzyToolbar)
					gui.OnGUIHideApplicationLauncher ();
			} else {
				if (!configuration.BlizzyToolbarIsAvailable || !configuration.useBlizzyToolbar) {
					if (MainMenuGui.CI_Button == null)
						GameEvents.onGUIApplicationLauncherReady.Add (gui.OnGUIApplicationLauncherReady);
					gui.OnGUIShowApplicationLauncher ();
				} else {
					setToolbarButtonVisibility (true);
				}
			}
		}


		internal void OnDestroy ()
		{
			Log.Info ("destroying CraftImport");
			DelToolbarButton ();
			configuration.Save ();
		}


		public static KeyCode setActiveKeycode (string keycode)
		{
			CI.activeKeycode = (KeyCode)Enum.Parse (typeof(KeyCode), keycode);
			if (CI.activeKeycode == KeyCode.None) {
				Log.Warning ("Make sure to use the list of keys to set the key! Reverting to F6");
				CI.activeKeycode = KeyCode.F6;
			}
		
			return CI.activeKeycode;
		}
	}
}