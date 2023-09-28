using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KeyIndicator : MonoBehaviour {

	public KeyCode key;
	private Text text;

	// Use this for initialization
	void Start () {
		text = gameObject.GetComponent<Text> ();	
		text.enabled = false;
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (key))
			text.enabled = true;
		if (Input.GetKeyUp (key))
			text.enabled = false;
	}
}
