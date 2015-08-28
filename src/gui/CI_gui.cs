using System;
using UnityEngine;
using System.IO;
using System.Text.RegularExpressions;


namespace CraftImport
{
	public class MainMenuGui : MonoBehaviour
	{
		private const int WIDTH = 500;
		private const int HEIGHT = 175;
		private Rect bounds = new Rect (Screen.width / 2 - WIDTH / 2, Screen.height / 2 - HEIGHT / 2, WIDTH, HEIGHT);
		private /* volatile*/ bool visible = false;
		// Stock APP Toolbar - Stavell
		public static ApplicationLauncherButton CI_Button = null;
		public  bool stockToolBarcreated = false;

		public static Texture2D CI_button_img = new Texture2D (38, 38, TextureFormat.ARGB32, false);

		private bool CI_Texture_Load = false;

		private bool cfgWinData = false;
		//		private static bool newScreenshotAtIntervals = true;
		private static bool blizzyToolbarInstalled = false;
		private static bool newUseBlizzyToolbar;
		private static bool newShowDrives;
		private static bool saveInSandbox = false;
		private static bool overwriteExisting = false;
		private static bool appLaucherHidden = true;
		private static string craftURL = "";
		private static string newCraftName = "";

		enum downloadStateType
		{
			INACTIVE,
			GUI,
			FILESELECTION,
			INPROGRESS,
			COMPLETED,
			FILEEXISTS,
			ERROR
		};

		private static downloadStateType downloadState = downloadStateType.INACTIVE;
		private static string downloadErrorMessage;

		internal MainMenuGui ()
		{
			
			blizzyToolbarInstalled = ToolbarManager.ToolbarAvailable;
		}

		public void setAppLauncherHidden ()
		{
			appLaucherHidden = true;
		}

		public void OnGUIHideApplicationLauncher ()
		{
			if (!appLaucherHidden) {

				if (CI.configuration.BlizzyToolbarIsAvailable && CI.configuration.useBlizzyToolbar) {
					HideToolbarStock ();
					appLaucherHidden = true;
				}

			}
		}

		public void OnGUIShowApplicationLauncher ()
		{
			if (!CI.configuration.BlizzyToolbarIsAvailable || !CI.configuration.useBlizzyToolbar) {
				if (appLaucherHidden) {
					appLaucherHidden = false;
					if (CI_Button != null)
						UpdateToolbarStock ();
				}
			}
		}

		public void OnGUIApplicationLauncherReady ()
		{
			UpdateToolbarStock ();
		}

		private void UpdateToolbarStock ()
		{
			Log.Info ("UpdateToolbarStock, appLaucherHidden: " + appLaucherHidden.ToString ());
			if (CI_Button != null)
				Log.Info ("CI_Button not null");
			// Create the button in the KSP AppLauncher
			if (!CI_Texture_Load) {
				if (GameDatabase.Instance.ExistsTexture (CI.TEXTURE_DIR + "CI-38"))
					CI_button_img = GameDatabase.Instance.GetTexture (CI.TEXTURE_DIR + "CI-38", false);
				//if (GameDatabase.Instance.ExistsTexture (CI.TEXTURE_DIR + "folder"))
				//	CI_button_off = GameDatabase.Instance.GetTexture (CI.TEXTURE_DIR + "folder", false);
				

				CI_Texture_Load = true;
			}
			if (CI_Button == null && !appLaucherHidden) {
				Log.Info ("CI_Button == null");
				
				CI_Button = ApplicationLauncher.Instance.AddModApplication (GUIToggle, GUIToggle,
					null, null,
					null, null,
					ApplicationLauncher.AppScenes.SPACECENTER | ApplicationLauncher.AppScenes.SPH | ApplicationLauncher.AppScenes.VAB,
					CI_button_img);
				Log.Info ("Added");
				stockToolBarcreated = true;
			}
		}

		private void HideToolbarStock ()
		{
			Log.Info ("HideToolbarStock");
			ApplicationLauncher.Instance.RemoveModApplication (MainMenuGui.CI_Button);
			Destroy (CI_Button); // Is this necessary?
			CI_Button = null;
			appLaucherHidden = false;
		}

		public bool Visible ()
		{ 
			return this.visible;
		}

