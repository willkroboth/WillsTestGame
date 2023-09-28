using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using System.Text.RegularExpressions;
using System.Linq;

public class RenameP3Project {
	
	[MenuItem ("Multimorphic/Rename a P3 project")]
	static void RenameP3ProjectDialog() {
		RenameP3ProjectDialog renameDialog = ScriptableObject.CreateInstance<RenameP3ProjectDialog>();
		renameDialog.Initialize ();
		renameDialog.Show ();
	}
}


public class RenameP3ProjectDialog : EditorWindow {
	
	public string sourceCompany;
	public string targetCompany;
	public string targetAppCode;
	public string sourceAppCode;
	
	public void Initialize() {
		sourceAppCode = AppCodeFromAssets ();
		sourceCompany = CompanyFromAssets ();
	}
	
	void OnGUI() {
		
		GUILayout.Label("Current company: " + sourceCompany);
		GUILayout.Label("Current app code: " + sourceAppCode);
		
		targetCompany = EditorGUILayout.TextField("New company name", targetCompany);
		targetAppCode = EditorGUILayout.TextField("New App Code", targetAppCode);
		
		if (GUILayout.Button("Continue")) {
			this.OnClickContinue();
			GUIUtility.ExitGUI();
		}
		
		if (GUILayout.Button("Cancel")) {
			Close();
			GUIUtility.ExitGUI();
		}
	}
	
	private void OnClickContinue() {
		bool proceed = true;
		targetAppCode = targetAppCode.Trim();

        if (string.IsNullOrEmpty(targetCompany) || (!targetCompany.All(char.IsLetterOrDigit))) {
			EditorUtility.DisplayDialog("Invalid company name", "Please specify a valid company name.", "Close");
			proceed = false;
		}
		
		if (string.IsNullOrEmpty(targetAppCode) || (!targetAppCode.All(char.IsLetterOrDigit))) {
			EditorUtility.DisplayDialog("Invalid app code", "Please specify a valid app code.", "Close");
			proceed = false;
		}
		
		if (targetCompany == targetAppCode) {
            EditorUtility.DisplayDialog("App code cannot match company name", "Please specify a valid app code.", "Close");
            proceed = false;
        }

		if (proceed) {
			Close ();
			RenameAppCode ();
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
	
	private void RenameAppCode ()
	{
		Debug.Log ("Renaming assets...");
        var guids = AssetDatabase.FindAssets("t:Object", null);
		int workingDepth = 1;
		while (workingDepth >= 0) {
			bool foundAssetsAtWorkingDepth = false;
			
			foreach (string guid in guids) {
				string assetName = AssetDatabase.GUIDToAssetPath(guid);
				
				if (!assetName.Contains("Assets/Editor") ) {
					int depth = assetName.Length - assetName.Replace("/", "").Length;
					if (depth == workingDepth) {
                        foundAssetsAtWorkingDepth = true;
                        if (assetName != assetName.Replace(sourceAppCode, targetAppCode))
                        {
							object obj = null;
							if (Path.GetExtension(assetName).Contains("prefab")) {
								obj = Resources.Load(assetName);
							}
                            AssetDatabase.RenameAsset(assetName, Path.GetFileNameWithoutExtension(assetName.Replace(sourceAppCode, targetAppCode)));
							if (obj != null)
								obj = null;
						}
					}
				}
			}
			AssetDatabase.Refresh();
			if (foundAssetsAtWorkingDepth)
				workingDepth++;
			else 
				workingDepth = -1;
		}
		
		Debug.Log ("Renaming within scripts...");
		foreach (string guid in guids) {
			string assetName = AssetDatabase.GUIDToAssetPath(guid);
			
			if (!assetName.Contains("Assets/Editor"))
            {
				// If it's a script or the NetProc config file, replace the app code.
				if ( ((Path.GetExtension (assetName) == ".cs") && 
				    !assetName.Contains("P3ProjectRenamer") &&
				    !assetName.Contains("Scripts/Docs")
				    ) || assetName.Contains("NetProcGameConfig.json"))
                {
					var fileContents = System.IO.File.ReadAllText (@assetName);
					Debug.Log ("Converting " + sourceAppCode + " to " + targetAppCode + " in " + assetName);
                    fileContents = fileContents.Replace (sourceAppCode, targetAppCode);
                    fileContents = fileContents.Replace (sourceCompany + "." + targetAppCode, targetCompany + "." + targetAppCode);
					System.IO.File.WriteAllText (@assetName, fileContents);
				}			
			}
		}
		
		AssetDatabase.Refresh();
		Debug.Log ("Done.");
		
	}
}