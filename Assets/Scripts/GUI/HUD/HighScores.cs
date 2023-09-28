using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Multimorphic.P3App.Modes.Data;
using Multimorphic.P3App.Modes;

namespace PinballClub.TestGame.GUI {


	public class HighScores : BonusBackgroundBase {

		string description;
		List<HighScoreDisplayItem> scoreItems;

		List<string> scoreLines;
		List<string> nameLines;

		// Use this for initialization
		public override void Start () {
			base.Start();
		}

		protected override void CreateEventHandlers() {
			base.CreateEventHandlers ();
			AddModeEventHandler("Evt_ShowHighScores", ShowHighScoresEventHandler);
			AddModeEventHandler("Evt_HideHighScores", HideHighScoresEventHandler);
		}

		// Update is called once per frame
		public override void Update () {
			base.Update ();
		}

		private void ShowTitle()
		{
			titleTextMesh[0].color = Color.red;
			titleTextMesh[0].text = "Champions";
			titleTextMesh[0].GetComponent<Renderer>().enabled = true;
			titleTextMesh[1].GetComponent<Renderer>().enabled = true;
			titleTextMesh[2].GetComponent<Renderer>().enabled = true;
		}

		private void ShowDescription()
		{
			subTitleTextMesh.text = description;
			subTitleTextMesh.GetComponent<Renderer>().enabled = true;
			subTitleSpacer.GetComponent<Renderer>().enabled = true;
		}

		private void PrepareDisplayData()
		{
			scoreLines = new List<string>();
			nameLines = new List<string>();
			string color;
			
			for (int i=0; i<scoreItems.Count; i++) {
				if (scoreItems[i].highlighted)
					color = "FFFF00FF";
				else
					color = "FFFFFFFF";

				nameLines.Add ("<color=#" + color + ">" + (i+1).ToString() + "     " + scoreItems[i].name + "</color>");
				scoreLines.Add ("<color=#" + color + ">" + scoreItems[i].value.ToString("n0") + "</color>");
			}
		}
		
		private void UpdateDisplay() {
			
			string leftCol = "";
			string rightCol = "";
			
			for (int i=0; i<nameLines.Count; i++)
			{
				leftCol += nameLines[i] + "\n"; 
				rightCol += scoreLines[i] + "\n";
			}

			bonusDataDescTextMesh[1].text = leftCol;
			bonusDataDescTextMesh[1].GetComponent<Renderer>().enabled = true;
			
			bonusDataValueTextMesh[1].text = rightCol;
			bonusDataValueTextMesh[1].GetComponent<Renderer>().enabled = true;
		}

		public void ShowHighScoresEventHandler(string eventName, object eventData) {
			HighScoreDisplayData data = (HighScoreDisplayData)eventData;
			description = data.title;
			scoreItems = data.GetHighScoreItems();
			gameObject.GetComponent<Renderer>().enabled = true;
			ShowTitle();
			ShowDescription();
			PrepareDisplayData();
			UpdateDisplay();
		}

		public void HideHighScoresEventHandler(string eventName, object eventData) {
			DeactivateDisplay();
		}

	}
}
