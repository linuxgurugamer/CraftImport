using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace CraftImport
{
	public partial class CI : MonoBehaviour
    {
		//static IButton btnReturn = null;
		private const string _tooltipOn = "Hide Craft Import";
		private const string _tooltipOff = "Show Craft Import";
		public const string MOD_DIR = "CraftImport/";
		public const string TEXTURE_DIR = MOD_DIR + "Textures/";

#if false
        public void setToolbarButtonVisibility(bool v)
		{

			btnReturn.Visible = v;
		}
#endif
		public void ToolbarToggle()
		{
			Log.Info ("ToolbarToggle, visible: " + gui.Visible().ToString());
			if (gui.Visible ()) {
				gui.endGUIToggle ();
				gui.SetVisible (false);
				GUI.enabled = false;
				gui.GUI_SaveData ();
#if false
                
				btnReturn.ToolTip = _tooltipOff;
                if (gui.thisCI.configuration.BlizzyToolbarIsAvailable && gui.thisCI.configuration.useBlizzyToolbar) {
					btnReturn.TexturePath = TEXTURE_DIR + "CI-24";
					gui.OnGUIHideApplicationLauncher ();
					//InitToolbarButton ();
				} else {

					//gui.OnGUIShowApplicationLauncher ();
					CIInfoDisplay.infoDisplayActive = false;
					//GameEvents.onGUIApplicationLauncherReady.Add (gui.OnGUIApplicationLauncherReady);

					//gui.setAppLauncherHidden ();
					gui.OnGUIApplicationLauncherReady();

					//GameEvents.onGUIApplicationLauncherReady.Add (gui.OnGUIApplicationLauncherReady);
					gui.OnGUIShowApplicationLauncher ();
					// Hide blizzy toolbar button
					setToolbarButtonVisibility(false);

				}
                               ToolBarActive();
#endif
                configuration.Save ();
 
			} else {
				gui.initGUIToggle ();
				//gui.SetVisible (true);
				GUI.enabled = true;
#if fale
                btnReturn.ToolTip = _tooltipOn;

				btnReturn.TexturePath = TEXTURE_DIR + "CI-24";
#endif
				//InputLockManager.SetControlLock((ControlTypes.EDITOR_LOCK | ControlTypes.EDITOR_GIZMO_TOOLS), "CraftImportLock");
			}
		}
#if false
        public /*static*/ void  ToolBarActive()
		{
				btnReturn.TexturePath = TEXTURE_DIR + "CI-24";
		}
#endif
#if false

        /// <summary>
        /// initialises a Toolbar Button for this mod
        /// </summary>
        /// <returns>The ToolbarButtonWrapper that was created</returns>
        public void InitToolbarButton()
        {
            
            try
            {
                btnReturn = ToolbarManager.Instance.add("CraftImport", "btnReturn");
				btnReturn.Visibility = new GameScenesVisibility(GameScenes.EDITOR);
				btnReturn.TexturePath = TEXTURE_DIR + "CI-24";
				btnReturn.ToolTip = TITLE;
				btnReturn.OnClick += e => ToolbarToggle();
            }
            catch (Exception ex)
            {
                DestroyToolbarButton(btnReturn);
				Log.Info("Error Initialising Toolbar Button: " + ex.Message);
            }
            return;
        }

		public void DelToolbarButton()
		{
			DestroyToolbarButton(btnReturn);
		}
        /// <summary>
        /// Destroys theToolbarButtonWrapper object
        /// </summary>
        /// <param name="btnToDestroy">Object to Destroy</param>
		static  void DestroyToolbarButton(IButton btnToDestroy)
        {
            if (btnToDestroy != null)
            {
				Log.Info("Destroying Toolbar Button");
                btnToDestroy.Destroy();
            }
            btnToDestroy = null;
        }
#endif
    }
}
