// Copyright � 2019 Multimorphic, Inc. All Rights Reserved

using Multimorphic.P3App.GUI.Selector;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PinballClub.TestGame.GUI {

	public class TestGameProfileNameEntry : Multimorphic.P3App.GUI.TextEditor {
		private GameObject bullet;
		private Text bulletText;
		private float bulletModulation = 0;
		private int bulletIndex = 0;
		private const float bulletSpeed = 2f;
		private const float bulletFadeFactor = 3.0f;
		private List<string> bulletPoints;
		private bool firstTimeShowingBullets;
		public Text captionTextMesh;
		public Text bulletTextMesh;

		public void Awake() {
			bulletPoints = new List<string>();
		}

		// Use this for initialization
		public override void Start() {
			base.Start();

			var bulletTransform = gameObject.transform.Find("BulletPoint");
			if (bulletTransform)
				bullet = gameObject.transform.Find("BulletPoint").gameObject;
			if (bullet) {
				bulletText = bullet.GetComponent<Text>();
			}
		}

		protected override void CreateEventHandlers() {
			base.CreateEventHandlers();
		}

		private void OnEnable() {
			bulletPoints.Clear();
		}

		private void OnDisable() {
		}

		// Update is called once per frame
		public override void Update() {
			base.Update();

			if (textSelector != null) {
				TestGameTextSelector selector = textSelector as TestGameTextSelector;
				if ((selector != null) && (selector.textSelectorData != null) && (bulletPoints.Count == 0)) {
					SetCaptionAndBulletPoints(selector.textSelectorData);
				}
			}
			else {
				if (captionText) {
					captionText.text = "";
				}

				if (captionTextMesh) {
					captionTextMesh.text = "";
				}

				if (bulletText) {
					bulletText.text = "";
				}
			}

			if (bulletPoints.Count > 0) {
				//						if (bulletPoints.Count > 1) {
				bulletModulation += Time.deltaTime * bulletSpeed;

				if (bulletModulation >= 1.8 * Mathf.PI) {
					bulletModulation = 0;
					bulletIndex = (bulletIndex + 1) % bulletPoints.Count;
					if ((bulletIndex) == 0) {
						firstTimeShowingBullets = false;
					}
				}

				if ((bulletModulation == 0) && firstTimeShowingBullets) {
					// Audio.PlaySound3D("NameEditorBulletPoint", gameObject.Position);
				}

				if (bulletText) {
					bulletText.text = bulletPoints[bulletIndex];
					Color bulletColor = bulletText.color;
					bulletColor.a = (Mathf.Clamp01(Mathf.Sin(bulletModulation) + 0.7f) * bulletFadeFactor);
					bulletText.color = bulletColor;
				}

				if (bulletTextMesh) {
					bulletTextMesh.text = bulletPoints[bulletIndex];
					Color bulletColor = bulletTextMesh.color;
				}
			}
		}

		public void SetCaptionAndBulletPoints(TextSelectorData data) {
			if (captionText) {
				captionText.text = data.caption;
			}

			if (captionTextMesh) {
				captionTextMesh.text = data.caption;
			}

			bulletPoints = data.bulletPoints;

			if (bulletText) {
				bulletText.text = "";
			}

			bulletModulation = 0;
			bulletIndex = 0;
			firstTimeShowingBullets = true;
		}
	}
}
