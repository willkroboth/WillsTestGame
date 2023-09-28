// Copyright � 2019 Multimorphic, Inc. All Rights Reserved

using Multimorphic.P3App.GUI;
using UnityEngine;
using UnityEngine.UI;

namespace PinballClub.TestGame.GUI {

	public class Credits : P3Aware {
		private Text creditsText;
		private float creditsTimer = -1f;
		public bool alwaysShow;
		private int startingFontSize;
		private int newFontSize;
		private bool fontChangeUp = false;
		private float fontChangeTimer = -1f;
		private const float TOTAL_SIZE_CHANGE_TIME = 1f;
		private Color newColor = Color.red;
		private Color startingColor;

		// Use this for initialization
		public override void Start() {
			base.Start();

			creditsText = gameObject.GetComponent<Text>();
			creditsText.text = "";
			startingFontSize = creditsText.fontSize;
			startingColor = creditsText.color;
			newFontSize = startingFontSize * 2;
		}

		protected override void CreateEventHandlers() {
			base.CreateEventHandlers();
			AddModeEventHandler("Evt_TextCredits", TextCreditsEventHandler);
			AddModeEventHandler("Evt_NoCreditAvailable", NoCreditAvailableEventHandler);
		}

		// Update is called once per frame
		public override void Update() {
			base.Update();

			if (creditsTimer > 0f) {
				creditsTimer -= Time.deltaTime;
				if (creditsTimer <= 0f) {
					creditsText.text = "";
				}
			}

			if (fontChangeTimer > 0f) {
				fontChangeTimer -= Time.deltaTime;
				if (fontChangeTimer <= 0f) {
					if (fontChangeUp) {
						fontChangeUp = false;
						fontChangeTimer = TOTAL_SIZE_CHANGE_TIME / 2;
					}
				}

				if (fontChangeUp) {
					creditsText.fontSize = (int)Mathf.Lerp(startingFontSize, newFontSize, ((TOTAL_SIZE_CHANGE_TIME / 2 - fontChangeTimer) / (TOTAL_SIZE_CHANGE_TIME / 2)));
					creditsText.color = Color.Lerp(startingColor, newColor, ((TOTAL_SIZE_CHANGE_TIME / 2 - fontChangeTimer) / (TOTAL_SIZE_CHANGE_TIME / 2)));
				}
				else {
					creditsText.fontSize = (int)Mathf.Lerp(startingFontSize, newFontSize, fontChangeTimer / (TOTAL_SIZE_CHANGE_TIME / 2));
					creditsText.color = Color.Lerp(startingColor, newColor, fontChangeTimer / (TOTAL_SIZE_CHANGE_TIME / 2));
				}
			}
		}

		public void TextCreditsEventHandler(string evtName, object evtData) {
			creditsText.text = (string)evtData;
			if (!alwaysShow) {
				creditsTimer = 2f;
			}
		}

		public void NoCreditAvailableEventHandler(string evtName, object evtData) {
			fontChangeTimer = TOTAL_SIZE_CHANGE_TIME / 2;
			fontChangeUp = true;
			if (!alwaysShow) {
				creditsTimer = 2f;
			}
		}
	}
}