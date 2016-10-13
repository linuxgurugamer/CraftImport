using System;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using System.Text;
using System.Net;
using System.Linq;
using SimpleJSON;
using KSP.UI.Screens;

//using KSPPluginFramework;

using System.Security;

namespace CraftImport
{
	//	[WindowInitials(Caption="Craft Import",
	//		Visible=false,
	//		DragEnabled=true,
	//		TooltipsEnabled=true,
	//		WindowMoveEventsEnabled=true)]
	public class MainMenuGui : MonoBehaviour
//	public class MainMenuGui : MonoBehaviourWindowPlus
	{
		public CI thisCI = null;
		//		internal override void DrawWindow(int id)
		//		{
		//		}
		private const int WIDTH = 500;
		private const int HEIGHT = 400;
		private const int BR_WIDTH = 600;
		private const int BR_HEIGHT = 500;

		private Rect bounds = new Rect (Screen.width / 2 - WIDTH / 2, Screen.height / 2 - HEIGHT / 2, WIDTH, HEIGHT);
		private /* volatile*/ bool visible = false;
		bool resetWinPos = false;
		// Stock APP Toolbar - Stavell
		public /*static*/ ApplicationLauncherButton CI_Button = null;
		public  bool stockToolBarcreated = false;

		public static Texture2D CI_button_img = new Texture2D (38, 38, TextureFormat.ARGB32, false);

		private bool CI_Texture_Load = false;

		private bool cfgWinData = false;
		//		private static bool newScreenshotAtIntervals = true;
		private static bool blizzyToolbarInstalled = false;
		private static bool newUseBlizzyToolbar;
		//private static bool newShowDrives;
		private static bool saveInSandbox = false;
		private static bool saveInShipDefault = true;
		private static bool saveInVAB = false;
		private static bool saveInSPH = false;
		private static bool overwriteExisting = false;
		private static bool loadAfterImport = true;
		private static bool appLaucherHidden = true;
		private static string craftURL = "";
		private static string newCraftName = "";
		private static bool subassembly = false;
		bool jpgToDelete = false;
		string convertedJpg = "";
		//private static string newCkanExecPath = "";

		string instructions = "";

		#if EXPORT



		string uploadServer ()
		{
			#if false
			return "http://kerbalx-stage.herokuapp.com";
			#else
			// The following is so that the kebalx-stage server can be accessed without changing any code
			// simply by adding a file 
			if (!System.IO.File.Exists (FileOperations.ROOT_PATH + "GameData/" + CI.MOD_DIR + "devmode.txt")) {
				return "https://kerbalx.com";
			} else {
				Log.Info ("devmode.txt exists");
				return  "http://kerbalx-stage.herokuapp.com";
			}
			#endif
		}

		#endif

		enum downloadStateType
		{
			#if EXPORT
			UPLOAD,
			UPLOAD_FILESELECTION,
			UPLOAD_IN_PROGRESS,
			UPLOAD_COMPLETED,
			UPLOAD_ERROR,
			ACTIONGROUPS,
			SHOW_WARNING,
			GET_DUP_CRAFT_LIST,
			UPDATE_CRAFTLIST_IN_PROGRESS,
			SELECT_DUP_CRAFT,
			GET_THUMBNAIL,
			#endif
			INACTIVE,
			GUI,
			FILESELECTION,
			IN_PROGRESS,
			COMPLETED,
			FILE_EXISTS,
			ERROR}

		;

		private static downloadStateType downloadState = downloadStateType.INACTIVE;
		private static string downloadErrorMessage;

		#if EXPORT
		private static string uploadErrorMessage;

		bool firstAwake = false;

		public void Awake ()
		{
			if (!firstAwake) {
				
				//RenderingManager.AddToPostDrawQueue (0, new Callback (DrawCall));
				firstAwake = true;
			}

			//register for the event
			styleItems.SelectionChanged += styleItemSelectionChanged;
		
		}


		public void OnDestroy ()
		{
			Log.Info("OnDestroy in CI_gui, RemoveModApplication & RemoveFromPostDrawQueue");
			ApplicationLauncher.Instance.RemoveModApplication (this.CI_Button);
			//RenderingManager.RemoveFromPostDrawQueue (0, new Callback (DrawCall));
		}

		//Rect windowRect = new Rect(300, 0, 200, 200);
		Boolean StylesSet = false;

		void DrawCall ()
		{
			//Log.Info ("DrawCall");
			if (!StylesSet) {
				Log.Info ("StylesSet = false");
				StylesSet = true;

				styleItems.styleListItem = new GUIStyle () { };
				styleItems.styleListItem.normal.textColor = Color.white;
				Texture2D texInit = new Texture2D (1, 1);
				texInit.SetPixel (0, 0, Color.white);
				texInit.Apply ();
				styleItems.styleListItem.hover.background = texInit;
				styleItems.styleListItem.onHover.background = texInit;
				styleItems.styleListItem.hover.textColor = Color.black;
				styleItems.styleListItem.onHover.textColor = Color.black;
				styleItems.styleListItem.padding = new RectOffset (4, 4, 4, 4);
				styleItems.styleListItem.fixedWidth = 150F;


				styleItems.styleListSelectedItem = new GUIStyle (GUI.skin.box);
				styleItems.styleListSelectedItem.fixedWidth = 150F;
				styleItems.styleListSelectedItem.alignment = TextAnchor.MiddleLeft; 
				
				styleItems.styleListBlocker = new GUIStyle ();

				Texture2D texInitListBox = new Texture2D (1, 1);
				texInitListBox.SetPixel (0, 0, Color.gray);
				texInitListBox.Apply ();

				styleItems.styleListBox = new GUIStyle (GUI.skin.box);
				styleItems.styleListBox.fixedWidth = 150F;
				//styleItems.styleListBox.normal.background = texInitListBox;


			}

			GUI.skin = HighLogic.Skin;
			//windowRect = GUILayout.Window(50, windowRect, DrawWindow, "Caption");
		}

		DropDownList styleItems = new DropDownList (new List<String> () {
			"ship",
			"aircraft",
			"spaceplane",
			"lander",
			"satellite",
			"station",
			"base",
			"probe",
			"rover",
			"lifter"
		});

		string[] fixedActions = { "abort", "gears", "brakes", "lights", "RCS", "SAS" };

		public void styleItemSelectionChanged (Int32 OldIndex, Int32 NewIndex)
		{
			Log.Info (String.Format ("ListChanged {0}->{1}", styleItems.Items [OldIndex], styleItems.Items [NewIndex]));
			style = styleItems.Items [NewIndex];
		}

		public static bool UrlExists (string file, string urlType)
		{
			string[] image_types = null;
			if (urlType == "image")
				image_types = new string[] { "jpg", "jpeg", "pjped", "png", "bml" };

			//if (urlType == "video")
			//	image_types = {};
			bool exists = false;
			bool b = false;

			// Assume it is an imgur album if lenght == 5
			if (file.Length == 5)
				return true;

			// First make sure it starts correctly
			if (!file.StartsWith ("http://") && !file.StartsWith ("https://") && !file.StartsWith ("ftp://")) {
				return false;
			}
					
				
			if (urlType == "video") {
				// Youtube will only be either http or https
				if (file.StartsWith ("http://") || file.StartsWith ("https://")) {
					// http://stackoverflow.com/questions/3652046/c-sharp-regex-to-get-video-id-from-youtube-and-vimeo-by-url
					Regex YoutubeVideoRegex = new Regex (@"youtu(?:\.be|be\.com)/(?:(.*)v(/|=)|(.*/)?)([a-zA-Z0-9-_]+)", RegexOptions.IgnoreCase);

					Match youtubeMatch = YoutubeVideoRegex.Match (file);
					return  youtubeMatch.Success;
				} else
					return false;
			}
			HttpWebResponse response = null;
			Log.Info ("UrlExists Downloading: " + file);
			var request = (HttpWebRequest)WebRequest.Create (file);
			request.Method = "HEAD";
			request.Timeout = 5000; // milliseconds
			request.AllowAutoRedirect = true;
			Log.Info ("before try");
			try {
				response = (HttpWebResponse)request.GetResponse ();
				exists = response.StatusCode == HttpStatusCode.OK;
				Log.Info ("exists: " + exists.ToString ());
				if (exists) {
					if (urlType == "image") {
						if (file.Contains ("staticflickr.com"))
							b = image_types.Any (s => file.Contains (s));
						else {
							if (file.StartsWith ("http://imgur.com/")) {
								b = true;
								exists = response.ContentType.Contains ("text/html");
							} else {
								b = image_types.Any (s => response.ResponseUri.ToString ().Contains (s));
								exists = response.ContentType.Contains ("image");
							}
						}
						exists = exists & b;
					} else { 
						
#if false
						// urlType == "video"
						Log.Info("video");
						Log.Info("ContentType: " + response.ContentType);
						Log.Info("ResponseUrl: " + response.ResponseUri);
						exists = response.ResponseUri.ToString().Contains("youtube.com");
						#endif
						exists = false;
					}
				}
			} catch {
				Log.Info ("response.StatusCode: " + response.StatusCode.ToString ());
				Log.Info ("    status descr: " + response.StatusDescription);
				exists = false;
			} finally {
				// close your response.
				if (response != null)
					response.Close ();
			}

			return exists;
		}


		#endif


		internal MainMenuGui ()
		{
			
			blizzyToolbarInstalled = ToolbarManager.ToolbarAvailable;
		}

//		public void setAppLauncherHidden ()
//		{
//			appLaucherHidden = true;
//		}

		public void OnGUIHideApplicationLauncher ()
		{
//			Log.Info ("OnGUIHideApplicationLauncher: BlizzyToolbarIsAvailable: " + thisCI.configuration.BlizzyToolbarIsAvailable.ToString () +
//				"   useBlizzyToolbar: " + thisCI.configuration.useBlizzyToolbar.ToString ());
			if (!appLaucherHidden) {
				
				if (thisCI.configuration.BlizzyToolbarIsAvailable && thisCI.configuration.useBlizzyToolbar) {
					HideToolbarStock ();
					appLaucherHidden = true;
				}

			}
		}

		public void OnGUIShowApplicationLauncher ()
		{
//			Log.Info ("OnGUIShowApplicationLauncher: BlizzyToolbarIsAvailable: " + thisCI.configuration.BlizzyToolbarIsAvailable.ToString () +
//			"   useBlizzyToolbar: " + thisCI.configuration.useBlizzyToolbar.ToString ());
			if (!thisCI.configuration.BlizzyToolbarIsAvailable || !thisCI.configuration.useBlizzyToolbar) {
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

		public void UpdateToolbarStock ()
		{
			Log.Info ("UpdateToolbarStock, appLaucherHidden: " + appLaucherHidden.ToString ());

			// Create the button in the KSP AppLauncher
			if (!CI_Texture_Load) {
				if (GameDatabase.Instance.ExistsTexture (CI.TEXTURE_DIR + "CI-38"))
					CI_button_img = GameDatabase.Instance.GetTexture (CI.TEXTURE_DIR + "CI-38", false);
				//if (GameDatabase.Instance.ExistsTexture (CI.TEXTURE_DIR + "CI-folder"))
				//	CI_button_off = GameDatabase.Instance.GetTexture (CI.TEXTURE_DIR + "CI-folder", false);
				

				CI_Texture_Load = true;
			}
			if (CI_Button == null /*&& !appLaucherHidden */) {
				Log.Info ("UpdateToolbarStock, CI_Button is null");
				CI_Button = ApplicationLauncher.Instance.AddModApplication (GUIToggle, GUIToggle,
					null, null,
					null, null,
					ApplicationLauncher.AppScenes.SPH | ApplicationLauncher.AppScenes.VAB,
					CI_button_img);
				stockToolBarcreated = true;
			}
		}

		private void HideToolbarStock ()
		{
			Log.Info ("HideToolbarStock");
			ApplicationLauncher.Instance.RemoveModApplication (CI_Button);
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

		void resetBeforeExit ()
		{
			Log.Info ("resetBeforeExit");
			if (download != null)
				download.Dispose ();
			if (jpgToDelete && convertedJpg != "") {
				System.IO.FileInfo file = new System.IO.FileInfo (convertedJpg);
				file.Delete ();
			}
		#if EXPORT
			_image = null;
		thumbnailInited = false;
		changed = true;
		checkedForThumbnail = false;

		#endif
			download = null;
			SetVisible (false);
			GUI.enabled = false;
			downloadState = downloadStateType.INACTIVE;
			RemoveInputLock ();

			cfgWinData = false;
			m_fileBrowser = null;
			m_textPath = "";
			craftURL = "";
			newCraftName = "";
			saveInSandbox = false;
			saveInVAB = false;
			saveInSPH = false;
			saveInShipDefault = true;
			overwriteExisting = false;
			loadAfterImport = true;
			subassembly = false;
			CIInfoDisplay.infoDisplayActive = false;
			if (thisCI.configuration.BlizzyToolbarIsAvailable && thisCI.configuration.useBlizzyToolbar) {
				HideToolbarStock ();
			} else {
				UpdateToolbarStock ();
				set_CI_Button_active ();

			}
//			GUIToggle ();

		}

		string saveFile;

		bool saveCraftFile (string craftFile, bool subassembly = false)
		{
			//Log.Info ("saveCraftFile: " + craftFile);
			string saveDir = "";
			if (saveInSandbox ) {
				saveDir = FileOperations.ROOT_PATH + "Ships";
			} else {
				saveDir = FileOperations.ROOT_PATH + "saves/" + HighLogic.SaveFolder + "/Ships";
			}
			//string saveDir = KSPUtil.ApplicationRootPath + sandbox + HighLogic.SaveFolder + "/ships";
			saveFile = "";

			string shipName = null;
			Match t = null;
			try {

				var s = Regex.Match (craftFile, "^ship.*=.*");
				s = Regex.Match (s.ToString (), "=.*");

				shipName = s.ToString ().Remove (0, 1).Trim ();


				t = Regex.Match (craftFile, "type.*=.*");
				Log.Info ("after regex type   t: " + t.ToString ());
				string strCraftFile = craftFile.ToString ();
				if (newCraftName != "") {
					strCraftFile = strCraftFile.Replace (shipName, newCraftName);
					shipName = newCraftName;
				}
				if (!subassembly) {
					//Log.Info ("regex: " + t.ToString ());
					bool vab = t.ToString ().Contains ("VAB");
					bool sph = t.ToString ().Contains ("SPH");
					if (!saveInShipDefault) {
						if (saveInVAB) {
							vab = true;
							sph = false;
						}
						if (saveInSPH) {
							vab = false;
							sph = true;
						}
					}


					if (vab) {
						saveFile = saveDir + "/VAB/" + shipName + ".craft";
					}
					if (sph) {
						saveFile = saveDir + "/SPH/" + shipName + ".craft";
					}
				} else {
					saveFile = FileOperations.ROOT_PATH + "saves/" + HighLogic.SaveFolder + "/Subassemblies/" + shipName + ".craft";
					Log.Info ("subassembly saveDir: " + saveFile);
				}

				if (System.IO.File.Exists (saveFile) && !overwriteExisting) {
					downloadState = downloadStateType.FILE_EXISTS;
					return false;
				}

				System.IO.File.WriteAllText (saveFile, strCraftFile);

				downloadState = downloadStateType.COMPLETED;
			} catch (Exception e) { 
				Log.Info ("Error: " + e);

				//downloadErrorMessage = download.error;
				downloadErrorMessage = "Download URL did not specify a valid .craft file";
				downloadState = downloadStateType.ERROR;
				return false;
			}
			return true;
		}

		private void LoadCraftAsSubassembly(string craftFile)
		{
			if (!System.IO.File.Exists(craftFile))
				return;
			ShipConstruct merge = ShipConstruction.LoadSubassembly(craftFile);
			if (merge != null)
			{
				EditorLogic.fetch.SpawnConstruct(merge);
			}
		}

		bool missingParts = false;
		bool checkMissingParts (string craft)
		{
			Log.Info ("checkMissingParts");
			SortedList<string, string> missingPartsList = new SortedList<string, string> ();

			Log.Info ("checkMissingParts");
			missingParts = false;
			string partRegex = ".*part.*=.*_[0-9]*";
			Regex detectPartLine = new Regex (partRegex);
			var matches = detectPartLine.Match (craft);
			int underscoreIndex, equalsIndex;
			string partName;
			while (matches.Success) {
				equalsIndex = matches.Value.IndexOf ("=") + 1;
				underscoreIndex = matches.Value.LastIndexOf ("_");
				partName = matches.Value.Substring (equalsIndex, underscoreIndex - equalsIndex);
				partName = partName.Trim ();

				if (PartLoader.getPartInfoByName (partName) == null) {
					missingParts = true;
					if (!missingPartsList.ContainsValue (partName))
						missingPartsList.Add (partName, partName);
				}

				matches = matches.NextMatch ();
			}
			if (missingParts) {
				loadAfterImport = false;
				instructions += "This craft file requires the following parts which are not available in the current install:\r\n\r\n";
				foreach (KeyValuePair<string, string> pair in missingPartsList)
					instructions += "      " + pair.Value + "\r\n";
				Log.Info ("instructions: " + instructions);
				return true;
			}
			return false;
		}

		WWW download = null;
		WWW craftDownload = null;
		bool kerbalx = false;

		System.Collections.IEnumerator doDownloadCraft (string craftURL)
		{
			if (craftURL != "") {
				string s = System.Uri.EscapeUriString (craftURL);

				// some simple error checking
				if (!s.StartsWith ("http://") && !s.StartsWith ("https://") && !s.StartsWith ("ftp://") && !s.StartsWith ("file://")) {
					downloadErrorMessage = "Invalid URL or file specified";
					downloadState = downloadStateType.ERROR;
					yield break;
				}
				kerbalx = (craftURL.IndexOf ("kerbalx.com", StringComparison.OrdinalIgnoreCase) >= 0);
				if (kerbalx)
				{
					downloadState = downloadStateType.COMPLETED;
					instructions = "Access to KerbalX is currently disabled due to a site problem.\n\n";
					instructions += "You can download the craft file using a browser, and then import using this mod.\n\n";
					instructions += "The mod will be updated again once the security issues with the site have been fixed";
					yield break;
						
				}
				// Create a download object
				Log.Info ("s: " + s);
				Log.Info ("kerbalx: " + kerbalx.ToString ());
				craftDownload = new WWW (s);
				// Wait until the download is done
				yield   return craftDownload;
				Log.Info ("Download completed   craftDownload.error: " +craftDownload.error);
				if (!String.IsNullOrEmpty (craftDownload.error)) {
					Log.Error ("Error downloading: " + craftDownload.error);
					downloadErrorMessage = craftDownload.error;
					downloadState = downloadStateType.ERROR;
					yield break;
				} else {
					JSONNode j = null;
					subassembly = false;
					if (kerbalx) {
						bool https = (craftURL.IndexOf ("https://kerbalx.com", StringComparison.OrdinalIgnoreCase) == 0);
						int dot = craftURL.IndexOf (".craft");
						if (dot > 0) {
							craftURL = craftURL.Substring (0, dot);
						}
						Log.Info ("craftURL on kerbalx: " + craftURL);
						s = System.Uri.EscapeUriString (craftURL + ".json");
						var json = new WWW (s);
						yield   return json;
						j = JSON.Parse (json.text);
						if (j ["subassembly"].AsBool == true)
							subassembly = true;
					}
					bool b = saveCraftFile (craftDownload.text, subassembly);
		
					Log.Info ("After saveCraftFile, b: " + b.ToString());
					saveInSandbox = false;
					overwriteExisting = false;

					if (b) {


						//downloadState = downloadStateType.COMPLETED;
						if (kerbalx) {
							#if false
							bool https = (craftURL.IndexOf ("https://kerbalx.com", StringComparison.OrdinalIgnoreCase) == 0);
							int dot = craftURL.IndexOf (".craft");
							if (dot > 0) {
								craftURL = craftURL.Substring (0, dot);
							}
							Log.Info ("craftURL on kerbalx: " + craftURL);
							s = System.Uri.EscapeUriString(craftURL + ".json");
							download.Dispose ();
							Log.Info ("json: " + s);
							download = new WWW (s);
							// Wait until the download is done
							yield   return download;

							j = JSON.Parse (download.text);
							#endif
							int i = j ["mods"].Count;
							string m;
							//for (int i1 = 0; i1 < i; i1++) {
							//	m = j ["mods"] [i1];
							//	Log.Info ("Mod: " + m);
							//}

							s = System.Uri.EscapeUriString (craftURL + ".ckan");
							Log.Info ("ckan: " + s);
							//download.Dispose ();
							download = new WWW (s);
							// Wait until the download is done
							yield   return download;

							// 1. Need to get current ksp directory for ckan, root path ends in a slash
							//string root = KSPUtil.ApplicationRootPath.Substring (0, KSPUtil.ApplicationRootPath.Length - 12);
							Log.Info ("Root path: " + FileOperations.ROOT_PATH);
							string ckanDir = FileOperations.ROOT_PATH + "CKAN";
							Log.Info ("ckanDir: " + ckanDir);
							if (System.IO.Directory.Exists (ckanDir)) {
								string ckanFile = j ["name"] + ".ckan";
								Log.Info ("ckanFile: " + ckanFile);
								string ckanFilePath = ckanDir + "/" + ckanFile;
								System.IO.File.WriteAllText (ckanFilePath, download.text);

								Log.Info ("installedCkan: " + ckanDir + "/installed-default.ckan");
								string ckanInstallDefault = System.IO.File.ReadAllText (ckanDir + "/installed-default.ckan");
								var installedCkanMods = JSON.Parse (ckanInstallDefault);

								bool missing, anymissing = false;
								instructions = "";
								if (j ["mods"].Count > 0) {
									
									// The following is very inefficient
									for (int i1 = 0; i1 < j ["mods"].Count; i1++) {
										missing = true;
										for (int ckan = 0; ckan < installedCkanMods ["depends"].Count; ckan++) {
											if (j ["mods"] [i1].ToString() == installedCkanMods ["depends"] [ckan] ["name"].ToString()) {
												missing = false;
												break;
											}
										}
										if (missing) {
											if (!anymissing)
												instructions += "You are missing the following mods:\r\n\r\n";
											anymissing = true;
											loadAfterImport = false;
											instructions += "      " + j ["mods"] [i1] + "\r\n";
										
										}
									}
								}

								if (anymissing) {
									instructions += "\r\nYou will need to load the missing mods before you can load the imported craft:\r\n" + j ["name"] + ".";
									instructions += "  A .ckan file has been saved in the CKAN directory for the current game:\r\n\r\n" +
										ckanFilePath + "\n\nYou can use CKAN to load all missing mods by following these instructions:\r\n\r\n" +
										"1.  Start CKAN\r\n2.  Select File->Import from .ckan\r\n3.  Navigate to the folder:      " + ckanDir +
										"\r\n4.  Select the file:      " + ckanFile + "\r\n";
								} else {
									checkMissingParts (craftDownload.text);
								}
							} else {
								// CKAN not installed
								if (j ["mods"].Count > 0) {
									instructions = "If you can't load the imported craft: " + j ["name"] + ", then you will have to load the necessary mods by hand." +
									"\r\n\r\nYou don't seem to have CKAN installed in this game, so you will have to install them manually.\r\n\r\n" +
									"Here is the list of mods that this craft file needs:\r\n\r\n";
									for (int i1 = 0; i1 < j ["mods"].Count; i1++) {
										m = j ["mods"] [i1];
										instructions += "     " + m + "\r\n";
									}
									instructions += "\r\n";
								} 
								checkMissingParts (craftDownload.text);
									// instructions = "You should be able to load the craft in either the VAB or SPH";

							}


							if (loadAfterImport) {
								if ( (EditorLogic.RootPart && subassembly) || !subassembly)
								instructions += "\r\nThe imported craft file will be loaded when you click the OK button";
							}
						} else {
							checkMissingParts (craftDownload.text);
						}
					}
					craftURL = "";
				}
				if (download != null) {
					download.Dispose ();
					download = null;
				}
			}
		}

		void downloadCraft (string s)
		{
			if (!s.StartsWith ("http://") && !s.StartsWith ("https://") && !s.StartsWith ("ftp://") && !s.StartsWith ("file://")) {
				if (!System.IO.File.Exists (s))
					s = "http://" + s;
			}
			downloadState = downloadStateType.IN_PROGRESS;
			StartCoroutine (doDownloadCraft (s));
		}

		#if EXPORT
		
		string uid = "";
		string pswd = "";

		string tags = "";
		string style = "";
		string videoUrl = "";
		string pictureUrl = "";
		bool forceNew = false;
		bool saveUid = false;
		bool savePswd = false;
		int uploadProgress;
		float lastUpdate;
		string[] actionGroups = new string[16];
		Vector2 buildMenuScrollPosition = new Vector2 ();

		List<string> ids = new List<string> ();
		List<string> craftNames = new List<string> ();
		//List<string> craftUrls = new List<string> ();
		List<bool> selectedCraft = new List<bool> ();
		int lastSelectedCraft = -1;

		void saveUidPswd ()
		{
			if (saveUid)
				thisCI.configuration.uid = uid;
			else
				thisCI.configuration.uid = "";
			if (savePswd)
				thisCI.configuration.pswd = pswd;
			else
				thisCI.configuration.pswd = "";
			thisCI.configuration.Save ();
		}

		void getUidPswd ()
		{
			uid = thisCI.configuration.uid;
			if (uid != "" && uid != null)
				saveUid = true;
			else
				saveUid = false;
			pswd = thisCI.configuration.pswd;
			if (pswd != "" && pswd != null)
				savePswd = true;
			else
				savePswd = false;
		}

		public void ConvertToJPG (string originalFile, string newFile, Color background, int quality = 75)
		{
			
			Texture2D png = new Texture2D (1, 1);

			byte[] pngData = System.IO.File.ReadAllBytes (originalFile);
			png.LoadImage (pngData);


			Color[] srcColors = png.GetPixels ();
			Color[] dstColors = new Color[srcColors.Length];
			for (int i = 0; i < srcColors.Length; i++) {
				Color srcColor = srcColors [i];
				float a = srcColor.a;
				if (a == 0.0F) {
					srcColor = background;
					srcColor.a = 1.0F;
				}

				dstColors [i] = new Color (srcColor.r, srcColor.g, srcColor.b, 1.0F);
			}

			Texture2D result = new Texture2D (png.width, png.height, TextureFormat.ARGB32, false);
			result.SetPixels (dstColors);
			result.Apply ();
			png = result;


			byte[] jpgData = png.EncodeToJPG (quality);
			var file = System.IO.File.Open (newFile, System.IO.FileMode.Create);
			var binary = new System.IO.BinaryWriter (file);
			binary.Write (jpgData);
			file.Close ();
			Destroy (png);
			//Resources.UnloadAsset(png);
		}

		void openUrl (string url)
		{
			if (url.StartsWith ("https")) {
				url = "http" + url.Substring (5);
				Log.Info ("Edited url: " + url);
			}
			Application.OpenURL (url);
		}

		System.Collections.IEnumerator doUploadCraft (string craftURL)
		{
			if (craftURL != "") {

				//if (craftURL.StartsWith ("file://"))
				if (System.IO.File.Exists (craftURL)) {
					//craftURL = craftURL.Substring (7);

					if (checkMissingParts(System.IO.File.ReadAllText (craftURL))) {
						uploadErrorMessage = "Craft file must be loadable.";
						uploadErrorMessage += instructions;
						instructions = "";
						downloadState = downloadStateType.UPLOAD_ERROR;
						yield break;
					}
					EditorLogic.LoadShipFromFile (craftURL);
					ShipConstruct ship = ShipConstruction.LoadShip (craftURL);

					Log.Info ("Craft name: " + ship.shipName);

					string thumbnailPath;
					byte[] image = null;
					bool thumbnail = false;
					//	bool deleteJpg = false;
					if (pictureUrl == "") {
						Log.Info ("Capturing thumbnail");
						ThumbnailHelper.CaptureThumbnail (ship, 1024, "Screenshots", ship.shipName);
						thumbnail = true;
						jpgToDelete = true;
						pictureUrl = "file://" + FileOperations.ROOT_PATH + "Screenshots/" + ship.shipName + ".png";
					} else {
						Log.Info ("pictureUrl nonblank");
						thumbnailPath = pictureUrl;
						if (lastSelectedCraft < 0) {
							//if (pictureUrl == "") {
							//	uploadErrorMessage = "Picture URL cannot be empty";
							//	downloadState = downloadStateType.UPLOAD_ERROR;
							//	yield break;
							//
							//}


							if (!pictureUrl.StartsWith ("file://") && !UrlExists (pictureUrl, "image")) {
								uploadErrorMessage = "Picture URL is not valid";
								downloadState = downloadStateType.UPLOAD_ERROR;
								yield break;
							} else
								pictureUrl = thumbnailPath;
	
							if (pictureUrl.StartsWith ("http://imgur.com/") || pictureUrl.Length == 5) {
								string[] p = pictureUrl.Split ('/');
								pictureUrl = "[imgur]" + p.Last () + "[/imgur]";
								Log.Info ("imgur: " + pictureUrl);
								//	file = "[imgur]" + file + "[/imgur]";
							}

						}

					}
					Log.Info ("pictureUrl: " + pictureUrl);

					if (videoUrl != "" && !UrlExists (videoUrl, "video")) {
						uploadErrorMessage = "Video URL is not a valid YouTube URL";
						downloadState = downloadStateType.UPLOAD_ERROR;
						yield break;
					}

					Log.Info ("craftURL: " + craftURL);
					string craft = System.IO.File.ReadAllText (craftURL);

					JSONNode jsonuid = uid;
					JSONNode jsonpswd = pswd;
					JSONNode jsoncraft = craft;

					string url = uploadServer () + "/crafts.json";

					JSONNode n = JSON.Parse ("{}");
					n ["username"] = uid;
					n ["password"] = pswd;
					n ["craft_file"] = craft;
					if (tags != "")
						n ["tags"] = tags;
					if (style != "")
						n ["style"] = style;
					Log.Info ("pictureUrl: " + pictureUrl);
					if (pictureUrl != "") {
						if (pictureUrl.StartsWith ("file://")) {
							// If a png file, convert to jpg before uploading
							if (pictureUrl.EndsWith (".png", StringComparison.OrdinalIgnoreCase)) {
								string pngToConvert = pictureUrl.Substring (7);
								convertedJpg = pictureUrl.Substring (7, pictureUrl.Length - 10) + "jpg";
								Log.Info ("pngToconvert: " + pngToConvert);
								Log.Info ("convertedJpg: " + convertedJpg);

								ConvertToJPG (pngToConvert, convertedJpg, thisCI.configuration.backgroundcolor);
								// Delete the file if we took a snapshot of the craft using the thumbnail function
								if (thumbnail) {
									Log.Info ("deleting: " + pngToConvert);
									System.IO.FileInfo file = new System.IO.FileInfo (pngToConvert);
									file.Delete ();
								} 
								//	deleteJpg = true;
								jpgToDelete = true;
								pictureUrl = "file://" + convertedJpg;
							}
							Log.Info ("pictureUrl 2: " + pictureUrl.Substring (7));
							if (System.IO.File.Exists (pictureUrl.Substring (7))) {
								Log.Info ("pictureUrl exists");
								image = System.IO.File.ReadAllBytes (pictureUrl.Substring (7));
								if (image != null) {
									n ["image"] = System.Convert.ToBase64String (image);
								}
								// If this wasn't a thumbnail, and it was converted, delete the jpg
								// Delete it if it was a thumbnail as well
								//		if (deleteJpg) {
								//			System.IO.FileInfo file = new System.IO.FileInfo (pictureUrl.Substring (7));
								//			file.Delete ();
								//		}
							}
						} else {
							n ["picture_url"] = pictureUrl;					
						}
					}

					if (videoUrl != "")
						n ["video_url"] = videoUrl;
					if (forceNew)
						n ["force_new"] = "true";
					else {
						n ["force_new"] = "false";
						if (lastSelectedCraft >= 0)
							n ["update_existing"] = ids [lastSelectedCraft];
					}

					//if (n ["action_groups"].AsInt == 0) {
					JSONNode a = JSON.Parse ("{}");
					string hash;
					bool actionGroupSpecified = false;
					for (int i = 0; i < 16; i++) {
						Log.Info ("i: " + i.ToString () + "    [" + actionGroups [i] + "]");
						actionGroups [i] = actionGroups [i].Trim ();
						if (actionGroups [i] != "")
							actionGroupSpecified = true;
						//if (actionGroups [i] != "") 
						{
							if (i < 10) {
								hash = (i + 1).ToString ();
							} else {
								hash = fixedActions [i - 10];
							}
							a [hash] = actionGroups [i];
							Log.Info ("Adding field: hash: " + hash + "      value: " + actionGroups [i]);
						}
					}
					//}
					if (actionGroupSpecified)
						n ["action_groups"] = a.ToJSON (0);
					string ns = n.ToJSON (0);
					//Log.Info ("json:" + ns);

					Dictionary<string, string> headers = new Dictionary<string, string> ();
					headers.Add ("Content-Type", "application/json");
					var upload = new WWW (url, Encoding.ASCII.GetBytes (ns), headers);

					// Wait until the upload is done
					yield   return upload;

					instructions = "";
				
					if (!String.IsNullOrEmpty (upload.error)) {
						Log.Error ("Error uploading: " + upload.error);
						uploadErrorMessage = upload.error;
						downloadState = downloadStateType.UPLOAD_ERROR;
						yield break;
					} else {

						Log.Info ("upload.responseHeaders.Count: " + upload.responseHeaders.Count);
						if (upload.responseHeaders.Count > 0) {
							foreach (var entry in upload.responseHeaders) {
								Log.Info (entry.Key + "=" + entry.Value);
							}
						}
							
						downloadState = downloadStateType.UPLOAD_COMPLETED;
						craftURL = "";
						//instructions = upload.text + "\n" + thumbnailPath;

						//if (upload.responseHeaders.Count > 0) {
						//	for (string entry in upload.responseHeaders) {
						//		Log.Info(entry.Value + "=" + entry.Key);
						Log.Info (upload.text);
						var j = JSON.Parse (upload.text);
						if (j != null) {
							if (j ["status"].AsInt == 200) {
								instructions = "The craft file has been uploaded.\r\n\r\nA browser window is being opened where you can edit the uploaded craft";
								if (j ["image_failed_to_upload "].AsBool == true) {
									instructions += "\r\n\r\nPlease note that the image failed to upload properly due to an Imgur error,\r\n";
									instructions += "so you will need to upload an image to Imgur yourself";
								}
								string editUrl = uploadServer () + j ["edit_url"];
								openUrl (editUrl);
							} else if (j ["status"].AsInt == 409) {
								downloadState = downloadStateType.SELECT_DUP_CRAFT;
								if (j ["existing_craft"].Count == 1)
									instructions = "There is already an existing craft by that name\r\n";
								else
									instructions = "There are " + j ["existing_craft"].Count.ToString () + " existing crafts by that name\r\n";

								ids.Clear ();
								craftNames.Clear ();
								//craftUrls.Clear ();
								selectedCraft.Clear ();
								lastSelectedCraft = -1;
								for (int i = 0; i < j ["existing_craft"].Count; i++) {
									ids.Add (j ["existing_craft"] [i] ["id"]);
									craftNames.Add (j ["existing_craft"] [i] ["name"]);
									//craftUrls.Add (j ["existing_craft"] [i] ["url"]);
									selectedCraft.Add (false);
									instructions += "\r\n" + ids [i] + ": " + craftNames [i];
								}
							} else if (j ["status"].AsInt == 401) {
								instructions = "There is a userid and/or password error\r\n";
								instructions += "Please correct your userid and/or password and try again";
								downloadState = downloadStateType.UPLOAD_ERROR;

							} else if (j ["status"].AsInt == 500) {
								instructions = "An unknown error occurred:\r\n\r\n";
								instructions += j ["error"];
								instructions += "\r\n\r\nPlease contact KerbalX support for assistance";
								downloadState = downloadStateType.UPLOAD_ERROR;
							} else {
								instructions = "Unknown Error:" + j ["status"].AsInt;
								downloadState = downloadStateType.UPLOAD_ERROR;
							}
						} else {
							instructions = "An unknown server error occurred: ";
							downloadState = downloadStateType.UPLOAD_ERROR;
						}
					}
					upload.Dispose ();
					upload = null;
				} else {
					instructions = "Error";
					downloadState = downloadStateType.UPLOAD_ERROR;
				}
			}
		}

		void uploadCraft (string craftURL)
		{
			saveUidPswd ();
			uploadProgress = 0;
			lastUpdate = Time.realtimeSinceStartup;
			downloadState = downloadStateType.UPLOAD_IN_PROGRESS;
			StartCoroutine (doUploadCraft (craftURL));
		}


		System.Collections.IEnumerator doGetUpdateCraftList ()
		{
			if (uid != "") {
				string url = uploadServer () + "/" + uid + ".json";

				var upload = new WWW (url);
				// Wait until the download is done
				yield   return upload;

				var j = JSON.Parse (upload.text);
				instructions = "";

				if (!String.IsNullOrEmpty (upload.error)) {
					Log.Error ("Error uploading: " + upload.error);
					uploadErrorMessage = upload.error;
					downloadState = downloadStateType.UPLOAD_ERROR;
					yield break;
				} else {
					Log.Info ("json: " + upload.text);
					//if (j ["status"].AsInt == 200)
					{

						ids.Clear ();
						craftNames.Clear ();
						//craftUrls.Clear ();
						selectedCraft.Clear ();
						lastSelectedCraft = -1;
						Log.Info ("c count: " + j ["craft"].Count);
						foreach (var key in j["craft"].Keys) {
							Log.Info ("key: " + key);
							Log.Info ("data: " + j ["craft"] [key].ToString ());
							ids.Add (key);
							craftNames.Add (j ["craft"] [key].ToString ());
							selectedCraft.Add (false);
						}
						downloadState = downloadStateType.SELECT_DUP_CRAFT;

					}
					
				}

			} else
				downloadState = downloadStateType.UPLOAD;
		}

		void getUpdateCraftList ()
		{
			downloadState = downloadStateType.UPDATE_CRAFTLIST_IN_PROGRESS;
			StartCoroutine (doGetUpdateCraftList ());
		}
		#endif
		/// <summary>
		/// //////////////////////////////////////////////
		/// </summary>
		protected string m_textPath;

		protected FileBrowser m_fileBrowser = null;
		bool fileBrowserEnabled = false;

		[SerializeField]
		protected Texture2D m_directoryImage,
			m_fileImage;
		private string selectionType;

		void getFile (string title, string suffix, string dir = "")
		{
			fileBrowserEnabled = true;
			selectionType = suffix;

			#if false
			GUILayout.BeginHorizontal ();
			GUILayout.Label ("Select Craft File: ", GUILayout.Width (100));
			//GUILayout.FlexibleSpace ();

			GUILayout.Label (m_textPath ?? "none selected");

			GUILayout.FlexibleSpace ();
			//if (!m_directoryImage) Log.Info ("getFile 1 m_directoryImage is null");
			if (GUILayout.Button ("  ...    ", GUILayout.ExpandWidth (false))) {
				Log.Info ("creating m_fileBrowser");
			#endif
			m_fileBrowser = new FileBrowser (
				new Rect (Screen.width / 2 - BR_WIDTH / 2, Screen.height / 2 - BR_HEIGHT / 2, BR_WIDTH, BR_HEIGHT),
				title,
				FileSelectedCallback
			);

			if (!m_directoryImage) {
				m_directoryImage = new Texture2D (38, 38, TextureFormat.ARGB32, false);
				if (GameDatabase.Instance.ExistsTexture (CI.TEXTURE_DIR + "CI-folder"))
					m_directoryImage = GameDatabase.Instance.GetTexture (CI.TEXTURE_DIR + "CI-folder", false);
			}
			if (!m_fileImage) {
				m_fileImage = new Texture2D (38, 38, TextureFormat.ARGB32, false);
				if (GameDatabase.Instance.ExistsTexture (CI.TEXTURE_DIR + "CI-file"))
					m_fileImage = GameDatabase.Instance.GetTexture (CI.TEXTURE_DIR + "CI-file", false);
			}
			// Linux change may needed here
			m_fileBrowser.SelectionPattern = "*" + suffix;
			m_fileBrowser.DirectoryImage = m_directoryImage;
			m_fileBrowser.FileImage = m_fileImage;
			m_fileBrowser.showDrives = thisCI.configuration.showDrives;
			m_fileBrowser.ShowNonmatchingFiles = false;


			if (dir != "") {
				m_fileBrowser.SetNewDirectory (dir);
			} else {
				if (m_textPath != "")
					m_fileBrowser.SetNewDirectory (m_textPath);
			}

			#if false
			}
			if (m_fileBrowser != null)
				Log.Info("3 m_fileBrowser not null");
			GUILayout.FlexibleSpace ();
			GUILayout.EndHorizontal ();
			GUILayout.BeginHorizontal ();
			GUILayout.Label (" ");
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
			GUI.DragWindow ();
			if (m_fileBrowser != null)
				Log.Info("m_fileBrowser not null");
			#endif
		}

		protected void FileSelectedCallback (string path)
		{
			m_fileBrowser = null;
			fileBrowserEnabled = false;


			m_textPath = path;
			if (m_textPath != "" && m_textPath != null) {
				if (selectionType == ".craft") {
					#if EXPORT
					if (downloadState == downloadStateType.UPLOAD_FILESELECTION)
						downloadState = downloadStateType.UPLOAD;
					else
					#endif
					downloadState = downloadStateType.GUI;
					craftURL = "file://" + m_textPath;
					thisCI.configuration.lastImportDir = System.IO.Path.GetDirectoryName (m_textPath);
					return;
				}
			}
			if (m_textPath != "" && m_textPath != null) {
				if (selectionType == ".png|.jpg") {
					#if EXPORT
					downloadState = downloadStateType.UPLOAD;
					pictureUrl = "file://" + m_textPath;
					#endif
					//CI.configuration.lastImportDir = System.IO.Path.GetDirectoryName (m_textPath);
					return;
				}
			}
			//Log.Info ("downloadState: " + downloadState.ToString ());

			downloadState = downloadStateType.GUI;
		}



		internal void RemoveInputLock ()
		{
			Log.Info ("RemoveInputLock");
			if (InputLockManager.GetControlLock ("CraftImportLock") != ControlTypes.None) {
				//LogFormatted_DebugOnly("Removing-{0}", "TWPControlLock");
				InputLockManager.RemoveControlLock ("CraftImportLock");
			}
			Input.ResetInputAxes ();
			//InputLockExists = false;
		}

		/////////////////////////////////////
		public void OnGUI ()
		{
			GUI.skin = HighLogic.Skin;

			if (Event.current.type == EventType.Repaint || Event.current.isMouse)
			{
				DrawCall(); // Your current on preDrawQueue code
			}
			#if EXPORT
			Awake ();
			#endif

			//	InputLockManager.SetControlLock((ControlTypes.EDITOR_LOCK | ControlTypes.EDITOR_GIZMO_TOOLS), "CraftImportLock");


			if (m_fileBrowser != null) {
				if (!fileBrowserEnabled) {
					
					m_fileBrowser = null;
					downloadState = downloadStateType.GUI;

					//this one closes the dropdown if you click outside the window elsewhere
					//	styleItems.CloseOnOutsideClick();

				} else {

					//	GUI.skin = HighLogic.Skin;
					#if true
					GUIStyle s = new GUIStyle (HighLogic.Skin.textField);

					s.onNormal = HighLogic.Skin.textField.onNormal;

					//			s.fontSize = 15;
					s.name = "listitem";
					s.alignment = TextAnchor.MiddleLeft;
					//s.fontStyle = FontStyle.Bold;
					//s.fixedHeight = 50;
					s.imagePosition = ImagePosition.ImageLeft;
					GUI.skin.customStyles [0] = s;
					#endif
					m_fileBrowser.Window (GetInstanceID ());
					return;
				}
			}


			try {
				if (this.Visible ()) {
//					Rect bounds2 = GUILayout.Window (GetInstanceID (), bounds, Window, CI.TITLE, HighLogic.Skin.window);
					if (resetWinPos) {
						bounds = new Rect (Screen.width - WIDTH, Screen.height / 2 - HEIGHT / 2, WIDTH, HEIGHT);
						resetWinPos = false;
					}
					bounds = GUILayout.Window (GetInstanceID (), bounds, Window, CI.TITLE, HighLogic.Skin.window);
				#if EXPORT
					if (_image != null) {
						imgbounds = new Rect (imgbounds.xMin, imgbounds.yMin, displayres, displayres + 30);
						imgbounds = GUILayout.Window (GetInstanceID () + 1, imgbounds, imgWindow, CI.TITLE + " Upload Image Preview", HighLogic.Skin.window);
					}
					#endif
				}
			} catch (Exception e) {
				Log.Error ("exception: " + e.Message);
			}
		}
		#if EXPORT
		void  uploadCase ()
		{
			if (craftURL.StartsWith ("file://"))
				craftURL = craftURL.Substring (7);
			if (System.IO.File.Exists (craftURL)) {

				// Need to add the following:

				//tags (space separated string), 
				//style (string, one of [ship aircraft spaceplane lander satellite station base probe rover lifter], if none provided will default to 'ship') *1
				//description (string, additional text about the craft, KX will also read the description from the craft file)
				//action_groups (hash of group_name => string-description only the keys [abort gears brakes lights RCS SAS 0, 1..9] will be read)
				//video_url (string with url to youtube vid, I don't have support for other video sites yet)

				//	update_existing (boolean, default false. If true and that user has a craft with the same name, KX will update the existing craft rather than creating a new one.  In this case providing pictures will be optional). *2

				GUILayout.BeginHorizontal ();
				GUILayout.Label ("Craft File:");
				GUILayout.FlexibleSpace ();
				//GUILayout.EndHorizontal ();
				//GUILayout.BeginHorizontal ();
				GUILayout.FlexibleSpace ();
				var styleLabel = new GUIStyle (GUI.skin.label);
				styleLabel.normal.textColor = Color.green;
				GUILayout.Label (craftURL, styleLabel);
				GUILayout.EndHorizontal ();

				GUILayout.BeginHorizontal ();
				GUILayout.Label ("Tags:", GUILayout.Width(50F));
				GUILayout.FlexibleSpace ();
				tags = GUILayout.TextField (tags, GUILayout.MinWidth (50F), GUILayout.MaxWidth (300F));
				GUILayout.EndHorizontal ();


				//this draws the transparent catching button that stops items behind the listbox getting events - and does the actual selection of listitems
				styleItems.DrawBlockingSelector ();

				GUILayout.BeginHorizontal ();
				GUILayout.Label ("Style:", GUILayout.Width(50F));
				GUILayout.FlexibleSpace ();
				
#if true
				//This draws the button and displays/hides the listbox - returns true if someone clicked the button
			//	if (styleItems.styleListSelectedItem == null)
			//		DrawCall ();

				styleItems.DrawButton ();
				#endif
				GUILayout.FlexibleSpace ();
				//style = GUILayout.TextField (style, GUILayout.MinWidth (50F), GUILayout.MaxWidth (150F));
				GUILayout.EndHorizontal ();

				//These two go at the end so the dropdown is drawn over the other controls, and closes the dropdown if someone clicks elsewhere in the window
				styleItems.DrawDropDown ();
				styleItems.CloseOnOutsideClick ();

				if (!styleItems.ListVisible) {
					GUILayout.BeginHorizontal ();
					GUILayout.Label ("", GUILayout.Height (7F));
					GUILayout.EndHorizontal ();
					GUILayout.BeginHorizontal ();

#if false
					styleLabel.normal.textColor = Color.yellow;
					GUILayout.Label ("Picture or Imgur album url:", styleLabel);
					
#else
					//GUILayout.FlexibleSpace ();
					if (GUILayout.Button ("Pic or Imgur url:", GUILayout.Width(125F))) {
						downloadState = downloadStateType.FILESELECTION;
						if (m_textPath == null)
							m_textPath = "";
						if (m_textPath.Contains (".png") || m_textPath.Contains (".jpg"))
							m_textPath = System.IO.Path.GetDirectoryName (m_textPath);
						getFile ("Enter Craft File", ".png|.jpg", FileOperations.ROOT_PATH + "/screenshots");
					}
#endif
					GUILayout.FlexibleSpace ();

					pictureUrl = GUILayout.TextField (pictureUrl, GUILayout.Width (300F));


					GUILayout.EndHorizontal ();
					GUILayout.BeginHorizontal ();
					if (GUILayout.Button ("Generate a custom snapshot", GUILayout.Width (200F))) {
						downloadState = downloadStateType.GET_THUMBNAIL;

					}
					GUILayout.Label ("(if no image is created or specified, a default one will be created and uploaded)");
					GUILayout.EndHorizontal ();

					GUILayout.BeginHorizontal ();
					GUILayout.Label ("", GUILayout.Height (7F));
					GUILayout.EndHorizontal ();

					GUILayout.BeginHorizontal ();
					GUILayout.Label ("Video url:", GUILayout.Width (100F));
					GUILayout.FlexibleSpace ();

					videoUrl = GUILayout.TextField (videoUrl, GUILayout.Width (300F));
					GUILayout.EndHorizontal ();
					GUILayout.BeginHorizontal ();
					GUILayout.Label ("", GUILayout.Height (7F));
					GUILayout.EndHorizontal ();
					GUILayout.BeginHorizontal ();
					GUILayout.FlexibleSpace ();
					if (GUILayout.Button ("Action Groups Descriptions", GUILayout.Width (250.0f))) {
						downloadState = downloadStateType.ACTIONGROUPS;
					}
					GUILayout.FlexibleSpace ();
					GUILayout.EndHorizontal ();
					GUILayout.BeginHorizontal ();
					GUILayout.Label ("", GUILayout.Height (7F));
					GUILayout.EndHorizontal ();
					GUILayout.BeginHorizontal ();
					GUILayout.Label ("Force new upload (if existing):");
					GUILayout.FlexibleSpace ();

					forceNew = GUILayout.Toggle (forceNew, "");
					GUILayout.FlexibleSpace ();
					if (!forceNew) {
						if (lastSelectedCraft >= 0) {
							GUILayout.Label ("Craft update id: " + ids [lastSelectedCraft]);
						} else {
							GUILayout.FlexibleSpace ();
							var styleButton = new GUIStyle (GUI.skin.label);
							styleButton = new GUIStyle (GUI.skin.button);
							if (uid == "" || pswd == "")
								styleButton.normal.textColor = Color.gray;
							if (GUILayout.Button ("Select Existing Craft to Replace", styleButton, GUILayout.Width (250.0f))) {
								if (uid != "" && pswd != "")
									downloadState = downloadStateType.GET_DUP_CRAFT_LIST;
							}

						}
					} else {
						GUILayout.FlexibleSpace ();
						GUILayout.Label ("", GUILayout.Width (250.0f));
					}


					GUILayout.EndHorizontal ();

					GUILayout.BeginHorizontal ();
					GUILayout.Label ("", GUILayout.Height (10F));
					GUILayout.EndHorizontal ();

					DrawTitle ("KerbalX Info", GUILayout.Width (150F));

					GUILayout.BeginHorizontal ();
					GUILayout.Label ("Enter userid:", styleLabel, GUILayout.Width (125F));

					
#if true
					GUILayout.FlexibleSpace ();
					uid = GUILayout.TextField (uid, GUILayout.Width (150F));



					GUILayout.Label ("", GUILayout.Width (20F));
					GUILayout.Label ("Save userid:", GUILayout.Width (100F));
					saveUid = GUILayout.Toggle (saveUid, "");
					GUILayout.FlexibleSpace ();


					GUILayout.EndHorizontal ();


					GUILayout.BeginHorizontal ();
					GUILayout.Label ("Enter password:", styleLabel, GUILayout.Width (125F));
					GUILayout.FlexibleSpace ();

					pswd = GUILayout.PasswordField (pswd, '*', 25, GUILayout.Width (150F));
					GUILayout.Label ("", GUILayout.Width (20F));
					GUILayout.Label ("Save password:", GUILayout.Width (100F));


					if (!saveUid)
						savePswd = false;

					savePswd = GUILayout.Toggle (savePswd, "");
					GUILayout.FlexibleSpace ();
					GUILayout.EndHorizontal ();

					GUILayout.BeginHorizontal ();
					GUILayout.Label ("");
					GUILayout.EndHorizontal ();

					GUILayout.BeginHorizontal ();
					GUILayout.FlexibleSpace ();
					if (GUILayout.Button ("Upload to KerbalX", GUILayout.Width (150.0f), GUILayout.Height (40))) {
						if (thisCI.configuration.showWarning &&
						    (pictureUrl == "" || pictureUrl.StartsWith ("file://"))) {
							downloadState = downloadStateType.SHOW_WARNING;
						} else {
							_image = null;
							uploadCraft (craftURL);
						}
					}

					GUILayout.FlexibleSpace ();
					#endif
				}
			} else {
				GUILayout.BeginHorizontal ();
				GUILayout.Label ("Craft File does not exist: ");
				GUILayout.FlexibleSpace ();
				//GUILayout.EndHorizontal ();
				//GUILayout.BeginHorizontal ();
				GUILayout.FlexibleSpace ();
				GUILayout.Label (craftURL);
				GUILayout.EndHorizontal ();
				GUILayout.BeginHorizontal ();
			}
			if (!styleItems.ListVisible) {
				if (GUILayout.Button ("Cancel", GUILayout.Width (125.0f))) {
					resetBeforeExit ();
					GUIToggle ();
				}
				GUILayout.FlexibleSpace ();

				GUILayout.EndHorizontal ();
			}
		}

		void selectDupCraftCase ()
		{
			GUILayout.BeginHorizontal ();
			GUILayout.Label ("Select Craft to replace");
			GUILayout.FlexibleSpace ();
			GUILayout.EndHorizontal ();

			GUILayout.BeginHorizontal ();
			GUILayout.Label ("KerbalX id", GUILayout.Width (90F));
			GUILayout.FlexibleSpace ();
			GUILayout.Label ("Craft Name (clickable URL)");
			GUILayout.FlexibleSpace ();
			GUILayout.Label ("Select");
			GUILayout.EndHorizontal ();

			GUILayout.BeginHorizontal ();

			buildMenuScrollPosition = GUILayout.BeginScrollView (buildMenuScrollPosition, GUILayout.Width (WIDTH - 30), GUILayout.Height (HEIGHT * 2 / 3));
			GUILayout.BeginVertical ();


			for (int i = 0; i < craftNames.Count; i++) {
				GUILayout.BeginHorizontal ();

				GUILayout.Label (ids [i] + ":", GUILayout.Width (90F));
				GUILayout.FlexibleSpace ();
				if (GUILayout.Button (craftNames [i])) {
					//Application.OpenURL (uploadServer + craftUrls [i]);
					openUrl (uploadServer () + "/crafts/" + ids [i]);
				}
				
#if false
				// Get the last rect to display the line
				var lastRect = GUILayoutUtility.GetLastRect();
				lastRect.y += lastRect.height - 2; // Vertical alignment of the underline
				lastRect.height = 2; // Thickness of the line
				GUI.Box(lastRect, "");
#endif
				GUILayout.FlexibleSpace ();
				if (lastSelectedCraft >= 0 && lastSelectedCraft != i)
					selectedCraft [i] = false;

				selectedCraft [i] = GUILayout.Toggle (selectedCraft [i], "");
				if (selectedCraft [i] && i != lastSelectedCraft)
					lastSelectedCraft = i;

				GUILayout.EndHorizontal ();
			}
			GUILayout.EndVertical ();
			GUILayout.EndScrollView (); 
			GUILayout.EndHorizontal ();

			GUILayout.BeginHorizontal ();
			GUILayout.Label ("");
			GUILayout.EndHorizontal ();
			GUILayout.BeginHorizontal ();
			GUILayout.FlexibleSpace ();
			if (lastSelectedCraft >= 0) {
				if (GUILayout.Button ("OK", GUILayout.Width (125.0f))) {
					downloadState = downloadStateType.UPLOAD;
				} 
			} else {
				if (GUILayout.Button ("Cancel", GUILayout.Width (125.0f))) {
					downloadState = downloadStateType.UPLOAD;
				}
			}

			GUILayout.FlexibleSpace ();

			GUILayout.EndHorizontal ();
		}

		void warningCase ()
		{
			GUILayout.BeginHorizontal ();
			GUILayout.Label ("Image Upload Warning");
			GUILayout.FlexibleSpace ();
			GUILayout.EndHorizontal ();
			GUILayout.BeginHorizontal ();
			GUILayout.Label ("");
			GUILayout.EndHorizontal ();


			GUILayout.BeginHorizontal ();
			GUILayout.Label ("Images uploaded with this mod will be uploaded to Imgur.");
			GUILayout.EndHorizontal ();

			GUILayout.BeginHorizontal ();
			GUILayout.Label ("Once uploaded, they CANNOT be deleted.");
			GUILayout.EndHorizontal ();

			GUILayout.BeginHorizontal ();
			GUILayout.Label ("");
			GUILayout.EndHorizontal ();

			GUILayout.BeginHorizontal ();
			GUILayout.FlexibleSpace ();
			if (GUILayout.Button ("OK", GUILayout.Width (125.0f))) {
				uploadCraft (craftURL);
			}

			GUILayout.FlexibleSpace ();
			if (GUILayout.Button ("Cancel", GUILayout.Width (125.0f))) {
				downloadState = downloadStateType.GUI;
			}
			GUILayout.FlexibleSpace ();
			GUILayout.EndHorizontal ();
			GUILayout.BeginHorizontal ();
			GUILayout.Label ("");
			GUILayout.EndHorizontal ();

			GUILayout.BeginHorizontal ();
			GUILayout.Label ("");
			GUILayout.EndHorizontal ();


			GUILayout.BeginHorizontal ();
			GUILayout.Label ("Show this warning in the future:");
			GUILayout.FlexibleSpace ();
			thisCI.configuration.showWarning = GUILayout.Toggle (thisCI.configuration.showWarning, "");
			GUILayout.FlexibleSpace ();
			GUILayout.EndHorizontal ();

		}

		int newresolution = 1024;
		int displayres = 512;
		float newelevation = 0, newazimuth = 0, newpitch = 0, newheading = 0, newfov = 0;
		float new2elevation = 0, new2azimuth = 0, new2pitch = 0, new2heading = 0, new2fov = 0;

		bool thumbnailInited = false;
		ShipConstruct ship;
		string facility;
		Texture2D _image = null;

		Rect imgbounds = new Rect (20, 20, 512, 512 + 30);
		bool changed = true;

		void imgWindow (int id)
		{
			GUILayout.BeginHorizontal ();
			GUI.DrawTexture (new Rect (0f, 30f, displayres, displayres), _image, ScaleMode.ScaleToFit, true, 0f);
			GUILayout.EndHorizontal ();
			GUI.DragWindow ();
		}

		bool checkedForThumbnail = false;
		int changecount;
		void getThumbnailcase ()
		{
			if (System.IO.File.Exists (craftURL)) {
				if (!checkedForThumbnail) {
					checkedForThumbnail = true;
					if (checkMissingParts (System.IO.File.ReadAllText (craftURL))) {
						uploadErrorMessage = "Craft file must be loadable.";
						uploadErrorMessage += instructions;
						instructions = "";
						downloadState = downloadStateType.UPLOAD_ERROR;
						return;
					}
				}
				if (!thumbnailInited) {
					thumbnailInited = true;
					changecount = 2;
					_image = null;
					displayres = 512;

			//		EditorLogic.fetch.newBtn.Invoke(EditorLogic.fetch.newBtn.methodToInvoke, 0);

					EditorLogic.LoadShipFromFile (craftURL);
					ship = ShipConstruction.LoadShip (craftURL);
					Log.Info ("Ship loaded");

					if (ship.shipFacility != EditorFacility.VAB) {
						facility = "SPH";
						newresolution = thisCI.configuration.sphResolution;
						newelevation = thisCI.configuration.sphElevation;
						newazimuth = thisCI.configuration.sphAzimuth;
						newpitch = thisCI.configuration.sphPitch;
						newheading = thisCI.configuration.sphHeading;
						newfov = thisCI.configuration.sphFov;
					} else {
						facility = "VAB";
						newresolution = thisCI.configuration.vabResolution;
						newelevation = thisCI.configuration.vabElevation;
						newazimuth = thisCI.configuration.vabAzimuth;
						newpitch = thisCI.configuration.vabPitch;
						newheading = thisCI.configuration.vabHeading;
						newfov = thisCI.configuration.vabFov;
					}
				}

				GUILayout.BeginHorizontal ();
				GUILayout.Label ("Create Craft image: ");

				GUILayout.FlexibleSpace ();
				var styleLabel = new GUIStyle (GUI.skin.label);
				styleLabel.normal.textColor = Color.green;
				GUILayout.Label (ship.shipName, styleLabel);
				GUILayout.FlexibleSpace ();
				//GUILayout.FlexibleSpace ();
				GUILayout.EndHorizontal ();

				GUILayout.BeginHorizontal ();
				GUILayout.Label ("");
				GUILayout.EndHorizontal ();

				GUILayout.BeginHorizontal ();
				GUILayout.Label ("Display Resolution(" + displayres.ToString () + "):");
				GUILayout.FlexibleSpace ();
				displayres = (int)GUILayout.HorizontalSlider (displayres, 256, 1024, GUILayout.Width (300F));
				if (displayres > newresolution)
					newresolution = displayres;
				GUILayout.EndHorizontal ();


				GUILayout.BeginHorizontal ();
				GUILayout.Label ("Image Resolution(" + newresolution.ToString () + "):");
				GUILayout.FlexibleSpace ();
				newresolution = (int)GUILayout.HorizontalSlider (newresolution, 256, 1024, GUILayout.Width (300F));
				GUILayout.EndHorizontal ();

				GUILayout.BeginHorizontal ();
				GUILayout.Label ("Elevation (" + newelevation.ToString () + " degrees):");
				GUILayout.FlexibleSpace ();
				new2elevation = GUILayout.HorizontalSlider (newelevation, 0, 90, GUILayout.Width (300F));
				GUILayout.EndHorizontal ();

				GUILayout.BeginHorizontal ();
				GUILayout.Label ("Azimuth (" + newazimuth.ToString () + " degrees):");
				GUILayout.FlexibleSpace ();
				new2azimuth = GUILayout.HorizontalSlider (newazimuth, 0, 259, GUILayout.Width (300F));
				GUILayout.EndHorizontal ();

				GUILayout.BeginHorizontal ();
				GUILayout.Label ("Pitch (" + newpitch.ToString () + " degrees):");
				GUILayout.FlexibleSpace ();
				new2pitch = GUILayout.HorizontalSlider (newpitch, 0, 90, GUILayout.Width (300F));
				GUILayout.EndHorizontal ();

				GUILayout.BeginHorizontal ();
				GUILayout.Label ("Heading (" + newheading.ToString () + " degrees):");
				GUILayout.FlexibleSpace ();
				new2heading = GUILayout.HorizontalSlider (newheading, 0, 259, GUILayout.Width (300F));
				GUILayout.EndHorizontal ();

				GUILayout.BeginHorizontal ();
				GUILayout.Label ("FOV (" + newfov.ToString () + "):");
				GUILayout.FlexibleSpace ();
				new2fov = GUILayout.HorizontalSlider (newfov, 0.25F, 3.0F, GUILayout.Width (300F));
				GUILayout.EndHorizontal ();

				if (newelevation != new2elevation || new2azimuth != newazimuth || new2pitch != newpitch || new2heading != newheading || new2fov != newfov) {
					newelevation = new2elevation;
					newazimuth = new2azimuth;
					newpitch = new2pitch;
					newheading = new2heading;
					newfov = new2fov;
					changed = true;
				}
				GUILayout.BeginHorizontal ();
				GUILayout.Label ("");
				GUILayout.EndHorizontal ();


				GUILayout.BeginHorizontal ();
				styleLabel = new GUIStyle (GUI.skin.label);
				styleLabel.normal.textColor = thisCI.configuration.backgroundcolor;
				GUILayout.Label ("Click on desired background color:", styleLabel);
				GUILayout.FlexibleSpace ();
				GUILayout.BeginArea (new Rect (250, 250, colorPickerImageWidth, colorPickerImageHeight));
				if (GUILayout.RepeatButton (colorPicker, GUILayout.Width (colorPickerImageWidth), GUILayout.Height (colorPickerImageHeight))) {
					Vector2 pickpos = Event.current.mousePosition;
					int aaa = Convert.ToInt32 (pickpos.x) * colorPicker.width / colorPickerImageWidth;
					int bbb = Convert.ToInt32 (pickpos.y) * colorPicker.height / colorPickerImageHeight;
					Color col = colorPicker.GetPixel (aaa, colorPicker.height - bbb);

					// "col" is the color value that Unity is returning.
					// Here you would do something with this color value, like
					// set a model's material tint value to this color to have it change
					// colors, etc, etc.
					//
					Log.Info ("Mouse position: " + pickpos.x.ToString () + "," + pickpos.y.ToString () + "   col: " + col.ToString ());
					thisCI.configuration.backgroundcolor = col;
					changed = true;
				}
				GUILayout.EndArea ();
				GUILayout.FlexibleSpace ();
				GUILayout.EndHorizontal ();
				GUILayout.BeginHorizontal ();
				GUILayout.Label ("");
				GUILayout.EndHorizontal ();
				GUILayout.BeginHorizontal ();
				if (changed) {
					// This is a hack until I can figure out why I'm getting the new craft
					// superimposed on the old craft for the first frame.
					if (changecount == 0)
						changed = false;
					else
						changecount--;
					Log.Info ("Capturing thumbnail");
					ThumbnailHelper.CaptureThumbnail (ship, newresolution, newelevation, newazimuth, newpitch, newheading, newfov,
						"screenshots", ship.shipName);
					
					pictureUrl = "file://" + FileOperations.ROOT_PATH + "Screenshots/" + ship.shipName + ".png";
					jpgToDelete = true;
					string pngToConvert = pictureUrl.Substring (7);
					convertedJpg = pictureUrl.Substring (7, pictureUrl.Length - 10) + "jpg";
					Log.Info ("pngToconvert: " + pngToConvert);
					Log.Info ("convertedJpg: " + convertedJpg);
					ConvertToJPG (pngToConvert, convertedJpg, thisCI.configuration.backgroundcolor);
					System.IO.FileInfo file = new System.IO.FileInfo (pngToConvert);
					file.Delete ();

					if (_image == null)
						resetWinPos = true;
					byte[] fileData = System.IO.File.ReadAllBytes (convertedJpg);
					_image = new Texture2D (2, 2);
					_image.LoadImage (fileData); //..this will auto-resize the texture dimensions.

					Log.Info ("image.width: " + _image.width + "   image.height: " + _image.height);
					//GUI.DrawTexture(new Rect(0f, 20f, 512, 512), _image, ScaleMode.ScaleToFit, true, 0f);
					//GUILayout.Box (_image);
				}
				GUILayout.EndHorizontal ();
				GUILayout.BeginHorizontal ();

				if (GUILayout.Button ("Reset to " + facility + " Defaults", GUILayout.Width (150.0f))) {
					thumbnailInited = false;
					if (ship.shipFacility != EditorFacility.VAB) {
						thisCI.configuration.setDefaultSPHResolution ();
					} else {
						thisCI.configuration.setDefaultVABResolution ();
					}

				}
				GUILayout.FlexibleSpace ();
				GUILayout.EndHorizontal ();


				GUILayout.BeginHorizontal ();
				if (GUILayout.Button ("Ok", GUILayout.Width (125.0f))) {
					downloadState = downloadStateType.UPLOAD;
					pictureUrl = pictureUrl.Substring (0, pictureUrl.Length - 3) + "jpg";
				}
				GUILayout.EndHorizontal ();
				GUILayout.BeginHorizontal ();

				if (GUILayout.Button ("Cancel", GUILayout.Width (125.0f))) {
					pictureUrl = "";
					thumbnailInited = false;
					_image = null;
					changed = true;
					downloadState = downloadStateType.UPLOAD;
				}
				GUILayout.FlexibleSpace ();
				GUILayout.EndHorizontal ();

			}
		}
		#endif

		void guiCase ()
		{
			GUILayout.BeginHorizontal ();
			GUILayout.Label ("Enter Craft URL:", GUILayout.MinWidth (150F));

			GUILayout.FlexibleSpace ();

			craftURL = GUILayout.TextField (craftURL, GUILayout.MinWidth (50F), GUILayout.MaxWidth (300F));
			craftURL = craftURL.Replace("\n", "").Replace("\r", "");
			if (craftURL.IndexOf ("%") >= 0) {
				craftURL = Uri.UnescapeDataString (craftURL);
			}
			GUILayout.EndHorizontal ();

			GUILayout.BeginHorizontal ();
			GUILayout.Label ("Rename craft to:", GUILayout.MinWidth (150F));
			GUILayout.FlexibleSpace ();
			//GUILayout.EndHorizontal ();
			//GUILayout.BeginHorizontal ();
			//GUILayout.FlexibleSpace ();
			newCraftName = GUILayout.TextField (newCraftName, GUILayout.MinWidth (50F), GUILayout.MaxWidth (300F));
			newCraftName = newCraftName.Replace("\n", "").Replace("\r", "");
			GUILayout.EndHorizontal ();

			GUILayout.BeginHorizontal ();
			var styleLabel = new GUIStyle (GUI.skin.label);
			Color lightBlue = Color.blue;
			styleLabel.normal.textColor = Color.blue;
			lightBlue.r = 30;
			lightBlue.g = 144;
			lightBlue.b = 255;
			//styleLabel.normal.textColor = lightBlue;
			GUILayout.Label ("Save Location", styleLabel);
			GUILayout.EndHorizontal ();

			GUILayout.BeginHorizontal ();
			GUILayout.Label ("Sandbox:", GUILayout.Width (60F));
			saveInSandbox = GUILayout.Toggle (saveInSandbox, "");
			GUILayout.FlexibleSpace ();

			GUILayout.Label ("Original:", GUILayout.Width (60F));
			saveInShipDefault = GUILayout.Toggle (saveInShipDefault, "");
			if (saveInShipDefault) {
				saveInVAB = false;
				saveInSPH = false;
			}
			GUILayout.FlexibleSpace ();

			GUILayout.Label ("VAB:", GUILayout.Width (35F));
			saveInVAB = GUILayout.Toggle (saveInVAB, "");
			if (saveInVAB) {
				saveInShipDefault = false;
				saveInSPH = false;
			}
			GUILayout.FlexibleSpace ();
			GUILayout.Label ("SPH:", GUILayout.Width (35F));
			saveInSPH = GUILayout.Toggle (saveInSPH, "");
			if (saveInSPH) {
				saveInShipDefault = false;
				saveInVAB = false;
			}
			GUILayout.FlexibleSpace ();

			GUILayout.EndHorizontal ();
			GUILayout.BeginHorizontal ();
			GUILayout.FlexibleSpace ();
			GUILayout.Label ("(subassemblies from KerbalX will always be saved in the Subassembliees)", styleLabel, GUILayout.Width (350F));
			GUILayout.FlexibleSpace ();
			GUILayout.EndHorizontal ();

			GUILayout.BeginHorizontal ();
			GUILayout.Label ("Overwrite existing file if exists:", GUILayout.Width (300F));
			//GUILayout.FlexibleSpace ();
			overwriteExisting = GUILayout.Toggle (overwriteExisting, "");
			GUILayout.FlexibleSpace ();

			GUILayout.EndHorizontal ();

			GUILayout.BeginHorizontal ();
			GUILayout.Label ("Load craft after import:", GUILayout.Width (200F));
			//GUILayout.FlexibleSpace ();
			loadAfterImport = GUILayout.Toggle (loadAfterImport, "");
			GUILayout.FlexibleSpace ();

			GUILayout.EndHorizontal ();



			GUILayout.BeginHorizontal ();
			GUILayout.Label (" ");
			GUILayout.EndHorizontal ();


			GUILayout.BeginHorizontal ();
			if (GUILayout.Button ("Select Local File", GUILayout.Width (125.0f))) {
				downloadState = downloadStateType.FILESELECTION;
				if (m_textPath == null) {
					m_textPath = FileOperations.ROOT_PATH + "saves/" + HighLogic.SaveFolder + "/Ships";
					if (EditorDriver.editorFacility == EditorFacility.VAB)
						m_textPath += "/VAB";
					if (EditorDriver.editorFacility == EditorFacility.SPH)
						m_textPath += "/SPH";
					
				}
				Log.Info ("m_textPath: " + m_textPath);
				if (m_textPath.Contains (".craft"))
					m_textPath = System.IO.Path.GetDirectoryName (m_textPath);
				getFile ("Enter Craft File", ".craft");
			}
			GUILayout.EndHorizontal ();

			GUILayout.BeginHorizontal ();
			GUILayout.EndHorizontal ();

			GUILayout.BeginHorizontal ();
			if (GUILayout.Button ("Import", GUILayout.Width (125.0f), GUILayout.Height (40))) {
				if (craftURL != "")
					downloadCraft (craftURL);
			}


			GUILayout.FlexibleSpace ();


			#if EXPORT
			//GUILayout.FlexibleSpace ();
			if (GUILayout.Button ("Upload to KerbalX", GUILayout.Width (200.0f), GUILayout.Height (40))) {
				//if (craftURL == "" || craftURL == null) {
				downloadState = downloadStateType.UPLOAD_FILESELECTION;
				//	if (m_textPath == null)
				m_textPath = FileOperations.ROOT_PATH + "saves/" + HighLogic.SaveFolder + "/Ships";
				if (EditorDriver.editorFacility == EditorFacility.VAB)
					m_textPath += "/VAB/";
				if (EditorDriver.editorFacility == EditorFacility.SPH)
					m_textPath += "/SPH/";
				//	if (m_textPath.Contains (".craft"))
				//		m_textPath = System.IO.Path.GetDirectoryName (m_textPath);
				Log.Info ("m_textPath: " + m_textPath);
				getFile ("Enter Craft File", ".craft");
				//	}else

				//downloadState = downloadStateType.UPLOAD;
				getUidPswd ();
			}
			GUILayout.FlexibleSpace ();
			if (GUILayout.Button ("Cancel", GUILayout.Width (125.0f))) {
				resetBeforeExit ();
				//GUIToggle ();
			}
				
			#else
			GUILayout.Label (" ");
			#endif
			GUILayout.EndHorizontal ();

			GUILayout.BeginHorizontal ();
			DrawTitle ("Options", GUILayout.Width(50F));
			GUILayout.EndHorizontal ();

			GUILayout.BeginHorizontal ();
			GUILayout.Label ("Use Blizzy Toolbar if available:", GUILayout.Width(300F));
		//	GUILayout.FlexibleSpace ();
			newUseBlizzyToolbar = GUILayout.Toggle (newUseBlizzyToolbar, "");
			GUILayout.EndHorizontal ();
			if (Application.platform == RuntimePlatform.WindowsPlayer) {
				GUILayout.BeginHorizontal ();
				GUILayout.Label ("Show drive letters in file selection dialog:", GUILayout.Width(300F));
				//GUILayout.FlexibleSpace ();
				thisCI.configuration.showDrives = GUILayout.Toggle (thisCI.configuration.showDrives, "");
				GUILayout.EndHorizontal ();
			}
		}

		int colorPickerImageWidth = 150;
		int colorPickerImageHeight = 150;
		Texture2D colorPicker;
		Vector2 scrollPosition;

		private void Window (int id)
		{
			if (cfgWinData == false) {
				cfgWinData = true;
				newUseBlizzyToolbar = thisCI.configuration.useBlizzyToolbar;
				//newShowDrives = CI.configuration.showDrives;
				//newCkanExecPath = CI.configuration.ckanExecPath;
				craftURL = "";
				m_textPath = "";
				saveInSandbox = false;
				overwriteExisting = false;
				m_textPath = thisCI.configuration.lastImportDir;
				//		#if EXPORT
				//		for (int i = 0; i < actionGroups.Length; i++)
				//			actionGroups [i] = "";
				//		lastSelectedCraft = -1;
				//		#endif

//				byte[] colorPickerFileData = System.IO.File.ReadAllBytes (FileOperations.ROOT_PATH + "GameData/CraftImport/Textures/colorpicker_texture.jpg");

				byte[] colorPickerFileData = System.IO.File.ReadAllBytes (FileOperations.ROOT_PATH + "GameData/" + CI.TEXTURE_DIR + "/colorpicker_texture.jpg");
				colorPicker = new Texture2D (2, 2);
				colorPicker.LoadImage (colorPickerFileData); //..this will auto-resize the texture dimensions.

			} 

			SetVisible (true);
			GUI.enabled = true;

			GUILayout.BeginHorizontal ();
			GUILayout.EndHorizontal ();

			GUILayout.BeginVertical ();

			//	DrawTitle ("Craft Import");

			//Log.Info ("downloadState: " + downloadState.ToString ());
			switch (downloadState) {
#if EXPORT
			case downloadStateType.UPLOAD:
				uploadCase ();

				break;
			case downloadStateType.GET_DUP_CRAFT_LIST:
				getUpdateCraftList ();

				break;

			case downloadStateType.GET_THUMBNAIL:
				getThumbnailcase ();
				break;

			case downloadStateType.SELECT_DUP_CRAFT:
				selectDupCraftCase ();

				break;

			case downloadStateType.ACTIONGROUPS:
				GUILayout.BeginHorizontal ();
				GUILayout.Label ("Action Groups");
				GUILayout.FlexibleSpace ();
				GUILayout.EndHorizontal ();

				GUILayout.BeginHorizontal ();

				buildMenuScrollPosition = GUILayout.BeginScrollView (buildMenuScrollPosition, GUILayout.Width (WIDTH - 30), GUILayout.Height (HEIGHT * 2 / 3));
				GUILayout.BeginVertical ();


				for (int i = 0; i < 16; i++) {
					GUILayout.BeginHorizontal ();
					if (i < 10) {
						GUILayout.Label ((i + 1).ToString () + ":");
					} else {
						GUILayout.Label (fixedActions [i - 10]);
					}
					GUILayout.FlexibleSpace ();
					actionGroups [i] = GUILayout.TextField (actionGroups [i], GUILayout.MinWidth (50F), GUILayout.MaxWidth (300F));
					actionGroups [i] = actionGroups [i].Replace("\n", "").Replace("\r", "");
					GUILayout.EndHorizontal ();
				}
				GUILayout.EndVertical ();
				GUILayout.EndScrollView (); 
				GUILayout.EndHorizontal ();

				GUILayout.BeginHorizontal ();
				GUILayout.Label ("");
				GUILayout.EndHorizontal ();
				GUILayout.BeginHorizontal ();
				GUILayout.FlexibleSpace ();
				if (GUILayout.Button ("OK", GUILayout.Width (125.0f))) {
					
					downloadState = downloadStateType.UPLOAD;
				}
				GUILayout.FlexibleSpace ();

				GUILayout.EndHorizontal ();
				break;
#endif
				
			case downloadStateType.GUI:
				guiCase ();
				break;
#if EXPORT
			case downloadStateType.SHOW_WARNING:
				warningCase ();
				break;

//			case downloadStateType.FILESELECTION:
//				getFile ();
//				break;

			case downloadStateType.UPLOAD_IN_PROGRESS:
#endif
			case downloadStateType.IN_PROGRESS:
				Log.Info ("IN_PROGRESS");
				GUILayout.BeginHorizontal ();
				GUILayout.FlexibleSpace ();
				if (downloadState == downloadStateType.IN_PROGRESS)
					GUILayout.Label ("Download in progress");
				else
					GUILayout.Label ("Upload in progress");
				GUILayout.FlexibleSpace ();
				GUILayout.EndHorizontal ();

				GUILayout.BeginHorizontal ();
				GUILayout.Label ("");
				GUILayout.EndHorizontal ();

				GUILayout.BeginHorizontal ();
				GUILayout.FlexibleSpace ();
				if (downloadState == downloadStateType.IN_PROGRESS) {
					if (download != null)
						GUILayout.Label ("Progress: " + (100 * download.progress).ToString () + "%");
				}
				#if EXPORT
				else {
					if (Time.realtimeSinceStartup - lastUpdate > 1) {
						lastUpdate = Time.realtimeSinceStartup;
						uploadProgress++;
					}
					GUILayout.Label ("Upload progress: " + uploadProgress.ToString ());
				}
				#endif
				GUILayout.FlexibleSpace ();
				GUILayout.EndHorizontal ();

				GUILayout.BeginHorizontal ();
				GUILayout.Label ("");
				GUILayout.EndHorizontal ();

				GUILayout.BeginHorizontal ();
				GUILayout.FlexibleSpace ();
				if (GUILayout.Button ("Cancel", GUILayout.Width (125.0f))) {
					resetBeforeExit ();
				}
				GUILayout.FlexibleSpace ();
				GUILayout.EndHorizontal ();
				break;

				#if EXPORT
			case downloadStateType.UPLOAD_COMPLETED:
				#endif
			case downloadStateType.COMPLETED:
				GUILayout.BeginHorizontal ();
				GUILayout.FlexibleSpace ();
				if (downloadState == downloadStateType.COMPLETED)
					GUILayout.Label ("Download completed");
				else
					GUILayout.Label ("Upload completed");
				
				GUILayout.FlexibleSpace ();
				GUILayout.EndHorizontal ();
				GUILayout.BeginHorizontal ();
				GUILayout.Label ("", GUILayout.Height (10));
				GUILayout.EndHorizontal ();
				GUILayout.BeginHorizontal ();
				if (instructions.Trim () != "") {
					scrollPosition = GUILayout.BeginScrollView (
						scrollPosition, GUILayout.Width (WIDTH - 20), GUILayout.Height (HEIGHT - 100));
					GUILayout.Label (instructions);
					GUILayout.EndScrollView ();
				} else
					GUILayout.Label ("");
				GUILayout.EndHorizontal ();
				GUILayout.BeginHorizontal ();
				GUILayout.Label ("", GUILayout.Height (10));
				GUILayout.EndHorizontal ();
				GUILayout.BeginHorizontal ();
				GUILayout.FlexibleSpace ();
				if (GUILayout.Button ("OK", GUILayout.Width (125.0f))) {

					if (downloadState == downloadStateType.COMPLETED && loadAfterImport) {
						Log.Info ("saveFile: " + saveFile);
						if (!subassembly)
							EditorLogic.LoadShipFromFile (saveFile);
						else {
							if (EditorLogic.RootPart) 
								LoadCraftAsSubassembly (saveFile);
						//	ShipConstruct ship = ShipConstruction.LoadShip (craftURL);
						//	EditorLogic.SpawnConstruct (ship);
						}

					}
					resetBeforeExit ();
				}
				GUILayout.FlexibleSpace ();
				if (instructions != "") {
					if (GUILayout.Button ("Copy to clipboard")) {
						TextEditor te = new TextEditor ();
						te.text = instructions;
						te.SelectAll ();
						te.Copy ();
					}
					GUILayout.FlexibleSpace ();
				}
				GUILayout.EndHorizontal ();
				break;

			case downloadStateType.FILE_EXISTS:
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
					resetBeforeExit ();
				}
				GUILayout.EndHorizontal ();
				break;

				#if EXPORT
			case downloadStateType.UPLOAD_ERROR:
				#endif
			case downloadStateType.ERROR:
				GUILayout.BeginHorizontal ();
				GUILayout.FlexibleSpace ();
				if (downloadState == downloadStateType.ERROR)
					GUILayout.Label ("Download Error");
				else
					GUILayout.Label ("Upload Error");
				GUILayout.FlexibleSpace ();
				GUILayout.EndHorizontal ();
				GUILayout.BeginHorizontal ();
				GUILayout.FlexibleSpace ();
				#if EXPORT
				if (downloadState == downloadStateType.UPLOAD_ERROR)
					GUILayout.Label (uploadErrorMessage);
				else
				#endif
				GUILayout.Label (downloadErrorMessage);
				GUILayout.FlexibleSpace ();
				GUILayout.EndHorizontal ();
				GUILayout.BeginHorizontal ();
		#if EXPORT
				if (GUILayout.Button ("OK", GUILayout.Width (125.0f))) {
					downloadState = downloadStateType.UPLOAD;
				}
		#endif
				GUILayout.FlexibleSpace ();
				if (GUILayout.Button ("Cancel", GUILayout.Width (125.0f))) {
					resetBeforeExit ();
				}
				GUILayout.FlexibleSpace ();
				GUILayout.EndHorizontal ();
				break;

			default:
				break;
			}
			GUILayout.EndVertical ();
			GUI.DragWindow ();
		}

		private void DrawTitle (String text)
		{
			GUILayout.BeginHorizontal ();
			GUILayout.Label (text, HighLogic.Skin.label);
			//GUILayout.FlexibleSpace ();
			GUILayout.EndHorizontal ();
		}
		private void DrawTitle (String text, GUILayoutOption layout)
		{
			GUILayout.BeginHorizontal ();
			GUILayout.Label (text, HighLogic.Skin.label, layout);
			//GUILayout.FlexibleSpace ();
			GUILayout.EndHorizontal ();
		}
		public void GUI_SaveData ()
		{
			thisCI.configuration.useBlizzyToolbar = newUseBlizzyToolbar;
			//CI.configuration.ckanExecPath = newCkanExecPath;
			//CI.configuration.showDrives = newShowDrives;
			//CI.configuration.lastImportDir = m_textPath;
		}

		public void set_CI_Button_active ()
		{
			
			if (!thisCI.configuration.useBlizzyToolbar) {
				CI_Button.SetTexture (CI_button_img);

			}
		}

		public void initGUIToggle()
		{
			Log.Info ("initGUIToggle");
			SetVisible (true);
			downloadState = downloadStateType.GUI;
			craftURL = "";
			m_textPath = "";
			newCraftName = "";
			saveInSandbox = false;
			overwriteExisting = false;

#if EXPORT
			pictureUrl = "";
			videoUrl = "";
			forceNew = false;
			for (int i = 0; i < actionGroups.Length; i++)
				actionGroups [i] = "";
			lastSelectedCraft = -1;
			Log.Info ("initGUIToggle e");
#endif

			Log.Info ("Setting screen lock");
			InputLockManager.SetControlLock ((ControlTypes.EDITOR_LOCK | ControlTypes.EDITOR_GIZMO_TOOLS), "CraftImportLock");
		}

		public void endGUIToggle()
		{
			Log.Info ("endGUIToggle");
			SetVisible (false);
			set_CI_Button_active ();
			cfgWinData = false;
			RemoveInputLock ();
			GUI_SaveData ();

			thisCI.configuration.Save ();

		}
		public void GUIToggle ()
		{
			Log.Info ("GUIToggle");
			downloadState = downloadStateType.GUI;
			CIInfoDisplay.infoDisplayActive = !CIInfoDisplay.infoDisplayActive;
			if (CIInfoDisplay.infoDisplayActive) {
				initGUIToggle ();
				CI_Button.SetTexture (CI_button_img); 

			} else {
				endGUIToggle ();
				if (thisCI.configuration.BlizzyToolbarIsAvailable && thisCI.configuration.useBlizzyToolbar) {
					HideToolbarStock ();
				} else {
					UpdateToolbarStock ();
					set_CI_Button_active ();

				}

			}
		}
	}
}