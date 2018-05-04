using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using System.Text;
using System.Net;
using System.Linq;
using SimpleJSON;
using KSP.UI.Screens;
using KSP.Localization;

using ToolbarControl_NS;
using ClickThroughFix;

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
        internal static  MainMenuGui Instance;
        //		internal override void DrawWindow(int id)
        //		{
        //		}
        private const int WIDTH = 500;
        private const int HEIGHT = 400;
        private const int BR_WIDTH = 600;
        private const int BR_HEIGHT = 500;

        private Rect bounds = new Rect(Screen.width / 2 - WIDTH / 2, Screen.height / 2 - HEIGHT / 2, WIDTH, HEIGHT);
        private /* volatile*/ bool visible = false;
        bool resetWinPos = false;
        // Stock APP Toolbar - Stavell
        //public /*static*/ ApplicationLauncherButton CI_Button = null;
        public bool stockToolBarcreated = false;


        internal ToolbarControl toolbarControl = null;


        public static Texture2D CI_button_img = new Texture2D(38, 38, TextureFormat.ARGB32, false);

        //private bool CI_Texture_Load = false;

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
        private static bool loadAsSubassembly = false;
        private static bool appLaucherHidden = true;
        private static string craftURL = "";
        private static string newCraftName = "";
        private static bool subassembly = false;
        bool jpgToDelete = false;
        string convertedJpg = "";
        //private static string newCkanExecPath = "";

        string instructions = "";


        enum downloadStateType
        {
            INACTIVE,
            GUI,
            FILESELECTION,
            IN_PROGRESS,
            COMPLETED,
            FILE_EXISTS,
            ERROR
        };

        private static downloadStateType downloadState = downloadStateType.INACTIVE;
        private static string downloadErrorMessage;

        internal MainMenuGui()
        {
            Instance = this;
            blizzyToolbarInstalled = ToolbarManager.ToolbarAvailable;
        }

        internal const string MODID = "craftImport_NS";
        internal const string MODNAME = "CraftImport";

        public void UpdateToolbarStock()
        {
            Log.Info("UpdateToolbarStock, appLaucherHidden: " + appLaucherHidden.ToString());

            if (toolbarControl == null)
            {
                toolbarControl = gameObject.AddComponent<ToolbarControl>();
                toolbarControl.AddToAllToolbars(GUIToggle, GUIToggle,
                   ApplicationLauncher.AppScenes.SPH | ApplicationLauncher.AppScenes.VAB,
                    MODID,
                    "craftImportButton",
                    CI.TEXTURE_DIR + "CI-38",
                    CI.TEXTURE_DIR + "CI-24",
                    MODNAME
                );

            }
        }

        public bool Visible()
        {
            return this.visible;
        }

        public void SetVisible(bool visible)
        {
            this.visible = visible;
        }

        void resetBeforeExit()
        {
            Log.Info("resetBeforeExit");
            if (download != null)
                download.Dispose();
            if (jpgToDelete && convertedJpg != "")
            {
                System.IO.FileInfo file = new System.IO.FileInfo(convertedJpg);
                file.Delete();
            }

            download = null;
            SetVisible(false);
            GUI.enabled = false;
            downloadState = downloadStateType.INACTIVE;
            RemoveInputLock();

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

        }

        string saveFile;

        bool SaveCraftFile(string craftFile, bool subassembly = false)
        {
            Log.Info ("SaveCraftFile: " + craftFile);
            string saveDir = "";
            if (saveInSandbox)
            {
                saveDir = FileOperations.ROOT_PATH + "Ships";
            }
            else
            {
                saveDir = FileOperations.ROOT_PATH + "saves/" + HighLogic.SaveFolder + "/Ships";
            }
            //string saveDir = KSPUtil.ApplicationRootPath + sandbox + HighLogic.SaveFolder + "/ships";
            saveFile = "";

            string shipName = null;
            Match t = null;
            try
            {

                var s = Regex.Match(craftFile, "^ship.*=.*");
                s = Regex.Match(s.ToString(), "=.*");
                string s1 = s.ToString().Remove(0, 1);
                if (s1.IndexOf("//") >= 0)
                {
                    s1 = s1.Substring(0, s1.IndexOf("//"));
                }
                shipName = Localizer.Format( s1.Trim());


                t = Regex.Match(craftFile, "type.*=.*");
                Log.Info("after regex type   t: " + t.ToString());
                string strCraftFile = craftFile.ToString();
                if (newCraftName != "")
                {
                    strCraftFile = strCraftFile.Replace(shipName, newCraftName);
                    shipName = newCraftName;
                }
                Log.Info("shipName: " + shipName);
                if (!subassembly)
                {
                    //Log.Info ("regex: " + t.ToString ());
                    bool vab = t.ToString().Contains("VAB");
                    bool sph = t.ToString().Contains("SPH");
                    if (!saveInShipDefault)
                    {
                        if (saveInVAB)
                        {
                            vab = true;
                            sph = false;
                        }
                        if (saveInSPH)
                        {
                            vab = false;
                            sph = true;
                        }
                    }


                    if (vab)
                    {
                        saveFile = saveDir + "/VAB/" + shipName + ".craft";
                    }
                    if (sph)
                    {
                        saveFile = saveDir + "/SPH/" + shipName + ".craft";
                    }
                }
                else
                {
                    if (!Directory.Exists(FileOperations.ROOT_PATH + "saves/" + HighLogic.SaveFolder + "/Subassemblies"))
                        Directory.CreateDirectory(FileOperations.ROOT_PATH + "saves/" + HighLogic.SaveFolder + "/Subassemblies");
                    saveFile = FileOperations.ROOT_PATH + "saves/" + HighLogic.SaveFolder + "/Subassemblies/" + shipName + ".craft";
                    Log.Info("subassembly saveDir: " + saveFile);
                }

                if (System.IO.File.Exists(saveFile) && !overwriteExisting)
                {
                    downloadState = downloadStateType.FILE_EXISTS;
                    return false;
                }

                System.IO.File.WriteAllText(saveFile, strCraftFile);

                downloadState = downloadStateType.COMPLETED;
            }
            catch (Exception e)
            {
                Log.Info("Error: " + e);

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
        bool checkMissingParts(string craft)
        {
            Log.Info("checkMissingParts");
            SortedList<string, string> missingPartsList = new SortedList<string, string>();

            Log.Info("checkMissingParts");
            missingParts = false;
            string partRegex = ".*part.*=.*_[0-9]*";
            Regex detectPartLine = new Regex(partRegex);
            var matches = detectPartLine.Match(craft);
            int underscoreIndex, equalsIndex;
            string partName;
            while (matches.Success)
            {
                equalsIndex = matches.Value.IndexOf("=") + 1;
                underscoreIndex = matches.Value.LastIndexOf("_");
                partName = matches.Value.Substring(equalsIndex, underscoreIndex - equalsIndex);
                partName = partName.Trim();

                if (PartLoader.getPartInfoByName(partName) == null)
                {
                    missingParts = true;
                    if (!missingPartsList.ContainsValue(partName))
                        missingPartsList.Add(partName, partName);
                }

                matches = matches.NextMatch();
            }
            if (missingParts)
            {
                loadAfterImport = false;
                instructions += "This craft file requires the following parts which are not available in the current install:\r\n\r\n";
                foreach (KeyValuePair<string, string> pair in missingPartsList)
                    instructions += "      " + pair.Value + "\r\n";
                Log.Info("instructions: " + instructions);
                return true;
            }
            return false;
        }

        WWW download = null;
        WWW craftDownload = null;
        bool kerbalx = false;

        System.Collections.IEnumerator doDownloadCraft(string craft)
        {

            //craftURL.Replace('\\', '/');
            string craftURL = "";
            for (int i = 0; i < craft.Length; i++)
            {
                if (craft[i] == '\\')
                    craftURL += "/";
                else
                    craftURL += craft[i];
            }
           
            Log.Info("doDownloadCraft 2, craftURL: " + craftURL);
            if (craftURL != "")
            {

                string s = System.Uri.EscapeUriString(craftURL);

                // some simple error checking
                if (!s.StartsWith("http://") && !s.StartsWith("https://") && !s.StartsWith("ftp://") && !s.StartsWith("file://"))
                {
                    downloadErrorMessage = "Invalid URL or file specified";
                    downloadState = downloadStateType.ERROR;
                    yield break;
                }
                kerbalx = (craftURL.IndexOf("kerbalx.com", StringComparison.OrdinalIgnoreCase) >= 0);
                if (kerbalx)
                {
                    downloadState = downloadStateType.COMPLETED;
                    instructions = "Access to KerbalX is currently disabled due to a site problem.\n\n";
                    instructions += "You can download the craft file using a browser, and then import using this mod.\n\n";
                    instructions += "The mod will be updated again once the security issues with the site have been fixed";
                    yield break;

                }
                // Create a download object
                Log.Info("s: " + s);
                Log.Info("kerbalx: " + kerbalx.ToString());
                string fileText = "";
                if (craftURL.StartsWith("file://"))
                {
                    if (File.Exists(craftURL.Substring(7)))
                    {
                        fileText = File.ReadAllText(craftURL.Substring(7));

                        subassembly = loadAsSubassembly;
                        bool b = SaveCraftFile(fileText, subassembly);
                        checkMissingParts(craftDownload.text);
                        if (loadAfterImport)
                        {
                            if ((EditorLogic.RootPart && subassembly) || !subassembly)
                                instructions += "\r\nThe imported craft file will be loaded when you click the OK button";
                        }
                    }
                }
                else
                {
                    craftDownload = new WWW(s);
                    // Wait until the download is done
                    yield return craftDownload;

                    Log.Info("Download completed   craftDownload.error: " + craftDownload.error);

                    if (!String.IsNullOrEmpty(craftDownload.error))
                    {
                        Log.Error("Error downloading: " + craftDownload.error);
                        downloadErrorMessage = craftDownload.error;
                        downloadState = downloadStateType.ERROR;
                        yield break;
                    }
                    else
                    {
                        JSONNode j = null;
                        subassembly = false;
                        if (kerbalx)
                        {
                            bool https = (craftURL.IndexOf("https://kerbalx.com", StringComparison.OrdinalIgnoreCase) == 0);
                            int dot = craftURL.IndexOf(".craft");
                            if (dot > 0)
                            {
                                craftURL = craftURL.Substring(0, dot);
                            }
                            Log.Info("craftURL on kerbalx: " + craftURL);
                            s = System.Uri.EscapeUriString(craftURL + ".json");
                            var json = new WWW(s);
                            yield return json;
                            j = JSON.Parse(json.text);
                            if (j["subassembly"].AsBool == true)
                                subassembly = true;
                        }
                        bool b = SaveCraftFile(craftDownload.text, subassembly);

                        Log.Info("After saveCraftFile, b: " + b.ToString());
                        saveInSandbox = false;
                        overwriteExisting = false;

                        if (b)
                        {


                            //downloadState = downloadStateType.COMPLETED;
                            if (kerbalx)
                            {
                                int i = j["mods"].Count;
                                string m;
                                //for (int i1 = 0; i1 < i; i1++) {
                                //	m = j ["mods"] [i1];
                                //	Log.Info ("Mod: " + m);
                                //}

                                s = System.Uri.EscapeUriString(craftURL + ".ckan");
                                Log.Info("ckan: " + s);
                                //download.Dispose ();
                                download = new WWW(s);
                                // Wait until the download is done
                                yield return download;

                                // 1. Need to get current ksp directory for ckan, root path ends in a slash
                                //string root = KSPUtil.ApplicationRootPath.Substring (0, KSPUtil.ApplicationRootPath.Length - 12);
                                Log.Info("Root path: " + FileOperations.ROOT_PATH);
                                string ckanDir = FileOperations.ROOT_PATH + "CKAN";
                                Log.Info("ckanDir: " + ckanDir);
                                if (System.IO.Directory.Exists(ckanDir))
                                {
                                    string ckanFile = j["name"] + ".ckan";
                                    Log.Info("ckanFile: " + ckanFile);
                                    string ckanFilePath = ckanDir + "/" + ckanFile;
                                    System.IO.File.WriteAllText(ckanFilePath, download.text);

                                    Log.Info("installedCkan: " + ckanDir + "/installed-default.ckan");
                                    string ckanInstallDefault = System.IO.File.ReadAllText(ckanDir + "/installed-default.ckan");
                                    var installedCkanMods = JSON.Parse(ckanInstallDefault);

                                    bool missing, anymissing = false;
                                    instructions = "";
                                    if (j["mods"].Count > 0)
                                    {

                                        // The following is very inefficient
                                        for (int i1 = 0; i1 < j["mods"].Count; i1++)
                                        {
                                            missing = true;
                                            for (int ckan = 0; ckan < installedCkanMods["depends"].Count; ckan++)
                                            {
                                                if (j["mods"][i1].ToString() == installedCkanMods["depends"][ckan]["name"].ToString())
                                                {
                                                    missing = false;
                                                    break;
                                                }
                                            }
                                            if (missing)
                                            {
                                                if (!anymissing)
                                                    instructions += "You are missing the following mods:\r\n\r\n";
                                                anymissing = true;
                                                loadAfterImport = false;
                                                instructions += "      " + j["mods"][i1] + "\r\n";

                                            }
                                        }
                                    }

                                    if (anymissing)
                                    {
                                        instructions += "\r\nYou will need to load the missing mods before you can load the imported craft:\r\n" + j["name"] + ".";
                                        instructions += "  A .ckan file has been saved in the CKAN directory for the current game:\r\n\r\n" +
                                            ckanFilePath + "\n\nYou can use CKAN to load all missing mods by following these instructions:\r\n\r\n" +
                                            "1.  Start CKAN\r\n2.  Select File->Import from .ckan\r\n3.  Navigate to the folder:      " + ckanDir +
                                            "\r\n4.  Select the file:      " + ckanFile + "\r\n";
                                    }
                                    else
                                    {
                                        checkMissingParts(craftDownload.text);
                                    }
                                }
                                else
                                {
                                    // CKAN not installed
                                    if (j["mods"].Count > 0)
                                    {
                                        instructions = "If you can't load the imported craft: " + j["name"] + ", then you will have to load the necessary mods by hand." +
                                        "\r\n\r\nYou don't seem to have CKAN installed in this game, so you will have to install them manually.\r\n\r\n" +
                                        "Here is the list of mods that this craft file needs:\r\n\r\n";
                                        for (int i1 = 0; i1 < j["mods"].Count; i1++)
                                        {
                                            m = j["mods"][i1];
                                            instructions += "     " + m + "\r\n";
                                        }
                                        instructions += "\r\n";
                                    }
                                    checkMissingParts(craftDownload.text);
                                    // instructions = "You should be able to load the craft in either the VAB or SPH";

                                }


                                if (loadAfterImport)
                                {
                                    if ((EditorLogic.RootPart && subassembly) || !subassembly)
                                        instructions += "\r\nThe imported craft file will be loaded when you click the OK button";
                                }
                            }
                            else
                            {
                                checkMissingParts(craftDownload.text);
                            }
                        }
                        craftURL = "";
                    }
                }
                if (download != null)
                {
                    download.Dispose();
                    download = null;
                }
            }
        }

        void downloadCraft(string s)
        {
            if (!s.StartsWith("http://") && !s.StartsWith("https://") && !s.StartsWith("ftp://") && !s.StartsWith("file://"))
            {
                if (!System.IO.File.Exists(s))
                    s = "http://" + s;
            }
            downloadState = downloadStateType.IN_PROGRESS;
            StartCoroutine(doDownloadCraft(s));
        }

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

        void getFile(string title, string suffix, string dir = "")
        {
            fileBrowserEnabled = true;
            selectionType = suffix;

            m_fileBrowser = new FileBrowser(
                new Rect(Screen.width / 2 - BR_WIDTH / 2, Screen.height / 2 - BR_HEIGHT / 2, BR_WIDTH, BR_HEIGHT),
                title,
                FileSelectedCallback
            );

            if (!m_directoryImage)
            {
                m_directoryImage = new Texture2D(38, 38, TextureFormat.ARGB32, false);
                if (GameDatabase.Instance.ExistsTexture(CI.TEXTURE_DIR + "CI-folder"))
                    m_directoryImage = GameDatabase.Instance.GetTexture(CI.TEXTURE_DIR + "CI-folder", false);
            }
            if (!m_fileImage)
            {
                m_fileImage = new Texture2D(38, 38, TextureFormat.ARGB32, false);
                if (GameDatabase.Instance.ExistsTexture(CI.TEXTURE_DIR + "CI-file"))
                    m_fileImage = GameDatabase.Instance.GetTexture(CI.TEXTURE_DIR + "CI-file", false);
            }
            // Linux change may needed here
            m_fileBrowser.SelectionPattern = "*" + suffix;
            m_fileBrowser.DirectoryImage = m_directoryImage;
            m_fileBrowser.FileImage = m_fileImage;
            m_fileBrowser.showDrives = thisCI.configuration.showDrives;
            m_fileBrowser.ShowNonmatchingFiles = false;


            if (dir != "")
            {
                m_fileBrowser.SetNewDirectory(dir);
            }
            else
            {
                if (m_textPath != "")
                    m_fileBrowser.SetNewDirectory(m_textPath);
            }

        }

        protected void FileSelectedCallback(string path)
        {
            m_fileBrowser = null;
            fileBrowserEnabled = false;


            m_textPath = path;
            if (m_textPath != "" && m_textPath != null)
            {
                if (selectionType == ".craft")
                {

                    downloadState = downloadStateType.GUI;
                    craftURL = "file://" + m_textPath;
                    thisCI.configuration.lastImportDir = System.IO.Path.GetDirectoryName(m_textPath);
                    return;
                }
            }
            if (m_textPath != "" && m_textPath != null)
            {
                if (selectionType == ".png|.jpg")
                {

                    //CI.configuration.lastImportDir = System.IO.Path.GetDirectoryName (m_textPath);
                    return;
                }
            }
            //Log.Info ("downloadState: " + downloadState.ToString ());

            downloadState = downloadStateType.GUI;
        }



        internal void RemoveInputLock()
        {
            Log.Info("RemoveInputLock");
            if (InputLockManager.GetControlLock("CraftImportLock") != ControlTypes.None)
            {
                //LogFormatted_DebugOnly("Removing-{0}", "TWPControlLock");
                InputLockManager.RemoveControlLock("CraftImportLock");
            }
            Input.ResetInputAxes();
            //InputLockExists = false;
        }

        /////////////////////////////////////
        public void OnGUI()
        {

            GUI.skin = HighLogic.Skin;

            //	InputLockManager.SetControlLock((ControlTypes.EDITOR_LOCK | ControlTypes.EDITOR_GIZMO_TOOLS), "CraftImportLock");


            if (m_fileBrowser != null)
            {
                if (!fileBrowserEnabled)
                {

                    m_fileBrowser = null;
                    downloadState = downloadStateType.GUI;

                    //this one closes the dropdown if you click outside the window elsewhere
                    //	styleItems.CloseOnOutsideClick();

                }
                else
                {

                    //	GUI.skin = HighLogic.Skin;
#if true
                    GUIStyle s = new GUIStyle(HighLogic.Skin.textField);

                    s.onNormal = HighLogic.Skin.textField.onNormal;

                    //			s.fontSize = 15;
                    s.name = "listitem";
                    s.alignment = TextAnchor.MiddleLeft;
                    //s.fontStyle = FontStyle.Bold;
                    //s.fixedHeight = 50;
                    s.imagePosition = ImagePosition.ImageLeft;
                    GUI.skin.customStyles[0] = s;
#endif
                    m_fileBrowser.Window(GetInstanceID());
                    return;
                }
            }


            try
            {
                if (this.Visible())
                {
                    //					Rect bounds2 = ClickThruBlocker.GUILayoutWindow (GetInstanceID (), bounds, Window, CI.TITLE, HighLogic.Skin.window);
                    if (resetWinPos)
                    {
                        bounds = new Rect(Screen.width - WIDTH, Screen.height / 2 - HEIGHT / 2, WIDTH, HEIGHT);
                        resetWinPos = false;
                    }
                    bounds = ClickThruBlocker.GUILayoutWindow(GetInstanceID(), bounds, Window, CI.TITLE, HighLogic.Skin.window);

                }
            }
            catch (Exception e)
            {
                Log.Error("exception: " + e.Message);
            }
        }


        void guiCase()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Enter Craft URL:", GUILayout.MinWidth(150F));

            GUILayout.FlexibleSpace();

            craftURL = GUILayout.TextField(craftURL, GUILayout.MinWidth(50F), GUILayout.MaxWidth(300F));
            craftURL = craftURL.Replace("\n", "").Replace("\r", "");
            if (craftURL.IndexOf("%") >= 0)
            {
                craftURL = Uri.UnescapeDataString(craftURL);
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Rename craft to:", GUILayout.MinWidth(150F));
            GUILayout.FlexibleSpace();
            //GUILayout.EndHorizontal ();
            //GUILayout.BeginHorizontal ();
            //GUILayout.FlexibleSpace ();
            newCraftName = GUILayout.TextField(newCraftName, GUILayout.MinWidth(50F), GUILayout.MaxWidth(300F));
            newCraftName = newCraftName.Replace("\n", "").Replace("\r", "");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            var styleLabel = new GUIStyle(GUI.skin.label);
            Color lightBlue = Color.blue;
            styleLabel.normal.textColor = Color.blue;
            lightBlue.r = 30;
            lightBlue.g = 144;
            lightBlue.b = 255;
            //styleLabel.normal.textColor = lightBlue;
            GUILayout.Label("Save Location", styleLabel);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Sandbox:", GUILayout.Width(60F));
            saveInSandbox = GUILayout.Toggle(saveInSandbox, "");
            GUILayout.FlexibleSpace();

            GUILayout.Label("Original:", GUILayout.Width(60F));
            saveInShipDefault = GUILayout.Toggle(saveInShipDefault, "");
            if (saveInShipDefault)
            {
                saveInVAB = false;
                saveInSPH = false;
            }
            GUILayout.FlexibleSpace();

            GUILayout.Label("VAB:", GUILayout.Width(35F));
            saveInVAB = GUILayout.Toggle(saveInVAB, "");
            if (saveInVAB)
            {
                saveInShipDefault = false;
                saveInSPH = false;
            }
            GUILayout.FlexibleSpace();
            GUILayout.Label("SPH:", GUILayout.Width(35F));
            saveInSPH = GUILayout.Toggle(saveInSPH, "");
            if (saveInSPH)
            {
                saveInShipDefault = false;
                saveInVAB = false;
            }
            GUILayout.FlexibleSpace();

            GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal();
            GUILayout.Label("Overwrite existing file if exists:", GUILayout.Width(300F));
            //GUILayout.FlexibleSpace ();
            overwriteExisting = GUILayout.Toggle(overwriteExisting, "");
            GUILayout.FlexibleSpace();

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Load as Subassembly:", GUILayout.Width(200F));;
            loadAsSubassembly = GUILayout.Toggle(loadAsSubassembly, "");
            GUILayout.FlexibleSpace();

            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (loadAsSubassembly)
            {
                loadAfterImport = false;
                GUI.enabled = false;
            }
            GUILayout.Label("Load craft after import:", GUILayout.Width(200F)); ;
            loadAfterImport = GUILayout.Toggle(loadAfterImport, "");
            GUILayout.FlexibleSpace();

            GUILayout.EndHorizontal();
            GUI.enabled = true;


            GUILayout.BeginHorizontal();
            GUILayout.Label(" ");
            GUILayout.EndHorizontal();


            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Select Local File", GUILayout.Width(125.0f)))
            {
                downloadState = downloadStateType.FILESELECTION;
                if (m_textPath == null)
                {
                    m_textPath = FileOperations.ROOT_PATH + "saves/" + HighLogic.SaveFolder + "/Ships";
                    if (EditorDriver.editorFacility == EditorFacility.VAB)
                        m_textPath += "/VAB";
                    if (EditorDriver.editorFacility == EditorFacility.SPH)
                        m_textPath += "/SPH";

                }
                Log.Info("m_textPath: " + m_textPath);
                if (m_textPath.Contains(".craft"))
                    m_textPath = System.IO.Path.GetDirectoryName(m_textPath);
                getFile("Enter Craft File", ".craft");
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Import", GUILayout.Width(125.0f), GUILayout.Height(40)))
            {
                if (craftURL != "")
                    downloadCraft(craftURL);
            }


            GUILayout.FlexibleSpace();


            GUILayout.Label(" ");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            DrawTitle("Options", GUILayout.Width(75f));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Use Blizzy Toolbar if available:", GUILayout.Width(300F));
            //	GUILayout.FlexibleSpace ();
            newUseBlizzyToolbar = GUILayout.Toggle(newUseBlizzyToolbar, "");
            GUILayout.EndHorizontal();
            if (Application.platform == RuntimePlatform.WindowsPlayer)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Show drive letters in file selection dialog:", GUILayout.Width(300F));
                //GUILayout.FlexibleSpace ();
                thisCI.configuration.showDrives = GUILayout.Toggle(thisCI.configuration.showDrives, "");
                GUILayout.EndHorizontal();
            }
        }

        //int colorPickerImageWidth = 150;
        //int colorPickerImageHeight = 150;
        Texture2D colorPicker;
        Vector2 scrollPosition;

        private void Window(int id)
        {
            if (cfgWinData == false)
            {
                cfgWinData = true;

                //newShowDrives = CI.configuration.showDrives;
                //newCkanExecPath = CI.configuration.ckanExecPath;
                craftURL = "";
                m_textPath = "";
                saveInSandbox = false;
                overwriteExisting = false;
                m_textPath = thisCI.configuration.lastImportDir;

                byte[] colorPickerFileData = System.IO.File.ReadAllBytes(FileOperations.ROOT_PATH + "GameData/" + CI.TEXTURE_DIR + "/colorpicker_texture.jpg");
                colorPicker = new Texture2D(2, 2);
                colorPicker.LoadImage(colorPickerFileData); //..this will auto-resize the texture dimensions.

            }

            SetVisible(true);
            GUI.enabled = true;

            GUILayout.BeginHorizontal();
            GUILayout.EndHorizontal();

            GUILayout.BeginVertical();

            //	DrawTitle ("Craft Import");

            //Log.Info ("downloadState: " + downloadState.ToString ());
            switch (downloadState)
            {


                case downloadStateType.GUI:
                    guiCase();
                    break;

                case downloadStateType.IN_PROGRESS:
                    Log.Info("IN_PROGRESS");
                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    if (downloadState == downloadStateType.IN_PROGRESS)
                        GUILayout.Label("Download in progress");
                    else
                        GUILayout.Label("Upload in progress");
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("");
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    if (downloadState == downloadStateType.IN_PROGRESS)
                    {
                        if (download != null)
                            GUILayout.Label("Progress: " + (100 * download.progress).ToString() + "%");
                    }

                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("");
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Cancel", GUILayout.Width(125.0f)))
                    {
                        resetBeforeExit();
                    }
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                    break;

                case downloadStateType.COMPLETED:
                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    if (downloadState == downloadStateType.COMPLETED)
                        GUILayout.Label("Download completed");
                    else
                        GUILayout.Label("Upload completed");

                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("", GUILayout.Height(10));
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    if (instructions.Trim() != "")
                    {
                        scrollPosition = GUILayout.BeginScrollView(
                            scrollPosition, GUILayout.Width(WIDTH - 20), GUILayout.Height(HEIGHT - 100));
                        GUILayout.Label(instructions);
                        GUILayout.EndScrollView();
                    }
                    else
                        GUILayout.Label("");
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("", GUILayout.Height(10));
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("OK", GUILayout.Width(125.0f)))
                    {

                        if (downloadState == downloadStateType.COMPLETED && loadAfterImport)
                        {
                            Log.Info("saveFile: " + saveFile);
                            if (!subassembly)
                                EditorLogic.LoadShipFromFile(saveFile);
                            else
                            {
                                if (EditorLogic.RootPart)
                                    LoadCraftAsSubassembly(saveFile);
                                //	ShipConstruct ship = ShipConstruction.LoadShip (craftURL);
                                //	EditorLogic.SpawnConstruct (ship);
                            }

                        }
                        resetBeforeExit();
                    }
                    GUILayout.FlexibleSpace();
                    if (instructions != "")
                    {
                        if (GUILayout.Button("Copy to clipboard"))
                        {
                            TextEditor te = new TextEditor();
                            te.text = instructions;
                            te.SelectAll();
                            te.Copy();
                        }
                        GUILayout.FlexibleSpace();
                    }
                    GUILayout.EndHorizontal();
                    break;

                case downloadStateType.FILE_EXISTS:
                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    GUILayout.Label("File Exists Error");
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    GUILayout.Label("The craft file already exists, will NOT be overwritten");
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("OK", GUILayout.Width(125.0f)))
                    {
                        resetBeforeExit();
                    }
                    GUILayout.EndHorizontal();
                    break;

                case downloadStateType.ERROR:
                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    if (downloadState == downloadStateType.ERROR)
                        GUILayout.Label("Download Error");
                    else
                        GUILayout.Label("Upload Error");
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();

                    GUILayout.Label(downloadErrorMessage);
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Cancel", GUILayout.Width(125.0f)))
                    {
                        resetBeforeExit();
                    }
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                    break;

                default:
                    break;
            }
            GUILayout.EndVertical();
            GUI.DragWindow();
        }

        private void DrawTitle(String text)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(text, HighLogic.Skin.label);
            //GUILayout.FlexibleSpace ();
            GUILayout.EndHorizontal();
        }
        private void DrawTitle(String text, GUILayoutOption layout)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(text, HighLogic.Skin.label, layout);
            //GUILayout.FlexibleSpace ();
            GUILayout.EndHorizontal();
        }
        public void GUI_SaveData()
        {
            //CI.configuration.ckanExecPath = newCkanExecPath;
            //CI.configuration.showDrives = newShowDrives;
            //CI.configuration.lastImportDir = m_textPath;
        }

        public void initGUIToggle()
        {
            Log.Info("initGUIToggle");
            SetVisible(true);
            downloadState = downloadStateType.GUI;
            craftURL = "";
            m_textPath = "";
            newCraftName = "";
            saveInSandbox = false;
            overwriteExisting = false;

            Log.Info("Setting screen lock");
            InputLockManager.SetControlLock((ControlTypes.EDITOR_LOCK | ControlTypes.EDITOR_GIZMO_TOOLS), "CraftImportLock");
        }

        public void endGUIToggle()
        {
            Log.Info("endGUIToggle");
            SetVisible(false);
            //set_CI_Button_active();
            cfgWinData = false;
            RemoveInputLock();
            GUI_SaveData();

            thisCI.configuration.Save();

        }
        public void GUIToggle()
        {
            Log.Info("GUIToggle");
            downloadState = downloadStateType.GUI;
            CIInfoDisplay.infoDisplayActive = !CIInfoDisplay.infoDisplayActive;
            if (CIInfoDisplay.infoDisplayActive)
            {
                initGUIToggle();
                //CI_Button.SetTexture(CI_button_img);

            }
            else
            {
                endGUIToggle();
            }
        }
    }
}