		public void SetVisible (bool visible)
		{
			this.visible = visible;
		}

		void saveCraftFile (string craftFile)
		{
			//Log.Info ("saveCraftFile: " + craftFile);
			string saveDir = "";
			if (saveInSandbox) {
				saveDir = KSPUtil.ApplicationRootPath + "ships";
			} else {
				saveDir = KSPUtil.ApplicationRootPath + "saves/" + HighLogic.SaveFolder + "/ships";
			}
			//string saveDir = KSPUtil.ApplicationRootPath + sandbox + HighLogic.SaveFolder + "/ships";
			string saveFile = "";
			Log.Info ("dir: " + saveDir);
			var s = Regex.Match (craftFile, "^ship.*=.*");
			s = Regex.Match (s.ToString (), "=.*");

			string shipName = s.ToString ().Remove (0, 1).Trim ();
			var t = Regex.Match (craftFile, "type.*=.*");
			Log.Info ("regex: " + t.ToString ());
			Log.Info ("t: " + t);
			bool vab = t.ToString ().Contains ("VAB");
			bool sph = t.ToString ().Contains ("SPH");

			string strCraftFile = craftFile.ToString ();
			if (newCraftName != "") {
				Log.Info ("shipName: " + shipName + "     newCraftName: " + newCraftName);
				strCraftFile = strCraftFile.Replace (shipName, newCraftName);
				shipName = newCraftName;
			}

			if (vab) {
				saveFile = saveDir + "/VAB/" + shipName + ".craft";
			}
			if (sph) {
				saveFile = saveDir + "/SPH/" + shipName + ".craft";
			}


			if (System.IO.File.Exists (saveFile) && !overwriteExisting) {
				downloadState = downloadStateType.FILEEXISTS;
				return;
			}

			System.IO.File.WriteAllText (saveFile, strCraftFile);
			downloadState = downloadStateType.COMPLETED;
		}

		System.Collections.IEnumerator doDownloadCraft (string craftURL)
		{
			Log.Info ("doDownloadCraft   craftURL: " + craftURL);
			if (craftURL != "") {
				string s = System.Uri.EscapeUriString (craftURL);
				Log.Info ("Import started");
				Log.Info (" Escaped craftURL: " + s);

				// some simple error checking
				if (!s.StartsWith ("http://") && !s.StartsWith ("https://") && !s.StartsWith ("ftp://") && !s.StartsWith ("file://")) {
					downloadErrorMessage = "Invalid URL or file specified";
					downloadState = downloadStateType.ERROR;
					yield break;
				}

				// Create a download object
				var download = new WWW (s);
				// Wait until the download is done
				yield   return download;
				if (!String.IsNullOrEmpty (download.error)) {
					Log.Error ("Error downloading: " + download.error);
					downloadErrorMessage = download.error;
					downloadState = downloadStateType.ERROR;
					yield break;
				} else {
					saveCraftFile (download.text);
					craftURL = "";
					saveInSandbox = false;
					overwriteExisting = false;
					//downloadState = downloadStateType.COMPLETED;
				}
			}
		}

		void downloadCraft (string craftURL)
		{
			Log.Info ("downloadCraft");
			downloadState = downloadStateType.INPROGRESS;
			StartCoroutine (doDownloadCraft (craftURL));
		}

		/// <summary>
		/// //////////////////////////////////////////////
		/// </summary>
		protected string m_textPath;

		static protected FileBrowser m_fileBrowser = null;
		static bool fileBrowserEnabled = false;

		[SerializeField]
		protected Texture2D m_directoryImage,
			m_fileImage;


