using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Reflection;
using Multimorphic.P3App.GUI;
using Multimorphic.P3App.GUI.Selector;

public class SelectorCreator {

	[UnityEditor.MenuItem ("Multimorphic/Create new selector")]
	static void CreateNewSelector() {
		NewSelectorDialog newSelectorDialog = ScriptableObject.CreateInstance<NewSelectorDialog>();
		newSelectorDialog.Initialize ();
		newSelectorDialog.Show();
	}
}


public class NewSelectorDialog : EditorWindow {
	
	public string selectorName = "MySelector";
	public string allUpperSelectorName;
	public string upperSelectorName;
	public string lowerSelectorName;
	public string dialogName = "MyDialog";
	public int visibleCount = 3;
	public int rowCount = 3;
	public int columnCount = 1;
	public float rowSpacing = 1f;
	public float columnSpacing = 1f;
	public bool isGrid = true;
	private string appCode;
	private string companyName;
	private string priority = "Priorities.PRIORITY_SELECTOR_BASE + 123";
	private bool createHighlight;
	private bool createSubfolderItem;
	private bool dialogExists = false;
	double compileGracePeriodStart;
	private const double COMPILER_GRACE_PERIOD = 2f;  // Wait at least this many seconds after the build has complete for post-build in-editor script compilation to start and finish

	private List<string> results = new List<string>();

	private int state;
	private const int ENTRY = 0;
	private const int CREATING_SCRIPTS = 1;
	private const int WAITING = 2;
	private const int CREATING_PREFABS = 3;
	private const int DONE = 4;

	public void Initialize() {
		appCode = AppCodeFromAssets ();
		companyName = CompanyFromAssets ();
		this.minSize = new Vector2 (650, 500);
	}
	
