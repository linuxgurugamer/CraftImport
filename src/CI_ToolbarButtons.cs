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

		public void ToolbarToggle()
		{
			Log.Info ("ToolbarToggle, visible: " + gui.Visible().ToString());
			if (gui.Visible ()) {
				gui.endGUIToggle ();
				gui.SetVisible (false);
				GUI.enabled = false;
				gui.GUI_SaveData ();

                configuration.Save ();
 
			} else {
				gui.initGUIToggle ();

				GUI.enabled = true;
			}
		}

    }
}
