using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using System.Text.RegularExpressions;
using System.Linq;

public class AddNewScene
{
    [UnityEditor.MenuItem("Multimorphic/Add new scene")]
    static void AddScene()
    {
        SceneNameWindow sceneNameEditor = ScriptableObject.CreateInstance<SceneNameWindow>();
        sceneNameEditor.ShowUtility();
    }
}

public class SceneNameWindow : EditorWindow, IDisposable
{

    public string sceneName;
    private int stage;

    string appCode;
    string companyName;

    private const string IDLE = "Idle";
    private const string CREATE_SCENE = "Creating scene...";
    private const string CREATE_SCRIPTS = "Creating scripts...";
    private const string AWAITING_COMPILATION = "Awaiting compilation...";
    private const string ERROR = "ERROR.";
    private const string DONE = "Done.";

    public double compileGracePeriodStart;
    private const double COMPILER_GRACE_PERIOD = 2f;  // Wait at least this many seconds after the build has complete for post-build in-editor script compilation to start and finish
    private string status = IDLE;
    private string lastStatus;
    private string error;

    void OnGUI()
    {
        if (status == IDLE)
        {

            sceneName = EditorGUILayout.TextField("Scene Name", sceneName);

            if (GUILayout.Button("Continue"))
            {
                this.OnClickContinue();
                GUIUtility.ExitGUI();
            }

            if (GUILayout.Button("Cancel"))
            {
                sceneName = "";
                Close();
                GUIUtility.ExitGUI();
            }
        }
        else if (status == ERROR)
        {
            GUILayout.Label("Error: " + error);
            if (GUILayout.Button("Ok"))
            {
                this.Close();
                GUIUtility.ExitGUI();
            }
        }
        else
        {
            GUILayout.Label("Status: " + status);
            if (GUILayout.Button("Cancel"))
            {
                this.Close();
                GUIUtility.ExitGUI();
            }
        }
    }


    void OnClickContinue()
    {
        sceneName = sceneName.Trim();

        bool validSceneName = !string.IsNullOrEmpty(sceneName) &&
            Regex.IsMatch(sceneName[0].ToString(), @"^[a-zA-Z]+$") &&   // First character is alpha
            Regex.IsMatch(sceneName, @"^[a-zA-Z0-9_]+$");  // only alphanum or underscore

        if (validSceneName)
            status = CREATE_SCRIPTS;
        else
        {
            EditorUtility.DisplayDialog("Invalid scene name", "Please specify a valid scene name.", "Close");
            return;
        }
    }

