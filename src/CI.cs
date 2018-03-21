

using System;
using UnityEngine;
//using KSP.IO;
using KSP.UI.Screens;

//using System.Globalization;

namespace CraftImport
{

	[KSPAddon (KSPAddon.Startup.EditorAny, false)]
	public partial class CI : MonoBehaviour
	{
        internal static CI Instance;

		public const String TITLE = "Craft Import";

		//			public Configuration configuration = Configuration.Instance;
		//			public static AS Instance { get; private set;}

		//		private float snapshotInterval = 5.0f;


		//public static bool changeCallbacks;

		public /*static*/ Configuration configuration = null;
//		public static KeyCode activeKeycode;
//		static private UICLASS uiVisiblity;
		public MainMenuGui gui = null;


		/*
		 * static CI ()
		{
		}
*/
//		public CI ()
//		{
//			Log.Info ("New instance of Craft Import: CI constructor");
//		}

//		public void Awake ()
//		{
//			uiVisiblity = new UICLASS ();
//			uiVisiblity.Awake ();
//		}

		public void Start ()
		{
            Instance = this;
			//DontDestroyOnLoad (this);
			configuration = new Configuration ();
			configuration.Load ();
#if (DEBUG)
			Log.SetLevel (Log.LEVEL.INFO);
#else
			Log.SetLevel (configuration.logLevel);
#endif

			if (this.gui == null) {
				this.gui = this.gameObject.AddComponent<MainMenuGui> ();
				//gui.UpdateToolbarStock ();
				gui.thisCI = this;
				gui.SetVisible (false);

			}

			configuration.BlizzyToolbarIsAvailable = ToolbarManager.ToolbarAvailable;

        }

        public void Update ()
		{

			if (HighLogic.LoadedScene == GameScenes.EDITOR) {

                gui.UpdateToolbarStock();
            }

        }


        public void OnDestroy ()
		{
			Log.Info ("destroying CraftImport");

//			uiVisiblity.OnDestroy();
			if (InputLockManager.GetControlLock ("CraftImportLock") != ControlTypes.None) {
				InputLockManager.RemoveControlLock ("CraftImportLock");
			}

            MainMenuGui.Instance.toolbarControl.OnDestroy();
            Destroy(MainMenuGui.Instance.toolbarControl);

            //	DelToolbarButton ();
            configuration.Save ();
		}


	}
}