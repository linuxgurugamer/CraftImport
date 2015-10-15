using System;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

using SimpleJSON;

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
		//		internal override void DrawWindow(int id)
		//		{
		//		}
		private const int WIDTH = 500;
		private const int HEIGHT = 400;
		private const int BR_WIDTH = 600;
		private const int BR_HEIGHT = 500;

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
		//private static bool newShowDrives;
		private static bool saveInSandbox = false;
		private static bool overwriteExisting = false;
		private static bool appLaucherHidden = true;
		private static string craftURL = "";
		private static string newCraftName = "";
		//private static string newCkanExecPath = "";

		string instructions = "";

		#if EXPORT
		#if DEBUG
		string uploadServer = "http://kerbalx-stage.herokuapp.com";
		#else
		//string uploadServer = "https://kerbalx.com";
		string uploadServer = "https://kerbalx-stage.herokuapp.com";
		#endif
		#endif

		enum downloadStateType
		{
			#if EXPORT
			UPLOAD,
			UPLOADINPROGRESS,
			UPLOADCOMPLETED,
			UPLOADERROR,
			ACTIONGROUPS,
			GETDUPCRAFTLIST,
			UPDATECRAFTLISTINPROGRESS,
			SELECTDUPCRAFT,
			#endif
			INACTIVE,
			GUI,
			FILESELECTION,
			INPROGRESS,
			COMPLETED,
			FILEEXISTS,
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
				
				RenderingManager.AddToPostDrawQueue (0, new Callback (DrawCall));
				firstAwake = true;
			}

			//register for the event
			styleItems.SelectionChanged += styleItemSelectionChanged;
		
		}

		public void OnDestroy ()
		{
			
			RenderingManager.RemoveFromPostDrawQueue (0, new Callback (DrawCall));
		}

		//Rect windowRect = new Rect(300, 0, 200, 200);
		Boolean StylesSet = false;

		void DrawCall ()
		{
			Log.Info ("DrawCall");
			if (!StylesSet) {

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
		#endif


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
			//Log.Info ("UpdateToolbarStock, appLaucherHidden: " + appLaucherHidden.ToString ());

			// Create the button in the KSP AppLauncher
			if (!CI_Texture_Load) {
				if (GameDatabase.Instance.ExistsTexture (CI.TEXTURE_DIR + "CI-38"))
					CI_button_img = GameDatabase.Instance.GetTexture (CI.TEXTURE_DIR + "CI-38", false);
				//if (GameDatabase.Instance.ExistsTexture (CI.TEXTURE_DIR + "CI-folder"))
				//	CI_button_off = GameDatabase.Instance.GetTexture (CI.TEXTURE_DIR + "CI-folder", false);
				

				CI_Texture_Load = true;
			}
			if (CI_Button == null && !appLaucherHidden) {
				
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

		bool saveCraftFile (string craftFile)
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

			string shipName = null;
			Match t = null;
			try {
				var s = Regex.Match (craftFile, "^ship.*=.*");
				s = Regex.Match (s.ToString (), "=.*");

				shipName = s.ToString ().Remove (0, 1).Trim ();

				t = Regex.Match (craftFile, "type.*=.*");
			
				//Log.Info ("regex: " + t.ToString ());
				bool vab = t.ToString ().Contains ("VAB");
				bool sph = t.ToString ().Contains ("SPH");

				string strCraftFile = craftFile.ToString ();
				if (newCraftName != "") {
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
					return false;
				}

				System.IO.File.WriteAllText (saveFile, strCraftFile);
				downloadState = downloadStateType.COMPLETED;
			} catch (Exception e) { 
				Log.Info ("Regex error: " + e);

				//downloadErrorMessage = download.error;
				downloadErrorMessage = "Download URL did not specify a valid .craft file";
				downloadState = downloadStateType.ERROR;
				return false;
			}
			return true;
		}


		WWW download;
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
				// Create a download object
				Log.Info ("s: " + s);
				download = new WWW (s);
				// Wait until the download is done
				yield   return download;
				if (!String.IsNullOrEmpty (download.error)) {
					Log.Error ("Error downloading: " + download.error);
					downloadErrorMessage = download.error;
					downloadState = downloadStateType.ERROR;
					yield break;
				} else {
					bool b = saveCraftFile (download.text);

					saveInSandbox = false;
					overwriteExisting = false;

					if (b) {
						//downloadState = downloadStateType.COMPLETED;
						if (kerbalx) {
							
							bool https = (craftURL.IndexOf ("https://kerbalx.com", StringComparison.OrdinalIgnoreCase) == 0);
							int dot = craftURL.IndexOf (".craft");
							if (dot > 0) {
								craftURL = craftURL.Substring (0, dot);
							}
							Log.Info ("craftURL on kerbalx: " + craftURL);
							s = craftURL + ".json";
							download.Dispose ();
							download = new WWW (s);
							// Wait until the download is done
							yield   return download;
							var j = JSON.Parse (download.text);
							int i = j ["mods"].Count;
							string m;
							for (int i1 = 0; i1 < i; i1++) {
								m = j ["mods"] [i1];
								Log.Info ("Mod: " + m);
							}

							s = craftURL + ".ckan";
							download.Dispose ();
							download = new WWW (s);
							// Wait until the download is done
							yield   return download;

							// 1. Need to get current ksp directory for ckan, root path ends in a slash
							string root = KSPUtil.ApplicationRootPath.Substring (0, KSPUtil.ApplicationRootPath.Length - 12);
							Log.Info ("Root path: " + root);
							string ckanDir = root + "CKAN";
							if (System.IO.Directory.Exists (ckanDir)) {
								string ckanFile = j ["name"] + ".ckan";
								Log.Info ("ckanFile: " + ckanFile);
								string ckanFilePath = ckanDir + "/" + ckanFile;
								System.IO.File.WriteAllText (ckanFilePath, download.text);
								instructions = "If you can't load the imported craft: " + j ["name"] +
								", then you will have to load the mods needed.  A .ckan file has been saved in the CKAN directory for the current game:\n\n" +
								ckanFilePath + "\n\nYou can use CKAN to load all missing mods by following these instructions:\n\n" +
								"1.  Start CKAN\n2.  Select File->Import from .ckan\n3.  Navigate to the folder:      " + ckanDir +
								"\n4.  Select the file:      " + ckanFile + "\n";
							} else {
								if (j ["mods"].Count > 0) {
									instructions = "If you can't load the imported craft: " + j ["name"] + ", then you will have to load the necessary mods by hand." +
									"\n\nYou don't seem to have CKAN installed in this game, so you will have to install them manually.\n\n" +
									"Here is the list of mods that this craft file needs:\n\n";
									for (int i1 = 0; i1 < i; i1++) {
										m = j ["mods"] [i1];
										instructions = instructions + m + "\n";
									}
								} else {
									instructions = "You should be able to load the craft in either the VAB or SPH";
								}
							}


							#if false
							// 2. Need to get list of installed mods from ckan
							var proc = new Process {
								StartInfo = new ProcessStartInfo {
									FileName = CI.configuration.ckanExecPath,
									Arguments = "list --kspdir \"" + KSPUtil.ApplicationRootPath + "\"" ,
									UseShellExecute = false,
									RedirectStandardOutput = true,
									CreateNoWindow = true
								}
							};
							proc.Start();
							while (!proc.StandardOutput.EndOfStream) {
								string line = proc.StandardOutput.ReadLine();
								Log.Info ("ckan output: " + line);
								// do something with line
							}
							#endif
							// mono ckan.exe list --kspdir "/Users/jbayer/KerbalInstalls/1.0 We Have Liftoff/test/KSP_all_2_1.0.4"

							// Need to run this to do install:
							//
							// mono ckan.exe install -c mod.ckan --kspdir "/Users/jbayer/KerbalInstalls/1.0 We Have Liftoff/test/KSP_all_2_1.0.4"

						} else {
							instructions = "If you can't load the imported craft, then you will have to load the necessary mods by hand.";
						}
					}
					craftURL = "";
				}
				download.Dispose ();
				download = null;
			}
		}

		void downloadCraft (string s)
		{
			if (!s.StartsWith ("http://") && !s.StartsWith ("https://") && !s.StartsWith ("ftp://") && !s.StartsWith ("file://")) {
				if (!System.IO.File.Exists (s))
					s = "http://" + s;
			}
			downloadState = downloadStateType.INPROGRESS;
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
				CI.configuration.uid = uid;
			else
				CI.configuration.uid = "";
			if (savePswd)
				CI.configuration.pswd = pswd;
			else
				CI.configuration.pswd = "";
			CI.configuration.Save ();
		}

		void getUidPswd ()
		{
			uid = CI.configuration.uid;
			if (uid != "" && uid != null)
				saveUid = true;
			else
				saveUid = false;
			pswd = CI.configuration.pswd;
			if (pswd != "" && pswd != null)
				savePswd = true;
			else
				savePswd = false;
		}

		System.Collections.IEnumerator doUploadCraft (string craftURL)
		{
			if (craftURL != "") {

				//if (craftURL.StartsWith ("file://"))
				if (System.IO.File.Exists (craftURL)) {
					//craftURL = craftURL.Substring (7);

					Log.Info ("craftURL: " + craftURL);
					string craft = System.IO.File.ReadAllText (craftURL);

					EditorLogic.LoadShipFromFile (craftURL);
					ShipConstruct ship = ShipConstruction.LoadShip (craftURL);

					Log.Info ("Craft name: " + ship.shipName);
					ThumbnailHelper.CaptureThumbnail (ship, 512, "tmp", ship.shipName);
					string thumbnailPath = KSPUtil.ApplicationRootPath.Substring (0, KSPUtil.ApplicationRootPath.Length - 12) + "/tmp";

					JSONNode jsonuid = uid;
					JSONNode jsonpswd = pswd;
					JSONNode jsoncraft = craft;

					Log.Info ("uid: " + uid + "   jsonuid: " + jsonuid);

					string url = uploadServer + "/crafts.json";

					WWWForm form = new WWWForm ();

					form.AddField ("username", jsonuid);
					form.AddField ("password", jsonpswd);
					form.AddField ("craft_file", jsoncraft);
					if (tags != "")
						form.AddField ("tags", tags);
					if (style != "")
						form.AddField ("style", style);
					if (pictureUrl != "")
						form.AddField ("picture_url", pictureUrl);					
					if (videoUrl != "")
						form.AddField ("video_url", videoUrl);
					if (forceNew)
						form.AddField ("force_new", "true");
					else {
						form.AddField ("force_new", "false");
						if (lastSelectedCraft >= 0)
							form.AddField ("update_existing", ids [lastSelectedCraft]);
					}

					string hash;
					for (int i = 0; i < 16; i++) {
						actionGroups [i] = actionGroups [i].Trim ();
						if (actionGroups [i] != "") {
							if (i < 10) {
								hash = (i + 1).ToString ();
							} else {
								hash = fixedActions [i - 10];
							}
							form.AddField (hash, actionGroups [i]);
							Log.Info ("Adding field: hash: " + hash + "      value: " + actionGroups [i]);
						}
					}

					var upload = new WWW (url, form);
					// Wait until the download is done
					yield   return upload;

					instructions = "";
				
					if (!String.IsNullOrEmpty (upload.error)) {
						Log.Error ("Error uploading: " + upload.error);
						uploadErrorMessage = upload.error;
						downloadState = downloadStateType.UPLOADERROR;
						yield break;
					} else {

						Log.Info ("upload.responseHeaders.Count: " + upload.responseHeaders.Count);
						if (upload.responseHeaders.Count > 0) {
							foreach (var entry in upload.responseHeaders) {
								Log.Info (entry.Key + "=" + entry.Value);
							}
						}
							
						downloadState = downloadStateType.UPLOADCOMPLETED;
						craftURL = "";
						//instructions = upload.text + "\n" + thumbnailPath;

						//if (upload.responseHeaders.Count > 0) {
						//	for (string entry in upload.responseHeaders) {
						//		Log.Info(entry.Value + "=" + entry.Key);
						var j = JSON.Parse (upload.text);

						if (j ["status"].AsInt == 200) {
							instructions = "The craft file has been imported.\n\nA browser window is being opened where you can edit the uploaded craft";
							string editUrl = uploadServer + j ["edit_url"];
							Application.OpenURL (editUrl);
						} else if (j ["status"].AsInt == 409) {
							downloadState = downloadStateType.SELECTDUPCRAFT;
							if (j ["existing_craft"].Count == 1)
								instructions = "There is already an existing craft by that name\n";
							else
								instructions = "There are " + j ["existing_craft"].Count.ToString () + " existing crafts by that name\n";

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
								instructions += "\n" + ids [i] + ": " + craftNames [i];
							}
						} else if (j ["status"].AsInt == 500) {
							instructions = "An unknown error occurred:\n\n";
							instructions += j ["error"];
							instructions += "\n\nPlease contact KerbalX support for assistance";
							downloadState = downloadStateType.UPLOADERROR;
						} else {
							instructions = "Unknown Error:" + j ["status"].AsInt;
							downloadState = downloadStateType.UPLOADERROR;
						}
					}
					upload.Dispose ();
					upload = null;
				} else {
					instructions = "Error";
					downloadState = downloadStateType.UPLOADERROR;
				}
			}
		}

		void uploadCraft (string craftURL)
		{
			saveUidPswd ();
			uploadProgress = 0;
			lastUpdate = Time.realtimeSinceStartup;
			downloadState = downloadStateType.UPLOADINPROGRESS;
			StartCoroutine (doUploadCraft (craftURL));
		}


		System.Collections.IEnumerator doGetUpdateCraftList ()
		{
			if (uid != "") {
				string url = uploadServer + "/" + uid + ".json";

				var upload = new WWW (url);
				// Wait until the download is done
				yield   return upload;

				var j = JSON.Parse (upload.text);
				instructions = "";

				if (!String.IsNullOrEmpty (upload.error)) {
					Log.Error ("Error uploading: " + upload.error);
					uploadErrorMessage = upload.error;
					downloadState = downloadStateType.UPLOADERROR;
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
							Log.Info("data: " + j["craft"][key].ToString());
							ids.Add (key);
							craftNames.Add (j["craft"][key].ToString());
							selectedCraft.Add (false);
						}
						downloadState = downloadStateType.SELECTDUPCRAFT;

					#if false
					} else if (j ["status"].AsInt == 500) {
						instructions = "An unknown error occurred:\n\n";
						instructions += j ["error"];
						instructions += "\n\nPlease contact KerbalX support for assistance";
						downloadState = downloadStateType.UPLOADERROR;
					} else {
						instructions = "Unknown Error:" + j ["status"].AsInt;
						downloadState = downloadStateType.UPLOADERROR;
		#endif
					}
					
				}

			} else
				downloadState = downloadStateType.UPLOAD;
		}

		void getUpdateCraftList ()
		{
			downloadState = downloadStateType.UPDATECRAFTLISTINPROGRESS;
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

		void getFile (string title, string suffix)
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
			m_fileBrowser.showDrives = CI.configuration.showDrives;
			m_fileBrowser.ShowNonmatchingFiles = false;

			if (m_textPath != "")
				m_fileBrowser.SetNewDirectory (m_textPath);

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
			downloadState = downloadStateType.GUI;

			m_textPath = path;
			if (m_textPath != "" && m_textPath != null) {
				if (selectionType == ".craft") {
					craftURL = "file://" + m_textPath;
					CI.configuration.lastImportDir = System.IO.Path.GetDirectoryName (m_textPath);
				}

			}
		}


		/////////////////////////////////////
		public void OnGUI ()
		{
			#if EXPORT
			Awake ();
			#endif
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
					Rect bounds2 = GUILayout.Window (GetInstanceID (), bounds, Window, CI.TITLE, HighLogic.Skin.window);
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
				//newShowDrives = CI.configuration.showDrives;
				//newCkanExecPath = CI.configuration.ckanExecPath;
				craftURL = "";
				saveInSandbox = false;
				overwriteExisting = false;
				m_textPath = CI.configuration.lastImportDir;
				#if EXPORT
				for (int i = 0; i < actionGroups.Length; i++)
					actionGroups [i] = "";
				lastSelectedCraft = -1;
				#endif
			} 

			SetVisible (true);
			GUI.enabled = true;

			GUILayout.BeginHorizontal ();
			GUILayout.EndHorizontal ();

			GUILayout.BeginVertical ();

			//	DrawTitle ("Craft Import");


			switch (downloadState) {
			#if EXPORT
			case downloadStateType.UPLOAD:

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
					GUILayout.Label (craftURL);
					GUILayout.EndHorizontal ();

					GUILayout.BeginHorizontal ();
					GUILayout.Label ("Tags:");
					GUILayout.FlexibleSpace ();
					tags = GUILayout.TextField (tags, GUILayout.MinWidth (50F), GUILayout.MaxWidth (300F));
					GUILayout.EndHorizontal ();

			
					//this draws the transparent catching button that stops items behind the listbox getting events - and does the actual selection of listitems
					styleItems.DrawBlockingSelector ();

					GUILayout.BeginHorizontal ();
					GUILayout.Label ("Style:");
					GUILayout.FlexibleSpace ();
					#if true
					//This draws the button and displays/hides the listbox - returns true if someone clicked the button
					if (styleItems.styleListSelectedItem == null)
						DrawCall ();
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
						GUILayout.Label ("Picture url (previously uploaded):");
						GUILayout.FlexibleSpace ();

						videoUrl = GUILayout.TextField (pictureUrl, GUILayout.Width (300F));
						GUILayout.EndHorizontal ();

						GUILayout.BeginHorizontal ();
						GUILayout.Label ("Video url (previously uploaded):");
						GUILayout.FlexibleSpace ();

						videoUrl = GUILayout.TextField (videoUrl, GUILayout.Width (300F));
						GUILayout.EndHorizontal ();

						GUILayout.BeginHorizontal ();
						GUILayout.FlexibleSpace ();
						if (GUILayout.Button ("Action Groups Descriptions", GUILayout.Width (250.0f))) {
							downloadState = downloadStateType.ACTIONGROUPS;
						}
						GUILayout.FlexibleSpace ();
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
								if (uid != "") {
									GUILayout.FlexibleSpace ();
									if (GUILayout.Button ("Select Existing Craft to Replace")) {
										downloadState = downloadStateType.GETDUPCRAFTLIST;
										//downloadState = downloadStateType.ACTIONGROUPS;
									}
								}
							}
						}


						GUILayout.EndHorizontal ();

						GUILayout.BeginHorizontal ();
						GUILayout.Label ("");
						GUILayout.EndHorizontal ();

						DrawTitle ("KerbalX Info");

						GUILayout.BeginHorizontal ();
						GUILayout.Label ("Enter userid:");

						#if true
						GUILayout.FlexibleSpace ();
						uid = GUILayout.TextField (uid, GUILayout.Width (150F));



						GUILayout.Label ("", GUILayout.Width (20F));
						GUILayout.Label ("Save userid:", GUILayout.Width (100F));
						saveUid = GUILayout.Toggle (saveUid, "");


						GUILayout.EndHorizontal ();


						GUILayout.BeginHorizontal ();
						GUILayout.Label ("Enter password:");
						GUILayout.FlexibleSpace ();

						pswd = GUILayout.PasswordField (pswd, '*', 25, GUILayout.Width (150F));
						GUILayout.Label ("", GUILayout.Width (20F));
						GUILayout.Label ("Save password:", GUILayout.Width (100F));

						if (!saveUid)
							savePswd = false;

						savePswd = GUILayout.Toggle (savePswd, "");
						GUILayout.EndHorizontal ();

						GUILayout.BeginHorizontal ();
						GUILayout.Label ("");
						GUILayout.EndHorizontal ();
					

						GUILayout.BeginHorizontal ();
						GUILayout.FlexibleSpace ();
						if (GUILayout.Button ("Upload to KerbalX", GUILayout.Width (125.0f))) {

							uploadCraft (craftURL);
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
						m_fileBrowser = null;
						craftURL = "";
						newCraftName = "";
						saveInSandbox = false;
						overwriteExisting = false;
						downloadState = downloadStateType.INACTIVE;
						GUIToggle ();
					}
					GUILayout.FlexibleSpace ();

					GUILayout.EndHorizontal ();
				}
				break;
			case downloadStateType.GETDUPCRAFTLIST:
				getUpdateCraftList ();

				break;

			case downloadStateType.SELECTDUPCRAFT:
				GUILayout.BeginHorizontal ();
				GUILayout.Label ("Select Craft to replace");
				GUILayout.FlexibleSpace ();
				GUILayout.EndHorizontal ();

				GUILayout.BeginHorizontal ();
				GUILayout.Label ("KerbalX id", GUILayout.Width (90F));
				GUILayout.FlexibleSpace ();
				GUILayout.Label ("Craft Name (clickable URL)");
				GUILayout.FlexibleSpace ();
				GUILayout.Label ("");
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
						Application.OpenURL (uploadServer + "/crafts/" + ids [i]);
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
					for (int i = 0; i < actionGroups.Length; i++)
						actionGroups [i] = "";
					downloadState = downloadStateType.UPLOAD;
				}
				GUILayout.FlexibleSpace ();

				GUILayout.EndHorizontal ();
				break;
			#endif
				
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
				GUILayout.Label (" ");
				GUILayout.EndHorizontal ();


				GUILayout.BeginHorizontal ();
				if (GUILayout.Button ("Select Local File", GUILayout.Width (125.0f))) {
					downloadState = downloadStateType.FILESELECTION;
					if (m_textPath == null)
						m_textPath = "";
					if (m_textPath.Contains (".craft"))
						m_textPath = System.IO.Path.GetDirectoryName (m_textPath);
					getFile ("Enter Craft File", ".craft");
				}
				GUILayout.EndHorizontal ();

				GUILayout.BeginHorizontal ();
				GUILayout.EndHorizontal ();

				GUILayout.BeginHorizontal ();
				if (GUILayout.Button ("Import", GUILayout.Width (125.0f))) {
					downloadCraft (craftURL);
				}
				#if EXPORT
				GUILayout.FlexibleSpace ();
				if (GUILayout.Button ("Upload to KerbalX", GUILayout.Width (125.0f))) {
					if (craftURL == "" || craftURL == null) {
						downloadState = downloadStateType.FILESELECTION;
						if (m_textPath == null)
							m_textPath = "";
						if (m_textPath.Contains (".craft"))
							m_textPath = System.IO.Path.GetDirectoryName (m_textPath);
						getFile ("Enter Craft File", ".craft");
					}

					downloadState = downloadStateType.UPLOAD;
					getUidPswd ();
				}

				#endif

				GUILayout.FlexibleSpace ();
				if (GUILayout.Button ("Cancel", GUILayout.Width (125.0f))) {
					m_fileBrowser = null;
					craftURL = "";
					newCraftName = "";
					saveInSandbox = false;
					overwriteExisting = false;
					downloadState = downloadStateType.INACTIVE;
					GUIToggle ();
				}

				GUILayout.EndHorizontal ();

				GUILayout.BeginHorizontal ();
				GUILayout.Label (" ");
				GUILayout.EndHorizontal ();

				GUILayout.BeginHorizontal ();
				DrawTitle ("Options");
				GUILayout.EndHorizontal ();
				#if false
				GUILayout.BeginHorizontal ();
				if (GUILayout.Button ("CKAN path", GUILayout.Width (125.0f))) {
					downloadState = downloadStateType.FILESELECTION;

					m_textPath = newCkanExecPath;
	
					// Linux change needed
					getFile ("Select CKAN Executable", ".exe");
				}

				GUILayout.FlexibleSpace ();
				newCkanExecPath = GUILayout.TextField (newCkanExecPath, GUILayout.MinWidth (200F), GUILayout.MaxWidth (300F));
				GUILayout.EndHorizontal ();
				#endif
				GUILayout.BeginHorizontal ();
				GUILayout.Label ("Use Blizzy Toolbar if available:");
				GUILayout.FlexibleSpace ();
				newUseBlizzyToolbar = GUILayout.Toggle (newUseBlizzyToolbar, "");
				GUILayout.EndHorizontal ();
				if (Application.platform == RuntimePlatform.WindowsPlayer) {
					GUILayout.BeginHorizontal ();
					GUILayout.Label ("Show drive letters in file selection dialog:");
					GUILayout.FlexibleSpace ();
					CI.configuration.showDrives = GUILayout.Toggle (CI.configuration.showDrives, "");
					GUILayout.EndHorizontal ();
				}

				break;

