using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Multimorphic.P3App.Modes;
using Multimorphic.P3App.GUI;

namespace PinballClub.TestGame.GUI {

	public class BonusBackgroundBase : P3Aware {

		protected const int NUM_DEFINED_LINES = 2;

		protected TextMesh [] bonusDataDescTextMesh = new TextMesh[NUM_DEFINED_LINES];
		protected TextMesh [] bonusDataScoreTextMesh = new TextMesh[NUM_DEFINED_LINES];
		protected TextMesh [] bonusDataValueTextMesh = new TextMesh[NUM_DEFINED_LINES];
		protected TextMesh [] titleTextMesh = new TextMesh[3];
		protected MeshRenderer subTitleSpacer;
		protected TextMesh subTitleTextMesh;

		// Use this for initialization
		public override void Start () {
			base.Start();

			string titleObjectName = "Title";
			for (int i=0; i<3; i++) {
				titleTextMesh[i] = (TextMesh) gameObject.transform.Find(titleObjectName).gameObject.GetComponent<TextMesh>();
				titleObjectName += "/Title";
			}

			subTitleTextMesh = (TextMesh) gameObject.transform.Find("SubTitle").gameObject.GetComponent<TextMesh>();
			subTitleSpacer = (MeshRenderer) gameObject.transform.Find("SubTitle/BonusSummary_Spacer").gameObject.GetComponent<MeshRenderer>();

			for (int i=1; i<NUM_DEFINED_LINES+1; i++) {
				bonusDataDescTextMesh[i-1] = (TextMesh) gameObject.transform.Find("Body" + i.ToString()).gameObject.GetComponent<TextMesh>();
				bonusDataScoreTextMesh[i-1] = (TextMesh) gameObject.transform.Find("Body" + i.ToString() + "/Score" + i.ToString()).gameObject.GetComponent<TextMesh>();
				bonusDataValueTextMesh[i-1] = (TextMesh) gameObject.transform.Find("Body" + i.ToString() + "/Value" + i.ToString()).gameObject.GetComponent<TextMesh>();
			}
		}

		protected override void CreateEventHandlers() {
			base.CreateEventHandlers ();
		}

		// Update is called once per frame
		public override void Update () {
			base.Update ();
		}

		protected virtual void DeactivateDisplay()
		{
			gameObject.GetComponent<Renderer>().enabled = false;
			for (int i=0; i<3; i++)
				titleTextMesh[i].GetComponent<Renderer>().enabled = false;
			subTitleTextMesh.GetComponent<Renderer>().enabled = false;
			subTitleSpacer.GetComponent<Renderer>().enabled = false;
			for (int i=0; i<NUM_DEFINED_LINES; i++) {
				bonusDataDescTextMesh[i].GetComponent<Renderer>().enabled = false;
				bonusDataValueTextMesh[i].GetComponent<Renderer>().enabled = false;
				bonusDataScoreTextMesh[i].GetComponent<Renderer>().enabled = false;
			}
		}
	}
}
