using UnityEngine;
using System.Collections;
using Multimorphic.P3App.GUI;

public class TestGameAttributeSelectorItem : AttributeSelectorItem {

	private TextMesh saveIndication;
	private Color saveIndicationStartColor;
	private Color saveIndicationEndColor;
	const float saveIndicationSpeed = 0.8f;

	// Use this for initialization
	public override void Start () {
		base.Start ();

		saveIndicationStartColor = Color.white;
		saveIndicationEndColor = Color.white;
		saveIndicationEndColor.a = 0;

		Transform t = gameObject.transform.Find ("SaveIndication");
		if (t) 
			saveIndication = t.gameObject.GetComponent<TextMesh>();
		if (saveIndication) {
			saveIndicationStartColor = saveIndication.color;
			saveIndication.color = saveIndicationEndColor;
		}
	}
	
	// Update is called once per frame
	public override void Update () {
		base.Update ();

		if (saveIndication)
			saveIndication.color = Color.Lerp(saveIndication.color, saveIndicationEndColor, Time.deltaTime / saveIndicationSpeed);
	}

	public override void ShowSaveIndication() {
		base.ShowSaveIndication ();
		if (saveIndication)
			saveIndication.color = saveIndicationStartColor;
	}
}