	void OnGUI() {

		if (state == ENTRY) {
			// GUILayout.Label ("Current app code: " + appCode);
			EditorGUIUtility.labelWidth = 300;
			EditorGUILayout.LabelField("");
			selectorName = EditorGUILayout.TextField("Create a new selector named", selectorName);
			dialogName = EditorGUILayout.TextField("to be part of a dialog named", dialogName);
			priority = EditorGUILayout.TextField("with an associated mode with priority", priority);
			EditorGUILayout.Space();
			createSubfolderItem = EditorGUILayout.Toggle ("Are subfolders needed (i.e. a tree view)?", createSubfolderItem);
			createHighlight = EditorGUILayout.Toggle ("Create an independent highlight/cursor?", createHighlight);
			EditorGUILayout.Space();
			visibleCount = EditorGUILayout.IntField ("How many items will be visible at once?", visibleCount);
			EditorGUILayout.Space();
			isGrid = EditorGUILayout.BeginToggleGroup ("Grid shaped layout?", isGrid);
			if (!isGrid)
				EditorGUILayout.LabelField ("(item nodes will be created with the selector for custom layout)");
			else {
				//	EditorGUILayout.Space();
				rowCount = EditorGUILayout.IntField ("How many rows?", rowCount);
				columnCount = EditorGUILayout.IntField ("How many columns?", columnCount);
				rowSpacing = EditorGUILayout.FloatField ("Row spacing", rowSpacing);
				columnSpacing = EditorGUILayout.FloatField ("Column spacing", columnSpacing);
			}
			EditorGUILayout.EndToggleGroup ();

			EditorGUILayout.Space();
			if (GUILayout.Button ("Continue")) {
				state = CREATING_SCRIPTS;
				upperSelectorName = selectorName.First ().ToString ().ToUpper () + selectorName.Substring (1);
				lowerSelectorName = selectorName.First ().ToString ().ToLower () + selectorName.Substring (1);
				allUpperSelectorName = selectorName.ToUpper();
				GUIUtility.ExitGUI ();
			}
		
			if (GUILayout.Button ("Cancel")) {
				Close ();
				GUIUtility.ExitGUI ();
			}
		} 
		else if (state == CREATING_SCRIPTS) {
			GUILayout.Box ("Creating scripts...");
			WriteSelectorScript ();
			WriteSelectorItemScript ();
			if (createSubfolderItem)
				WriteSelectorSubfolderItemScript ();
			WriteModeScript ();
			AddPriority ();
			RewriteRegistrationMethod ();
			state = WAITING;
			compileGracePeriodStart = EditorApplication.timeSinceStartup;
			AssetDatabase.Refresh();
			PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, "");
			GUIUtility.ExitGUI ();
		}
		else if (state == WAITING) {
			GUILayout.Box ("Awaiting compilation of scripts...");

			if ((EditorApplication.timeSinceStartup - compileGracePeriodStart > COMPILER_GRACE_PERIOD) && !EditorApplication.isCompiling) {
				// If the scripts have compiled, the types will be available, so we can move on.
				Type type = GetTypeByName(selectorName);
				if (type != null)
					state = CREATING_PREFABS;
				else {
					// Compiling is done, but the most basic script can't be found.  Error.
					results.Add("Could not find compiled version of scripts.");
					foreach (string result in results)
						Multimorphic.P3App.Logging.Logger.Log (result);
					state = DONE; 
				}

			}
			GUIUtility.ExitGUI ();
		}
		else if (state == CREATING_PREFABS) {
			GUILayout.Box ("Creating prefabs...");
			CreatePrefabs ();
			AssetDatabase.Refresh();

			foreach (string result in results)
				Multimorphic.P3App.Logging.Logger.Log (result);

			state = DONE;

			GUIUtility.ExitGUI ();
		}
		else if (state == DONE) {
			AssetDatabase.Refresh();

			// GUILayout.Label("Results:");

			foreach (string result in results)
				GUILayout.Label(result);

			if (GUILayout.Button ("Close")) {
				Close ();
				GUIUtility.ExitGUI ();
			}
//			GUIUtility.ExitGUI ();
		} 
	}
	

	private string AppCodeFromAssets () {
		string appCode = "";
		var guids = AssetDatabase.FindAssets ("Setup", null);
		
		foreach (string guid in guids) {
			string fileName = AssetDatabase.GUIDToAssetPath(guid);
			// If it's a script, replace the app code.
			fileName = fileName.Replace ("Setup.cs", "");
			fileName = fileName.Replace("Assets/Scripts/GUI/", "");
			if (!fileName.Contains ("/") && !fileName.Contains ("."))
				appCode = fileName;
		}
		return(appCode);
	}

	private string CompanyFromAssets () {
		string company = "";
		var guids = AssetDatabase.FindAssets ("SceneController", null);
		
		foreach (string guid in guids) {
			string fileName = AssetDatabase.GUIDToAssetPath(guid);
			
			if ( (Path.GetExtension (fileName) == ".cs") && !fileName.Contains("P3ProjectStarter") ) {
				var fileContents = System.IO.File.ReadAllText (@fileName);
				List<string> lines = Regex.Split(fileContents, "\n").ToList<string>();
				
				foreach(string line in lines) {
					string lower = line.ToLower ();
					if (lower.Contains("namespace") && lower.Contains(".gui") ) {
						int start = lower.IndexOf("namespace") + 10;
						int end = lower.IndexOf(".");
						string candidate = line.Substring(start, end - start).Trim(); 			

						if (company == "")  // first one found
							company = candidate;  
						else if (candidate != "Multimorphic")   // give preference to non-Multimorphic names
							company = candidate;   
					}
				}
			}
		}
		
		return(company);
	}

	string WriteSelectorScript() {
		string shape = "";
		if (isGrid)
			shape = "GridShaped";

		string[] lines = {
			"using UnityEngine;",
			"using System.IO;",
			"using System.Collections;",
			"using System.Collections.Generic;",
			"using Multimorphic.P3App.GUI;",
			"using Multimorphic.P3App.GUI.Selector;",
			"",
			"namespace COMPANYNAME.APPCODE.GUI {",
			"",
			"	public class UPPERSELECTORNAME : " + shape + "Selector {",
			"	",		
			"		private List<string> LOWERSELECTORNAMEData = new List<string>();  // Could be a list of any type, depending on what data might come from the mode side via OnReceiptOfDataFromModes().",
			"",
			"		// Use this for initialization",
			"		public override void Start () {",
			"			base.Start ();	",
			"		}",
			"",
			"		protected override void CreateEventHandlers () ",
			"		{",
			"			base.CreateEventHandlers ();",
			"			// The following event handler is only required if the content of the selector is derived from data that comes from the mode side.",
			"			AddModeEventHandler (\"Evt_UPPERSELECTORNAMEData\", UPPERSELECTORNAMEDataEventHandler);",
			"		}",
			"",
			"		// Update is called once per frame",
			"		public override void Update () {",
			"			base.Update ();",
			"		}",
			"",
            "        /// <summary>",
            "        /// Receives data for this selector, as it was sent via the modes' SetSelectorData call.",
            "        /// Store the data locally for later reference in PrepareDataObjects.",
            "        /// ",
            "        /// For example, if we received some contacts as data, we might store it in a local member contactsReceivedFromMode:",
            "        ///     contactsReceivedFromMode.Clear();",
            "        ///     contactsReceivedFromMode.AddRange(dataFromMode as List<Contact>);",
            "        /// ",
            "        /// base.OnReceiptOfDataFromMode(dataFromMode) must be the last line in this method.",
            "        /// </summary>",
            "        /// <param name=\"dataFromMode\">Data for this selector, as it was sent via the modes' SetSelectorData call.</param>",
            "        protected override void OnReceiptOfDataFromMode(object dataFromMode)",
            "        {",
            "            var data = dataFromMode as List<string>;",
            "",
            "            LOWERSELECTORNAMEData.Clear();   // Forget the data we had before",
            "            LOWERSELECTORNAMEData.AddRange(data);   // Remember this new data.  PrepareDataObjects will filter it and organize it into selector items later.",
            "    ",
            "            // base call must be last",
            "            base.OnReceiptOfDataFromMode(dataFromMode);",
            "        }",
            "",
            "        /// <summary>",
            "        /// Prepare one data object for each intended selector item, using AddDataObject.",
            "        /// This might be a constant set, such as:",
            "        ///      AddDataObject(\"Apple\", \"\");",
            "        ///      AddDataObject(\"Orange\", \"\");",
            "        ///      ",
            "        /// Or it might be data that was received in OnReceiptOfDataFromMode() and saved in a local list:",
            "        ///      foreach (var contact in contactsReceivedFromMode)",
            "        ///         AddDataObject(contact, \"\");    // or, optionally, AddDataObject(contact.lastName + \", \" + contact.firstName, \"\");",
            "        /// ",
            "        /// If a tree-structured selector is desired, the path parameter of AddDataObject can be used.  Here's an example to make a list of files:",
            "        ///       AddDataObject(\"Apple\", \"Food/Fruit\");",
            "        ///       AddDataObject(\"Orange\", \"Food/Fruit\");",
            "        ///       AddDataObject(\"Carrot\", \"Food/Vegetables\");",
            "        ///       AddDataObject(\"Lettuce\", \"Food/Vegetables\");",
            "        /// ",
            "        /// </summary>",
            "		protected override void PrepareDataObjects ()",
			"		{",
			"			base.PrepareDataObjects ();",
			"",
            "            // The following works for single text strings and also for filename paths",
			"			if (LOWERSELECTORNAMEData != null) {",
			"				foreach(string item in LOWERSELECTORNAMEData) {",
			"					string path = Path.GetDirectoryName(item);",
			"					string caption = Path.GetFileName(item);",
			"					AddDataObject(caption, path);",
			"				}",
			"			}",
			"",
			"			// Uncomment this next line if all the items are to be visible all the time. ",
			"			// Leave it commented if the visible items are only a subset of a larger list of data objects.",
			"			// visibleCount = dataObjects.Count;",   
			"",
			"			dataObjectsArePrepared = true; // So that the current (or next) Refresh() will use the dataObjects to rebuild the selectable items",
			"		}",
			"",
			"		protected override void InitializeSelectorItem(SelectorItem item)",
			"		{",
			"			base.InitializeSelectorItem (item);",
			"			item.caption = (string) item.dataObject;",
			"		}",
			"",
			"		protected override void InitializeSubfolderItem (string fullPath, string branchName, int itemIndex, SelectorItem item)",
			"		{",
			"			base.InitializeSubfolderItem (fullPath, branchName, itemIndex, item);",
			"			item.caption = branchName + \" ...\";",
			"		}",
			"",
			"		public override void Select ()",
			"		{" ,
			"			base.Select ();",
			"			// Add code here to determine what to do when the user selects an item.",
			"			PostGUIEventToModes (\"Evt_UPPERSELECTORNAMESelect\", currentItemIndex);",
			"		}",
			"",
			"		public void UPPERSELECTORNAMEDataEventHandler(string eventName, object eventData) {",
			"			// This event handler is only required if this selector's content is derived from data that comes from the mode side.",
			"			Multimorphic.P3App.Logging.Logger.Log(\"UPPERSELECTORNAME receiving UPPERSELECTORNAME data\");",
			"			// Store the data locally so that it can be later used in PrepareDataObjects.",
			"			LOWERSELECTORNAMEData = (List<string>) eventData;     // For example.  Any suitable type could be sent from the mode",
			"",
			"			this.Refresh();",
			"		}",
			"	}",
			"}"};
	
		for(int i=0; i < lines.Count(); i++) {
			lines[i] = lines[i].Replace("COMPANYNAME", companyName);
			lines[i] = lines[i].Replace("APPCODE", appCode);
			lines[i] = lines[i].Replace("UPPERSELECTORNAME", upperSelectorName);
			lines[i] = lines[i].Replace("LOWERSELECTORNAME", lowerSelectorName);
		}

		string assetName = "/Scripts/GUI/" + upperSelectorName + ".cs";
		string filename = Application.dataPath + assetName; 
		System.IO.File.WriteAllLines(filename, lines);
		AssetDatabase.StartAssetEditing ();
		AssetDatabase.ImportAsset("Assets/" + assetName);
		AssetDatabase.StopAssetEditing ();
		AssetDatabase.Refresh();
		results.Add ("Created GUI script " + assetName); 
		return(filename);
	}

	string WriteSelectorItemScript() {
		string[] lines = {
			"using UnityEngine;",
			"using System.Collections;",
			"using Multimorphic.P3App.GUI;",
			"using Multimorphic.P3App.GUI.Selector;",
			"",
			"namespace COMPANYNAME.APPCODE.GUI {",
			"",
			"	public class UPPERSELECTORNAMEItem : " + "SelectorItem {",
			"	",		
			"",
			"		// Use this for initialization",
			"		public override void Start () {",
			"			base.Start ();	",
			"		}",
			"",
			"		// Update is called once per frame",
			"		public override void Update () {",
			"			base.Update ();",
			"			if (selected) {",
			"				// Change some properties of gameObject or child gameObjects here to indicate that this item is the current item.",
			"			}",
			"		}",
			"	}",
			"}"};

		for(int i=0; i < lines.Count(); i++) {
			lines[i] = lines[i].Replace("COMPANYNAME", companyName);
			lines[i] = lines[i].Replace("APPCODE", appCode);
			lines[i] = lines[i].Replace("UPPERSELECTORNAME", upperSelectorName);
			lines[i] = lines[i].Replace("LOWERSELECTORNAME", lowerSelectorName);
		}

		string assetName = "/Scripts/GUI/" + upperSelectorName + "Item.cs";
		string filename = Application.dataPath + assetName; 
		System.IO.File.WriteAllLines(filename, lines);

		AssetDatabase.StartAssetEditing ();
		AssetDatabase.ImportAsset("Assets" + assetName);
		AssetDatabase.StopAssetEditing ();
		AssetDatabase.Refresh();
		results.Add ("Created GUI script " + assetName + "."); 
		return(filename);
	}

	string WriteSelectorSubfolderItemScript() {
		string[] lines = {
			"using UnityEngine;",
			"using System.Collections;",
			"using Multimorphic.P3App.GUI;",
			"using Multimorphic.P3App.GUI.Selector;",
			"",
			"namespace COMPANYNAME.APPCODE.GUI {",
			"",
			"	public class UPPERSELECTORNAMESubfolderItem : " + "SelectorItem {",
			"	",		
			"",
			"		// Use this for initialization",
			"		public override void Start () {",
			"			base.Start ();	",
			"		}",
			"",
			"		// Update is called once per frame",
			"		public override void Update () {",
			"			base.Update ();",
			"			if (selected) {",
			"				// Change some properties of gameObject or child gameObjects here to indicate that this subfolder item is the current item.",
			"			}",
			"		}",
			"	}",
			"}"};

		for(int i=0; i < lines.Count(); i++) {
			lines[i] = lines[i].Replace("COMPANYNAME", companyName);
			lines[i] = lines[i].Replace("APPCODE", appCode);
			lines[i] = lines[i].Replace("UPPERSELECTORNAME", upperSelectorName);
			lines[i] = lines[i].Replace("LOWERSELECTORNAME", lowerSelectorName);
		}

		string assetName = "/Scripts/GUI/" + upperSelectorName + "SubfolderItem.cs";
		string filename = Application.dataPath + assetName; 
		System.IO.File.WriteAllLines(filename, lines);

		AssetDatabase.StartAssetEditing ();
		AssetDatabase.ImportAsset("Assets" + assetName);
		AssetDatabase.StopAssetEditing ();
		AssetDatabase.Refresh();
		results.Add ("Created GUI script " + assetName + "."); 
		return(filename);
	}

	string WriteModeScript() {
		string[] lines = {
			"using System.Collections;",
			"using System.Collections.Generic;",
			"using Multimorphic.P3;",
			"using Multimorphic.P3App.Modes.Selector;",
			"",
			"namespace COMPANYNAME.APPCODE.Modes {",
			"",
			"	public class UPPERSELECTORNAMEMode : " + "SelectorMode {",
			"	",		
			"		public UPPERSELECTORNAMEMode(P3Controller controller, int priority)",
			"			: base(controller, priority)",
			"		{",
			"            selectorId = \"UPPERSELECTORNAME\";",
			"",
			"			buttonLegend[\"LeftWhiteButton\"] = \"\";",
			"			buttonLegend[\"RightWhiteButton\"] = \"\";",
			"			buttonLegend[\"LeftRedButton\"] = \"Up\";",
			"			buttonLegend[\"RightRedButton\"] = \"Down\";",
			"			buttonLegend[\"LeftYellowButton\"] = \"Exit\";",
			"			buttonLegend[\"RightYellowButton\"] = \"Select\";",
			"			buttonLegend[\"StartButton\"] = \"Select\";",
			"			buttonLegend[\"LaunchButton\"] = \"\";",
			"",
			"			AddSwitchHandlerMap(\"buttonLeft0\", Left);",
			"			AddSwitchHandlerMap(\"buttonRight0\", Right);",
			"			AddSwitchHandlerMap(\"buttonLeft1\", Exit);",
			"			AddSwitchHandlerMap(\"buttonRight1\", Enter);",
			"			AddSwitchHandlerMap(\"start\", Exit);",
			"			AddSwitchHandlerMap(\"launch\", Enter);",
			"			AddSwitchHandlerMap(\"buttonLeft2\", Shift);",
			"			AddSwitchHandlerMap(\"buttonRight2\", Shift);",
			"",
			"            AddGUIEventHandler(\"Evt_UPPERSELECTORNAMEExit\", UPPERSELECTORNAMEExitEventHandler);",
			"            AddGUIEventHandler(\"Evt_UPPERSELECTORNAMESelect\", UPPERSELECTORNAMESelectEventHandler);",
			"        }",
			"",
			"		public override void mode_started ()",
			"		{",
			"			base.mode_started ();",
			"		}",
			"",
			"        protected void UPPERSELECTORNAMEExitEventHandler(string evtName, object evtData)",
			"        {",
			"            PostModeEventToModes(\"Evt_CloseDialog\", \"UPPERSELECTORNAME\");",
			"        }",
			"",
            "       /// <summary>",
            "       /// Handling for what should happen when the user has selected an item.",
            "       /// </summary>",
            "       /// <param name=\"evtName\"></param>", 
            "       /// <param name=\"evtData\"></param>",
			"        protected void UPPERSELECTORNAMESelectEventHandler(string evtName, object evtData)",
			"        {",
			"            PostModeEventToModes(\"Evt_UPPERSELECTORNAMESelect\", evtData);",
            "",
            "            // Close the dialog after selection.  Comment this out if the selector should stay open on select.",
			"            PostModeEventToModes(\"Evt_CloseDialog\", \"UPPERSELECTORNAME\");",
			"        }",
			"",
			"	}",
			"}"};

		for(int i=0; i < lines.Count(); i++) {
			if (lines[i] == "//CHOICES")
			{
				if (createSubfolderItem) {
					lines [i + 0] = "			choices.Add(\"Fruit/Apple\");";
					lines [i + 1] = "			choices.Add(\"Fruit/Orange\");";
					lines [i + 2] = "			choices.Add(\"Fruit/Pear\");";
					lines [i + 3] = "			choices.Add(\"Vegetable/Corn\");";
					lines [i + 4] = "			choices.Add(\"Vegetable/Carrot\");";
					lines [i + 5] = "			choices.Add(\"Vegetable/Bean\");";
					lines [i + 6] = "";
					lines [i + 7] = "";
				}
				else {
					lines [i + 0] = "			choices.Add(\"Apple\");";
					lines [i + 1] = "			choices.Add(\"Orange\");";
					lines [i + 2] = "			choices.Add(\"Pear\");";
					lines [i + 3] = "			choices.Add(\"Grape\");";
					lines [i + 4] = "			choices.Add(\"Banana\");";
					lines [i + 5] = "			choices.Add(\"Blueberry\");";
					lines [i + 6] = "";
					lines [i + 7] = "";
				}
			}
		}

		for(int i=0; i < lines.Count(); i++) {
			lines[i] = lines[i].Replace("COMPANYNAME", companyName);
			lines[i] = lines[i].Replace("APPCODE", appCode);
			lines[i] = lines[i].Replace("UPPERSELECTORNAME", upperSelectorName);
			lines[i] = lines[i].Replace("LOWERSELECTORNAME", lowerSelectorName);
		}

		string assetName = "/Scripts/Modes/" + upperSelectorName + "Mode.cs";
		string filename = Application.dataPath + assetName; 
		System.IO.File.WriteAllLines(filename, lines);
		AssetDatabase.StartAssetEditing ();
		AssetDatabase.ImportAsset("Assets/" + assetName);
		AssetDatabase.StopAssetEditing ();
		AssetDatabase.Refresh();
		results.Add ("Created mode script " + assetName + " with selector id " + upperSelectorName + "."); 
		return(filename);
	}

	string AddPriority() {
		bool priorityExists = false;
		string assetName = "/Scripts/Modes/" + appCode + "Priorities.cs";
		string filename = Application.dataPath + assetName; 
		string[] lines = File.ReadAllLines (filename);
		int lastConstantLineIndex = -1;

		for(int i=0; i < lines.Count(); i++) {
			if (lines[i].Contains("PRIORITY_") && lines[i].Contains("="))
				lastConstantLineIndex = i;

			if (lines [i].Contains ("PRIORITY_" + allUpperSelectorName + " = "))
				priorityExists = true;
				
		}

		if (!priorityExists) {
			List<string> lineList = new List<string> ();
			lineList.AddRange (lines);

			if (lastConstantLineIndex >= 0)
				lineList.Insert (lastConstantLineIndex + 1, "\t\tpublic const int PRIORITY_" + allUpperSelectorName + " = " + priority + ";");

			System.IO.File.WriteAllLines (filename, lineList.ToArray ());
			AssetDatabase.ImportAsset ("Assets/" + assetName);
			AssetDatabase.Refresh ();
			results.Add ("Added priority constant PRIORITY_" + allUpperSelectorName + " in mode script " + assetName + "."); 
		}

		return(filename);
	}

	string RewriteRegistrationMethod() {
		bool alreadyRegistered = false;
		string assetName = "/Scripts/Modes/" + appCode + "BaseGameMode.cs";
		string filename = Application.dataPath + assetName; 
		string[] lines = File.ReadAllLines (filename);
		int lastRegistrationLineIndex = -1;

		for(int i=0; i < lines.Count(); i++) {
			if (lines[i].Contains(".RegisterSelector("))
				lastRegistrationLineIndex = i;

			if (lines [i].Contains (".RegisterSelector(") && lines [i].Contains (upperSelectorName) && lines [i].Contains (dialogName))
				alreadyRegistered = true;
		}

		List<string> lineList = new List<string>();
		lineList.AddRange(lines);

		if (!alreadyRegistered && (lastRegistrationLineIndex >= 0)) {
			lineList.Insert (lastRegistrationLineIndex + 1, "\t\t\tselectorManagerMode.RegisterSelector(new " + upperSelectorName + "Mode(p3, " + appCode + "Priorities.PRIORITY_" + allUpperSelectorName + "), \"" + dialogName + "\", \"Prefabs/GUI/" + dialogName + "\", " + (!dialogExists).ToString().ToLower() + ");");

			System.IO.File.WriteAllLines (filename, lineList.ToArray ());
			AssetDatabase.ImportAsset ("Assets/" + assetName);
			AssetDatabase.Refresh ();
			results.Add ("Updated RegisterSelectors method in mode script " + assetName + "."); 
		}
		return(filename);
	}

	private void CreatePrefabs() {
		GameObject dialogGO = null;
		UnityEngine.Object dialogPrefab = null;

		string[] prefabsFolder = {"Assets/Resources/Prefabs"};
		string[] assets = AssetDatabase.FindAssets(dialogName, prefabsFolder);
		if (assets.Length > 0) {
			Multimorphic.P3App.Logging.Logger.LogWarning ("Loading existing dialog prefab " + dialogName);
			dialogPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(assets[0])) as GameObject;
			dialogGO = PrefabUtility.InstantiatePrefab (dialogPrefab) as GameObject;
			dialogExists = true;
		}
		else {
			// Multimorphic.P3App.Logging.Logger.LogWarning ("Creating dialog prefab " + upperSelectorName);
			dialogGO = new GameObject(dialogName);
		}

		GameObject selectorGO = new GameObject(upperSelectorName);
		selectorGO.transform.parent = dialogGO.transform;
		selectorGO.transform.localPosition = new Vector3 (-2f, 4f, 0f);
		Type selectorType = GetTypeByName(upperSelectorName);
		Multimorphic.P3App.GUI.Selector.Selector selector = selectorGO.AddComponent(selectorType) as Multimorphic.P3App.GUI.Selector.Selector;
		selector.nodePrefix = upperSelectorName + "Node";
		selector.visibleCount = visibleCount;
		if (isGrid) {
			(selector as GridShapedSelector).rowCount = rowCount;
			(selector as GridShapedSelector).columnCount = columnCount;
		} 
		else {
			GameObject nodeHead = new GameObject ("Nodes");
			nodeHead.transform.parent = selector.transform;
			nodeHead.transform.localRotation = Quaternion.identity;
			nodeHead.transform.localScale = Vector3.one;
			nodeHead.transform.localPosition = Vector3.zero;
			for (int i = 0; i < visibleCount; i++) {
				GameObject node = new GameObject (selector.nodePrefix + i.ToString());
				node.transform.parent = nodeHead.transform;
				node.transform.localRotation = Quaternion.identity;
				node.transform.localScale = Vector3.one;
				node.transform.localPosition = Vector3.zero;
			}
				
		}

		string selectorItemName = upperSelectorName + "Item";
		// Multimorphic.P3App.Logging.Logger.LogWarning ("Creating selector item prefab " + selectorItemName);
		GameObject selectorItemGO = new GameObject(selectorItemName);
		Type selectorItemType = GetTypeByName(selectorItemName);
		Multimorphic.P3App.GUI.Selector.SelectorItem selectorItem = selectorItemGO.AddComponent(selectorItemType) as Multimorphic.P3App.GUI.Selector.SelectorItem;
		// Make caption text on selector item
		GameObject captionGO = new GameObject("Caption");
		captionGO.transform.parent = selectorItemGO.transform;
		TextMesh caption = captionGO.AddComponent<TextMesh>();
		caption.text = "caption";
		caption.fontSize = 20;
		caption.characterSize = 0.5f;
		caption.anchor = TextAnchor.MiddleLeft;
		selectorItem.captionDisplay = captionGO;

		string assetName = "Assets/Resources/Prefabs/GUI/" + selectorItemName + ".prefab";
		GameObject selectorItemPrefab = PrefabUtility.CreatePrefab(assetName, selectorItemGO);
		results.Add ("Created selector item prefab " + assetName); 

		selector.selectorItemPrefab = selectorItemPrefab;
		selector.selectorId = upperSelectorName;

		GameObject subfolderItemGO = null;
		if (createSubfolderItem) {
			string subfolderItemName = upperSelectorName + "SubfolderItem";
			// Multimorphic.P3App.Logging.Logger.LogWarning("Creating subfolder item prefab " + subfolderItemName);
			subfolderItemGO = new GameObject(subfolderItemName);
			Type subfolderItemType = GetTypeByName(subfolderItemName);
			Multimorphic.P3App.GUI.Selector.SelectorItem subfolderItem = subfolderItemGO.AddComponent(subfolderItemType) as Multimorphic.P3App.GUI.Selector.SelectorItem;
			// Make caption text on selector item
			captionGO = new GameObject("Caption");
			captionGO.transform.parent = subfolderItemGO.transform;
			caption = captionGO.AddComponent<TextMesh>();
			caption.text = "caption...";
			caption.fontSize = 20;
			caption.characterSize = 0.5f;
			caption.anchor = TextAnchor.MiddleLeft;
			subfolderItem.captionDisplay = captionGO;

			assetName = "Assets/Resources/Prefabs/GUI/" + subfolderItemName + ".prefab";
			GameObject subfolderItemPrefab = PrefabUtility.CreatePrefab(assetName, subfolderItemGO);
			results.Add ("Created subfolder item prefab " + assetName); 

			selector.subfolderItemPrefab = subfolderItemPrefab;
		}

		GameObject highlightGO = null;
		if (createHighlight) {
			string highlightName = upperSelectorName + "Highlight";
			// Multimorphic.P3App.Logging.Logger.LogWarning ("Creating highlight prefab " + highlightName);
			highlightGO = new GameObject(highlightName);
			highlightGO.AddComponent<SelectorHighlight>();
			GameObject capsule = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			capsule.transform.parent = highlightGO.transform;
			capsule.transform.localRotation = Quaternion.identity;
			capsule.transform.localScale = new Vector3(0.8f, 0.8f, 0.1f);
			capsule.transform.localPosition = new Vector3(-1, 0, 0);
			capsule.GetComponent<Renderer> ().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

			assetName = "Assets/Resources/Prefabs/GUI/" + highlightName + ".prefab";
			GameObject highlightPrefab = PrefabUtility.CreatePrefab(assetName, highlightGO);
			results.Add ("Created highlight prefab " + assetName); 

			selector.itemHighlightPrefab = highlightPrefab;
		}

		assetName = "Assets/Resources/Prefabs/GUI/" + dialogName + ".prefab";
		if (dialogExists) {
			PrefabUtility.ReplacePrefab(dialogGO, PrefabUtility.GetPrefabParent(dialogGO), ReplacePrefabOptions.ConnectToPrefab);
			results.Add ("Updated dialog prefab " + assetName);
		}
		else {
			dialogGO.AddComponent<FrontAndCenter> ();
			PrefabUtility.CreatePrefab (assetName, dialogGO);
			results.Add ("Created dialog prefab " + assetName);
		}

		AssetDatabase.Refresh();
		if (dialogGO)
			DestroyImmediate(dialogGO);
		if (selectorItemGO)
			DestroyImmediate(selectorItemGO);
		if (subfolderItemGO)
			DestroyImmediate(subfolderItemGO);
		if (highlightGO)
			DestroyImmediate(highlightGO);

        results.Add("\n\nUse the following lines to set the data for this selector and open the dialog from a mode:\n" +
                    "   List<string> choices = new List<string>();\n" +
                    "   choices.Add(\"Apple\"); \n" +
                    "   choices.Add(\"Orange\");\n" +
                    "   choices.Add(\"Pear\");\n" +
                    "   choices.Add(\"Grape\");\n" +
                    "   choices.Add(\"Banana\");\n" +
                    "   choices.Add(\"Blueberry\");\n" +
                    "   SetSelectorData(\"" + selectorName + "\", choices);\n" +
                    "   PostModeEventToModes(\"Evt_OpenDialog\", \"" + dialogName + "\");\n");
        results.Add("If using more complex data than simple strings and/or \nif the selector should always contain a constant list of items,\nsee OnReceiptOfDataFromMode and PrepareDataObjects in " + selectorName + ".cs \n");

        results.Add("Use the following line to close the dialog from a mode:\n" +
			"   PostModeEventToModes(\"Evt_CloseDialog\", \"" + dialogName + "\");\n");

		results.Add ("Use the following lines to monitor when a selection is made or when a selector is exited:\n" +
			"   AddGUIEventHandler(\"Evt_" + upperSelectorName + "Select\", " + upperSelectorName + "SelectEventHandler);\n" +
			"   AddGUIEventHandler(\"Evt_" + upperSelectorName + "Exit\", " + upperSelectorName + "ExitEventHandler);\n");
	}

		
	public static Type GetTypeByName(string name)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
			
                foreach (Type type in assembly.GetExportedTypes())
                {
                    if (type.Name == name)
                        return type;
                }
            }
 
            return null;
        }
}