		 void getFile ()
		{
			GUILayout.BeginHorizontal ();
			GUILayout.Label ("Select Craft File", GUILayout.Width (100));
			GUILayout.FlexibleSpace ();
			GUILayout.Label (m_textPath ?? "none selected");
			fileBrowserEnabled = true;
			//if (!m_directoryImage) Log.Info ("getFile 1 m_directoryImage is null");
			if (GUILayout.Button ("...", GUILayout.ExpandWidth (false))) {
//				Log.Info ("creating m_fileBrowser");

				m_fileBrowser = new FileBrowser (
					new Rect (100, 100, 600, 500),
					"Choose Craft File",
					FileSelectedCallback
				);

				if (!m_directoryImage) {
					m_directoryImage = new Texture2D (38, 38, TextureFormat.ARGB32, false);
					if (GameDatabase.Instance.ExistsTexture (CI.TEXTURE_DIR + "folder"))
						m_directoryImage = GameDatabase.Instance.GetTexture (CI.TEXTURE_DIR + "folder", false);
				}
				if (!m_fileImage) {
					m_fileImage =  new Texture2D (38, 38, TextureFormat.ARGB32, false);
					if (GameDatabase.Instance.ExistsTexture (CI.TEXTURE_DIR + "file"))
						m_fileImage = GameDatabase.Instance.GetTexture (CI.TEXTURE_DIR + "file", false);
				}

				m_fileBrowser.SelectionPattern = "*.craft";
				m_fileBrowser.DirectoryImage = m_directoryImage;
				m_fileBrowser.FileImage = m_fileImage;
				if (m_textPath != "")
					m_fileBrowser.SetNewDirectory(m_textPath);
			}

			GUILayout.EndHorizontal ();
			GUILayout.BeginHorizontal ();
			if (GUILayout.Button ("OK", GUILayout.Width (125.0f))) {;
				fileBrowserEnabled = false;
				downloadState = downloadStateType.GUI;
				if (m_textPath != "")
					craftURL = "file://" + m_textPath;
				//m_fileBrowser = null;
			}

			if (GUILayout.Button ("Cancel", GUILayout.Width (125.0f))) {
//				Log.Info ("Cancel2");
				fileBrowserEnabled = false;
				downloadState = downloadStateType.GUI;
				//m_fileBrowser = null;
			}
			GUILayout.EndHorizontal ();
		}

		protected void FileSelectedCallback (string path)
		{
//			Log.Info ("FileSelectedCallback");
			m_fileBrowser = null;
			fileBrowserEnabled = false;

			m_textPath = path;
			CI.configuration.lastImportDir = System.IO.Path.GetDirectoryName(m_textPath);
		}


		/////////////////////////////////////
		public void OnGUI ()
		{
			if (m_fileBrowser != null) {
				if (!fileBrowserEnabled) {
					m_fileBrowser = null;
					downloadState = downloadStateType.GUI;
				} else {
					// For some reason, the following code, which does load the images
					// doesn't work in the getFile;  In the getFile() function, the images are
					// still null, so I moved this code into that
					#if false
				if (!m_directoryImage) {
					Log.Info ("Loading m_directoryImage");
					m_directoryImage = new Texture2D (38, 38, TextureFormat.ARGB32, false);
					if (GameDatabase.Instance.ExistsTexture (CI.TEXTURE_DIR + "folder"))
						m_directoryImage = GameDatabase.Instance.GetTexture (CI.TEXTURE_DIR + "folder", false);
				}
				if (!m_fileImage) {
					Log.Info ("Loading m_fileImage");
					m_fileImage =  new Texture2D (38, 38, TextureFormat.ARGB32, false);
					if (GameDatabase.Instance.ExistsTexture (CI.TEXTURE_DIR + "file"))
						m_fileImage = GameDatabase.Instance.GetTexture (CI.TEXTURE_DIR + "file", false);
				}
					#endif

				//	GUI.skin = HighLogic.Skin;
		
					GUIStyle s = new GUIStyle (HighLogic.Skin.textField);

					s.onNormal = HighLogic.Skin.textField.onNormal;

		//			s.fontSize = 15;
					s.name = "List Item";
					s.alignment = TextAnchor.MiddleLeft;
					//s.fontStyle = FontStyle.Bold;
					//s.fixedHeight = 50;
					s.imagePosition = ImagePosition.ImageLeft;

					GUI.skin.customStyles [0] = s;

					m_fileBrowser.OnGUI ();
					return;
				}
			}
			try {
				if (this.Visible ()) {
					this.bounds = GUILayout.Window (this.GetInstanceID (), this.bounds, this.Window, CI.TITLE, HighLogic.Skin.window);
				}
			} catch (Exception e) {
				Log.Error ("exception: " + e.Message);
			}
		}

