using System;
using UnityEngine;


//using System.Globalization;

namespace CraftImport
{

	[KSPAddon (KSPAddon.Startup.EditorAny, false)]
	public partial class CI : MonoBehaviour
	{
        internal static CI Instance;

		public const String TITLE = "Craft Import";

		public /*static*/ Configuration configuration = null;

		public MainMenuGui gui = null;



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

			if (InputLockManager.GetControlLock ("CraftImportLock") != ControlTypes.None) {
				InputLockManager.RemoveControlLock ("CraftImportLock");
			}

            MainMenuGui.Instance.toolbarControl.OnDestroy();
            Destroy(MainMenuGui.Instance.toolbarControl);

            configuration.Save ();
		}


	}
}