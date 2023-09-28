using UnityEngine;
using System.Collections;

namespace PinballClub.TestGame.GUI {
	
	/// <summary>
	/// Controls the attract mode parade of models.
	/// </summary>
	public class ModelSlideshow : MonoBehaviour {

		public GameObject[] models;
		private float countdown;
		private float duration = 6.0f;
		private int modelIndex;
		private Vector3 targetScale;

		// Use this for initialization
		void Start () {
			countdown = -1f;
			modelIndex = models.Length - 1;
		}
		
		// Update is called once per frame
		void Update () {
			countdown -= Time.deltaTime;
			if (countdown < 0) {
				models[modelIndex].SetActive (false);
				modelIndex = ++modelIndex % models.Length;
				models[modelIndex].SetActive (true);
				targetScale = models[modelIndex].transform.localScale;
				models[modelIndex].transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
				countdown = duration + (models.Length - modelIndex);  // to give the first items in the list more display time
			}

			if (models[modelIndex].transform.localScale.x < (targetScale.x * 0.8f)) {
				Vector3 scale = models[modelIndex].transform.localScale;
				scale *= 1.4f;
				models[modelIndex].transform.localScale = scale;
			}
			else
				models[modelIndex].transform.localScale = Vector3.Lerp(models[modelIndex].transform.localScale, targetScale, Time.deltaTime);

		}
	}
}