    void Update()
    {
        if (status == CREATE_SCRIPTS)
        {
            try
            {
                appCode = AppCodeFromAssets();
                companyName = CompanyFromAssets();

                string sceneScriptPath = Path.Combine(Path.Combine(Path.Combine(Application.dataPath, "Scripts"), "GUI"), sceneName);
                Directory.CreateDirectory(sceneScriptPath);

                WriteSceneModeScript(companyName, appCode, sceneName);
                WriteSceneControllerScript(companyName, appCode, sceneName);

                // After we've built the app, all scripts - imcluding this one, importantly - will be recompiled.
                // We need to wait a little while to let that compilation start before we start checking for its completion.
                compileGracePeriodStart = EditorApplication.timeSinceStartup;
                status = AWAITING_COMPILATION;
                AssetDatabase.Refresh();
            }
            catch (Exception e)
            {
                error = e.Message;
                status = ERROR;
            }
        }
        else if (status == AWAITING_COMPILATION)
        {
            // After we've built the app, all scripts - imcluding this one, importantly - will be recompiled.
            // We wait a little while, then start checking for compilation completion.
            // This recompilation causes this script to lose its non-public data, so we need to reload the certificate inputs.
            if ((EditorApplication.timeSinceStartup - compileGracePeriodStart > COMPILER_GRACE_PERIOD) && !EditorApplication.isUpdating && !EditorApplication.isCompiling)
                status = CREATE_SCENE;
        }
        else if (status == CREATE_SCENE)
        {
            try
            {
                var scene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.DefaultGameObjects);
                GameObject sceneController = new GameObject();
                sceneController.name = sceneName + "SceneController";

                string typeName = companyName + "." + appCode + ".GUI." + sceneName + "SceneController, Assembly-CSharp";
                Type tp = Type.GetType(typeName);
                if (tp == null)
                {
                    error = "No such type as " + typeName;
                    status = ERROR;
                }
                else
                {
                    Debug.Log("Type:" + tp.ToString());
                    sceneController.AddComponent(tp);
                    UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene, "Assets/Scenes/" + sceneName + ".unity");
                    status = DONE;
                }
            }
            catch (Exception e)
            {
                error = e.Message;
                status = ERROR;
            }
        }
        else if (status == DONE)
            Close();

    }

    string WriteSceneModeScript(string companyName, string appCode, string sceneName)
    {
        string[] lines = {
            "using System;",
            "using Multimorphic.P3;",
            "using Multimorphic.P3App.Modes;",
            "using Multimorphic.NetProcMachine.Machine;",
            "using System.Collections.Generic;",
            "",
            "namespace " + companyName + "." + appCode + ".Modes",
            "{",
            "",
            "	public class " + sceneName + "Mode : SceneMode",
            "	{",
            "",
            "		// private SomeRelatedMode someRelatedMode;",
            "",
            "		public " + sceneName +"Mode (P3Controller controller, int priority, string SceneName)",
            "			: base(controller, priority, SceneName)",
            "		{",
            "			// AddModeEventHandler(\"Evt_SomeModeEventName\", SomeHandlerFunction, priority);",
            "		}",
            "",
            "		public override void mode_started ()",
            "		{",
            "			base.mode_started ();",
            "",
            "			// p3.AddMode(someRelatedMode);",
            "",
            "			// AddGUIEventHandler (\"Evt_SomeGUIEventName\", SomeHandlerFunction);",
            "			// AddModeEventHandler (\"SomeModeEventName\", SomeHandlerFunction, priority);",
            "		}",
            "",
            "		public override void mode_stopped ()",
            "		{",
            "			// p3.RemoveMode (someRelatedMode);",
            "			// RemoveGUIEventHandler (\"Evt_SomeGUIEventName\", SomeHandlerFunction);",
            "			// RemoveModeEventHandler (\"Evt_SomeModeEventName\", SomeHandlerFunction, priority);",
            "			// p3.RemoveMode(someRelatedMode);",
            "			base.mode_stopped();",
            "		}",
            "",
            "		public override void LoadPlayerData()",
            "		{",
            "			base.LoadPlayerData();",
            "			// Add any special data loading needed here for this scene and this player",
            "		}",
            "",
            "		public override void SavePlayerData()",
            "		{",
            "			base.SavePlayerData();",
            "			// Add any special data loading needed here for this scene and this player",
            "		}",
            "",
            "		public override void SceneLiveEventHandler( string evtName, object evtData )",
            "		{",
            "			base.SceneLiveEventHandler(evtName, evtData);",
            "			// Add any special setup that the scene requires here, including sending messages to the GUI.",
            "		}",
            "",
            "		protected override void StartPlaying()",
            "		{",
            "			base.StartPlaying();",
            "",
            "			PostModeEventToGUI(\"Evt_" + sceneName + "Setup\", 0);",
            "",
            "			// PostInstructionEvent(\"Some instructions\");",
            "			" + appCode + "BallLauncher.launch ();",
            "		}",
            "",
            "		protected override void Completed(long score)",
            "		{",
            "			base.Completed (score);",
            "			PostModeEventToModes (\"Evt_" + sceneName + "Completed\", 0);",
            "		}",
            "",
            "",
            "		public bool sw_slingL_active(Switch sw)",
            "		{",
            "			// Add code here to let he GUI side know about that a sling has been hit",
            "			//e.g. PostModeEventToGUI(\"Evt_" + sceneName + "SlingHit\", false);",
            "",
            "			return SWITCH_CONTINUE;   // use SWITCH_STOP to prevent other modes from receiving this notification.",
            "		}",
            "",
            "		public bool sw_slingR_active(Switch sw)",
            "		{",
            "			// Add code here to let he GUI side know about that a sling has been hit",
            "			// e.g. PostModeEventToGUI(\"Evt_" + sceneName + "SlingHit\", false);",
            "			return SWITCH_CONTINUE;   // use SWITCH_STOP to prevent other modes from receiving this notification.",
            "		}",
            "",
            "		public override void End()",
            "		{",
            "			// someRelatedMode.End();",
            "",
            "			Pause();",
            "			// Save any remaining stats",
            ""          ,
            "			base.End ();",
            "		}",
            "",
            "		public void Pause()",
            "		{",
            "			p3.ModesToGUIEventManager.Post(\"Evt_ScenePause\", null);",
            "		}",
            "",
            "		public override ModeSummary getModeSummary()",
            "		{",
            "			ModeSummary modeSummary = new ModeSummary();",
            "			modeSummary.Title = sceneName;",
            "			modeSummary.Completed = modeCompleted;",
            "			if (modeCompleted) ",
            "				modeSummary.SetItemAndValue(0, \"" + sceneName + " completed!\", \"\");",
            "			else",
            "				modeSummary.SetItemAndValue(1, \"" + sceneName + " not yet completed!\", \"\");",
            "			modeSummary.SetItemAndValue(2, \"\", \"\");",
            "			return modeSummary;",
            "		}",
            "",
            "	}",
            "}"};

        string filename = Path.Combine(Path.Combine(Path.Combine(Path.Combine(Application.dataPath, "Scripts"), "Modes"), "SceneModes"), sceneName + "Mode.cs");
        System.IO.File.WriteAllLines(filename, lines);
        return (filename);
    }

    string WriteSceneControllerScript(string companyName, string appCode, string sceneName)
    {
        string[] lines = {
            "using UnityEngine;",
            "using System.Collections;",
            "using Multimorphic.P3App.GUI;",
            "",
            "namespace " + companyName + "." + appCode + ".GUI {",
            "",
            "	public class " + sceneName + "SceneController : " + appCode + "SceneController {",
            "	",
            "		// Use this for initialization",
            "		public override void Start () {",
            "			base.Start ();",
            "		}",
            "",
            "		protected override void CreateEventHandlers() {",
            "			base.CreateEventHandlers ();",
            "			// AddModeEventHandler((\"Evt_SomeEventName\", SomeHandlerFunction);",
            "		}",
            "		",
            "		// Update is called once per frame",
            "		public override void Update () {",
            "			base.Update ();",
            "		}",
            "",
            "		protected override void SceneLive() {",
            "			base.SceneLive();",
            "		}",
            "	}",
            "}"};

        string filename = Path.Combine(Path.Combine(Path.Combine(Path.Combine(Application.dataPath, "Scripts"), "GUI"), sceneName), sceneName + "SceneController.cs");
        System.IO.File.WriteAllLines(filename, lines);
        return (filename);
    }

    private string AppCodeFromAssets()
    {
        string appCode = "";
        var guids = AssetDatabase.FindAssets("Setup", null);

        foreach (string guid in guids)
        {
            string fileName = AssetDatabase.GUIDToAssetPath(guid);
            // Debug.Log ("App code might be in " + fileName);

            // If it's a script, replace the app code.
            fileName = fileName.Replace("Setup.cs", "");
            fileName = fileName.Replace("Assets/Scripts/GUI/", "");
            if (!fileName.Contains("/") && !fileName.Contains("."))
                appCode = fileName;
        }
        return (appCode);
    }

    private string CompanyFromAssets()
    {
        string company = "";
        var guids = AssetDatabase.FindAssets("SceneController", null);

        foreach (string guid in guids)
        {
            string fileName = AssetDatabase.GUIDToAssetPath(guid);

            if ((Path.GetExtension(fileName) == ".cs") && !fileName.Contains("P3ProjectStarter"))
            {
                var fileContents = System.IO.File.ReadAllText(@fileName);
                List<string> lines = Regex.Split(fileContents, "\n").ToList<string>();

                foreach (string line in lines)
                {
                    string lower = line.ToLower();
                    if (lower.Contains("namespace") && lower.Contains(".gui"))
                    {
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

        return (company);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            // dispose managed resources
        }
        // free native resources
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }


}