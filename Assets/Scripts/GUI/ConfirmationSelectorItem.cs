// Copyright � 2019 Multimorphic, Inc. All Rights Reserved

using Multimorphic.P3App.GUI.Selector;
using UnityEngine;

public class ConfirmationSelectorItem : SelectorItem {
	public bool wasChosen;
	public GameObject highlight;

	// Use this for initialization
	public override void Start() {
		base.Start();
	}

	// Update is called once per frame
	public override void Update() {
		base.Update();

		if (highlight) {
			highlight.SetActive(wasChosen);
		}
	}
}