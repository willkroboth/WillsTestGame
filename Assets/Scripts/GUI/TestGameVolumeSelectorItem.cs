using UnityEngine;
using System.Collections;
using Multimorphic.P3App.GUI;
using Multimorphic.P3App.GUI.Selector;

public class TestGameVolumeSelectorItem : SelectorItem {

	public bool enlarge = false;

	// Use this for initialization
	public override void Start() {
		base.Start();

		if (enlarge) {
			Transform t = transform.Find("Tick");
			if (t)
				t.localScale += new Vector3 (0f, 0.1f, 0f);
		}
	}
	
	// Update is called once per frame
	public override void Update () {
		base.Update();
	}
}