		private void Window (int id)
		{
			if (cfgWinData == false) {
				cfgWinData = true;
				newUseBlizzyToolbar = CI.configuration.useBlizzyToolbar;
				newShowDrives = CI.configuration.showDrives;
				craftURL = "";
				saveInSandbox = false;
				overwriteExisting = false;
				m_textPath = CI.configuration.lastImportDir;
			} 

			SetVisible (true);
			GUI.enabled = true;

			if (downloadState == downloadStateType.FILESELECTION)
			{
				getFile();
				return;
			}

			GUILayout.BeginHorizontal ();
			GUILayout.EndHorizontal ();

			GUILayout.BeginVertical ();

			DrawTitle ("Craft Import");


			switch (downloadState) {

			case downloadStateType.GUI:
				GUILayout.BeginHorizontal ();
				GUILayout.Label ("Enter Craft URL:");
				GUILayout.FlexibleSpace ();
				//GUILayout.EndHorizontal ();

				//GUILayout.BeginHorizontal ();
				GUILayout.FlexibleSpace ();
				craftURL = GUILayout.TextField (craftURL, GUILayout.MinWidth (50F), GUILayout.MaxWidth (300F));
				GUILayout.EndHorizontal ();

				GUILayout.BeginHorizontal ();
				GUILayout.Label ("Rename craft to:");
				GUILayout.FlexibleSpace ();
				//GUILayout.EndHorizontal ();
				//GUILayout.BeginHorizontal ();
				GUILayout.FlexibleSpace ();
				newCraftName = GUILayout.TextField (newCraftName, GUILayout.MinWidth (50F), GUILayout.MaxWidth (300F));
				GUILayout.EndHorizontal ();

				GUILayout.BeginHorizontal ();
				GUILayout.Label ("Save in sandbox directory:");
				GUILayout.FlexibleSpace ();
				saveInSandbox = GUILayout.Toggle (saveInSandbox, "");
				GUILayout.EndHorizontal ();

				GUILayout.BeginHorizontal ();
				GUILayout.Label ("Overwrite existing file if exists:");
				GUILayout.FlexibleSpace ();
				overwriteExisting = GUILayout.Toggle (overwriteExisting, "");
				GUILayout.EndHorizontal ();

				GUILayout.BeginHorizontal ();
				GUILayout.Label ("Use Blizzy Toolbar if available:");
				GUILayout.FlexibleSpace ();
				newUseBlizzyToolbar = GUILayout.Toggle (newUseBlizzyToolbar, "");
				GUILayout.EndHorizontal ();
				if (Application.platform == RuntimePlatform.WindowsPlayer) {
					GUILayout.BeginHorizontal ();
					GUILayout.Label ("Show drive letters:");
					GUILayout.FlexibleSpace ();
					newShowDrives = GUILayout.Toggle (newShowDrives, "");
					GUILayout.EndHorizontal ();
				}

				GUILayout.BeginHorizontal ();
				if (GUILayout.Button ("Select Local File", GUILayout.Width (125.0f))) {
					Log.Info ("Selecting Local File");
					downloadState = downloadStateType.FILESELECTION;
					//var path = Editor
					//newCraftName = 
				}
				GUILayout.EndHorizontal ();

				GUILayout.BeginHorizontal ();
				GUILayout.EndHorizontal ();

				GUILayout.BeginHorizontal ();
				if (GUILayout.Button ("Import", GUILayout.Width (125.0f))) {
					Log.Info ("Calling downloadCraft");
					downloadCraft (craftURL);
				}

				GUILayout.FlexibleSpace ();
				if (GUILayout.Button ("Cancel", GUILayout.Width (125.0f))) {

					GUIToggle ();
				}

				GUILayout.EndHorizontal ();
				break;

//			case downloadStateType.FILESELECTION:
//				getFile ();
//				break;

			case downloadStateType.INPROGRESS:
				GUILayout.BeginHorizontal ();
				GUILayout.FlexibleSpace ();
				GUILayout.Label ("Download in progress");
				GUILayout.FlexibleSpace ();
				GUILayout.EndHorizontal ();
				GUILayout.BeginHorizontal ();
				if (GUILayout.Button ("Cancel", GUILayout.Width (125.0f))) {
					Log.Info ("Cancel");
				}
				GUILayout.EndHorizontal ();
				break;

			case downloadStateType.COMPLETED:
				GUILayout.BeginHorizontal ();
				GUILayout.FlexibleSpace ();
				GUILayout.Label ("Download completed");
				GUILayout.FlexibleSpace ();
				GUILayout.EndHorizontal ();
				GUILayout.BeginHorizontal ();
				if (GUILayout.Button ("OK", GUILayout.Width (125.0f))) {
					Log.Info ("OK");
					SetVisible (false);
					GUI.enabled = false;
					CIInfoDisplay.infoDisplayActive = false;
				}
				GUILayout.EndHorizontal ();
				break;

			case downloadStateType.FILEEXISTS:
				GUILayout.BeginHorizontal ();
				GUILayout.FlexibleSpace ();
				GUILayout.Label ("File Exists Error");
				GUILayout.FlexibleSpace ();
				GUILayout.EndHorizontal ();
				GUILayout.BeginHorizontal ();
				GUILayout.FlexibleSpace ();
				GUILayout.Label ("The craft file already exists, will NOT be overwritten");
				GUILayout.FlexibleSpace ();
				GUILayout.EndHorizontal ();
				GUILayout.BeginHorizontal ();
				if (GUILayout.Button ("OK", GUILayout.Width (125.0f))) {
					craftURL = "";
					newCraftName = "";
					saveInSandbox = false;
					overwriteExisting = false;
					SetVisible (false);
					GUI.enabled = false;
					CIInfoDisplay.infoDisplayActive = false;
				}
				GUILayout.EndHorizontal ();
				break;

			case downloadStateType.ERROR:
				GUILayout.BeginHorizontal ();
				GUILayout.FlexibleSpace ();
				GUILayout.Label ("Download Error");
				GUILayout.FlexibleSpace ();
				GUILayout.EndHorizontal ();
				GUILayout.BeginHorizontal ();
				GUILayout.FlexibleSpace ();
				GUILayout.Label (downloadErrorMessage);
				GUILayout.FlexibleSpace ();
				GUILayout.EndHorizontal ();
				GUILayout.BeginHorizontal ();
				if (GUILayout.Button ("Cancel", GUILayout.Width (125.0f))) {
					Log.Info ("Cancel");
					craftURL = "";
					newCraftName = "";
					saveInSandbox = false;
					overwriteExisting = false;
					SetVisible (false);
					GUI.enabled = false;
					CIInfoDisplay.infoDisplayActive = false;
				}
				GUILayout.EndHorizontal ();
				break;

			default:
				break;
			}
			GUILayout.EndVertical ();
		}

