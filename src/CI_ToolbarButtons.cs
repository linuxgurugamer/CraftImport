using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace CraftImport
{
    public partial class CI
    {
		static IButton btnReturn = null;
		private const string _tooltipOn = "Hide Craft Import";
		private const string _tooltipOff = "Show Craft Import";
		public const string TEXTURE_DIR = "CraftImport/Textures/";

		public void setToolbarButtonVisibility(bool v)
		{

			btnReturn.Visible = v;
		}

		public void ToolbarToggle()
		{
			if (gui.Visible ()) {
				gui.SetVisible (false);
				GUI.enabled = false;
				btnReturn.ToolTip = _tooltipOff;
				gui.GUI_SaveData ();

				if (CI.configuration.BlizzyToolbarIsAvailable && CI.configuration.useBlizzyToolbar) {
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
				configuration.Save ();
				ToolBarActive ();
			} else {
				gui.SetVisible (true);
				GUI.enabled = true;
				btnReturn.ToolTip = _tooltipOn;

				btnReturn.TexturePath = TEXTURE_DIR + "CI-24";
			}
		}

		public /*static*/ void  ToolBarActive()
		{
				btnReturn.TexturePath = TEXTURE_DIR + "CI-24";
		}


        /// <summary>
        /// initialises a Toolbar Button for this mod
        /// </summary>
        /// <returns>The ToolbarButtonWrapper that was created</returns>
        public void InitToolbarButton()
        {
            
            try
            {
                btnReturn = ToolbarManager.Instance.add("CraftImport", "btnReturn");
//				btnReturn.TexturePath = "SpaceTux/AS/Textures/AS_24_white";
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

    }
}
