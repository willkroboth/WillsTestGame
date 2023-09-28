using UnityEngine;
using System;
using System.Collections;

[ExecuteInEditMode]

/// <summary>
/// A placeholder to add a component when it becomes available.
/// </summary>
public class ComponentProxy : MonoBehaviour {
	
	public string className;
	public int attemptCount = 20;
	public int countdown = 50;

	// Update is called once per frame
	public virtual void Update () {
		string componentName = "";

		if (className != "") {
			componentName = className;
			while (componentName.Contains(".")) {
				int position = componentName.IndexOf (".");
				componentName = componentName.Substring(position + 1);
			}

			Type type = Type.GetType(className);
			if (type == null)
				componentName = "";
		}

		Component comp = gameObject.GetComponent (componentName);
		if ((comp == null) && (componentName != "") && (attemptCount > 0) ) {
			attemptCount--;

			Component placedComp = null;
            Type type = Type.GetType(componentName);
			placedComp = gameObject.AddComponent(type);
			placedComp.name = componentName;
			DestroyImmediate (this);
		}				

		if (countdown-- < 0)
			DestroyImmediate (this);
	}

}
