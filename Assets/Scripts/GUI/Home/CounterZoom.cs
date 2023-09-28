using UnityEngine;
using System.Collections;

namespace PinballClub.TestGame.GUI {

	public class CounterZoom : MonoBehaviour {
		private float startZoom;
		private Vector3 startScale;
		private Camera cam;

		// Use this for initialization
		void Start () {
			cam = GameObject.FindObjectOfType<Camera>();
			startZoom = cam.orthographicSize;
			startScale = gameObject.transform.localScale;
		}
		
		// Update is called once per frame
		void Update () {
			gameObject.transform.localScale = startScale * cam.orthographicSize / startZoom; 
		}
	}
}