//			case downloadStateType.FILESELECTION:
//				getFile ();
//				break;
				#if EXPORT
			case downloadStateType.UPLOADINPROGRESS:
				#endif
			case downloadStateType.INPROGRESS:
				GUILayout.BeginHorizontal ();
				GUILayout.FlexibleSpace ();
				if (downloadState == downloadStateType.INPROGRESS)
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
				if (downloadState == downloadStateType.INPROGRESS)
					GUILayout.Label ("Progress: " + (100 * download.progress).ToString () + "%");
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
					download.Dispose ();
					download = null;
					SetVisible (false);
					GUI.enabled = false;
					downloadState = downloadStateType.INACTIVE;
				}
				GUILayout.FlexibleSpace ();
				GUILayout.EndHorizontal ();
				break;

				#if EXPORT
			case downloadStateType.UPLOADCOMPLETED:
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
				if (instructions.Trim () != "")
					GUILayout.TextArea (instructions);
				else
					GUILayout.Label ("");
				GUILayout.EndHorizontal ();
				GUILayout.BeginHorizontal ();
				GUILayout.Label ("");
				GUILayout.EndHorizontal ();
				GUILayout.BeginHorizontal ();
				GUILayout.FlexibleSpace ();
				if (GUILayout.Button ("OK", GUILayout.Width (125.0f))) {
					SetVisible (false);
					GUI.enabled = false;
					CIInfoDisplay.infoDisplayActive = false;
					downloadState = downloadStateType.INACTIVE;
				}
				GUILayout.FlexibleSpace ();
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
				#if EXPORT
			case downloadStateType.UPLOADERROR:
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
				if (downloadState == downloadStateType.UPLOADERROR)
					GUILayout.Label (uploadErrorMessage);
				else
				#endif
					GUILayout.Label (downloadErrorMessage);
				GUILayout.FlexibleSpace ();
				GUILayout.EndHorizontal ();
				GUILayout.BeginHorizontal ();
				if (GUILayout.Button ("OK", GUILayout.Width (125.0f))) {
					downloadState = downloadStateType.GUI;
				}
				GUILayout.FlexibleSpace ();
				if (GUILayout.Button ("Cancel", GUILayout.Width (125.0f))) {
					craftURL = "";
					newCraftName = "";
					saveInSandbox = false;
					overwriteExisting = false;
					SetVisible (false);
					GUI.enabled = false;
					CIInfoDisplay.infoDisplayActive = false;
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

		public void GUI_SaveData ()
		{
			CI.configuration.useBlizzyToolbar = newUseBlizzyToolbar;
			//CI.configuration.ckanExecPath = newCkanExecPath;
			//CI.configuration.showDrives = newShowDrives;
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