		private void DrawTitle (String text)
		{
			GUILayout.BeginHorizontal ();
			GUILayout.Label (text, HighLogic.Skin.label);
			GUILayout.FlexibleSpace ();
			GUILayout.EndHorizontal ();
		}

		public void GUI_SaveData ()
		{
			CI.configuration.useBlizzyToolbar = newUseBlizzyToolbar;
			CI.configuration.showDrives = newShowDrives;
			Log.Info ("CI.configuration.lastImportDir: " + CI.configuration.lastImportDir);
			//CI.configuration.lastImportDir = m_textPath;
		}

		public void set_CI_Button_active ()
		{
			
			if (!CI.configuration.useBlizzyToolbar) {
				CI_Button.SetTexture (CI_button_img);

			}
		}

		public void GUIToggle ()
		{
			Log.Info ("GUIToggle");
			downloadState = downloadStateType.GUI;
			CIInfoDisplay.infoDisplayActive = !CIInfoDisplay.infoDisplayActive;
			if (CIInfoDisplay.infoDisplayActive) {
				SetVisible (true);
				craftURL = "";
				newCraftName = "";
				saveInSandbox = false;
				overwriteExisting = false;
				CI_Button.SetTexture (CI_button_img); 
			} else {
				SetVisible (false);
				set_CI_Button_active ();
				cfgWinData = false;

				GUI_SaveData ();

				CI.configuration.Save ();
				if (CI.configuration.BlizzyToolbarIsAvailable && CI.configuration.useBlizzyToolbar) {
					HideToolbarStock ();
				} else {
					UpdateToolbarStock ();
					set_CI_Button_active ();

				}

			}
		}
	}
}