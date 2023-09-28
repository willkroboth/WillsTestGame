using UnityEngine;
using System.Collections;
using Multimorphic.P3App.GUI;

namespace PinballClub.TestGame.GUI {

	public class TestGamePopupScore : PopupScore {
		const float FADE_IN_DURATION = 0.1f;
		const float FADE_OUT_DURATION = 0.5f;
		const float SCALE_INCREMENT_PER_SCOREBAND = 0.40f;
		const int CAPTION = 0;
		const int CAPTION_SHADOW = 1;
		const int SCORE = 2;
		const int SCORE_SHADOW = 3;
		float targetSize;
		TextMesh[] texts;

		void Awake() {
			texts = new TextMesh[4];
		}

		// Use this for initialization
		public override void Start () {
			base.Start();
		}
		
		// Update is called once per frame
		public override void Update () {
			base.Update();

			gameObject.transform.localScale = Vector3.Lerp(gameObject.transform.localScale, Vector3.one, this.elapsed / FADE_IN_DURATION);

			if (texts[SCORE])
				texts[SCORE].characterSize = Mathf.Lerp(texts[SCORE].characterSize, targetSize, this.progress);
			if (texts[SCORE_SHADOW])
				texts[SCORE_SHADOW].characterSize = Mathf.Lerp(texts[SCORE_SHADOW].characterSize, targetSize, this.progress);

			if (this.remaining < FADE_OUT_DURATION) {
				// fade out
				foreach(TextMesh text in texts) {
					if (text) {
						Color targetColor = text.color;
						targetColor.a = 0;
						text.color = Color.Lerp(text.color, targetColor, 1f - (this.remaining / FADE_IN_DURATION));
					}
				}
			}  
		}

		public override void AfterInitialization() {
			base.AfterInitialization();

			GameObject captionObject = gameObject.transform.Find ("Caption").gameObject;
			if (captionObject != null) {
				texts[CAPTION] = captionObject.GetComponent<TextMesh> ();
				texts[CAPTION].text = this.caption;
			}

			captionObject = gameObject.transform.Find("CaptionShadow").gameObject;
			if (captionObject != null) {
				texts[CAPTION_SHADOW] = captionObject.GetComponent<TextMesh> ();
				texts[CAPTION_SHADOW].text = this.caption;
			}

			GameObject scoreObject = gameObject.transform.Find ("Score").gameObject;
			if (scoreObject != null) {
				texts[SCORE] = scoreObject.GetComponent<TextMesh> ();
				if (score > 0)
					texts[SCORE].text = this.score.ToString ("n0");
				else
					texts[SCORE].text = "";
				targetSize = texts[SCORE].characterSize * (1f + this.scoreBand * SCALE_INCREMENT_PER_SCOREBAND);
			}
			
			scoreObject = gameObject.transform.Find ("ScoreShadow").gameObject;
			if (scoreObject != null) {
				texts[SCORE_SHADOW] = scoreObject.GetComponent<TextMesh> ();
				if (score > 0)
					texts[SCORE_SHADOW].text = this.score.ToString ("n0");
				else
					texts[SCORE_SHADOW].text = "";
				targetSize = texts[SCORE_SHADOW].characterSize * (1f + this.scoreBand * SCALE_INCREMENT_PER_SCOREBAND);
			}
			
			gameObject.transform.localScale = Vector3.one * 0.1f;
		}
		
	}
}
