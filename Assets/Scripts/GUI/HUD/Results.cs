using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace PinballClub.TestGame.GUI {


	public class Results : BonusBackgroundBase {

		public int fontSize;
		public int titleFontSize;
		List<long> Scores;
		List<string> Names;
		List<int> Places;

		List<string> scoreLines;
		List<string> placeLines;
		List<string> nameLines;

		public float LeftColWidth = 3.0f;
		public float MidColWidth = 2.0f;
		public float RightColWidth = 2.0f;

		private bool teamMember = false;
		private bool restoredGame = false;

		private const int NUM_LINES = 10;

		// Use this for initialization
		public override void Start () {
			Places = new List<int>();

			base.Start();
		}

		protected override void CreateEventHandlers() {
			base.CreateEventHandlers ();
			AddModeEventHandler("Evt_ShowResults", ShowResultsEventHandler);
			AddModeEventHandler("Evt_HideResults", HideResultsEventHandler);
		}
		
		// Update is called once per frame
		public override void Update () {
			base.Update ();
		}

		private void ShowTitle()
		{
			titleTextMesh[0].color = Color.red;
			titleTextMesh[0].text = "Results";
			titleTextMesh[0].GetComponent<Renderer>().enabled = true;
			titleTextMesh[1].GetComponent<Renderer>().enabled = true;
			titleTextMesh[2].GetComponent<Renderer>().enabled = true;
		}

		private void SetPlaces()
		{
			for (int i=0; i<Scores.Count; i++)
			{
				int place=1;
				for (int j=0; j<Scores.Count; j++)
				{
					if (Scores[j] > Scores[i])
						place++;
						
				}
				Places.Add(place);
			}
		}

		private void PrepareDisplayData()
		{
			scoreLines = new List<string>();
			placeLines = new List<string>();
			nameLines = new List<string>();
			string color;

			for (int i=0; i<Scores.Count; i++) {
				if (Places[i] == 1)
					color = "FFFF00FF";
				else
					color = "FFFFFFFF";

				scoreLines.Add ("<color=#" + color + ">" + Scores[i].ToString("n0") + "</color>");
				placeLines.Add ("<color=#" + color + ">" + Places[i].ToString() + "</color>");
				nameLines.Add ("<color=#" + color + ">" + Names[i] + "</color>");
				if (Names[i].Contains(" * "))
					teamMember = true;
				if (Names[i].Contains (" ** "))
					restoredGame = true;
			}
		}

		private void UpdateDisplay() {
			
			string leftCol = "";
			string midCol = "";
			string rightCol = "";

			int leftSize = bonusDataDescTextMesh[0].fontSize;
			int midSize = bonusDataScoreTextMesh[0].fontSize;
			int rightSize = bonusDataValueTextMesh[0].fontSize;

			for (int i=0; i<Scores.Count; i++)
			{
				leftSize = setMaxFontSize(bonusDataDescTextMesh[0], nameLines[i], leftSize, LeftColWidth);
				midSize = setMaxFontSize(bonusDataScoreTextMesh[0], scoreLines[i], midSize, MidColWidth);
				rightSize = setMaxFontSize(bonusDataValueTextMesh[0], placeLines[i], rightSize, RightColWidth);

				leftCol += nameLines[i] + "\n"; 
				midCol += scoreLines[i] + "\n"; 
				rightCol += placeLines[i] + "\n";
			}

			if (teamMember || restoredGame)
			{
				int numNotes = 0;
				if (teamMember)
					numNotes++;
				if (restoredGame)
					numNotes++;

				for (int i=Scores.Count; i<NUM_LINES-numNotes; i++)
				{
					leftCol += "\n";
				}
				if (teamMember)
					leftCol += "\n<color=#FF0000FF>* : Teams not eligible for high scores.</color>";
				if (restoredGame)
					leftCol += "\n<color=#FF0000FF>** : Restored games not eligible for high scores.</color>";

                float footnoteFontSize = bonusDataDescTextMesh[0].fontSize * 0.66f;
                if (teamMember)
                    leftCol += "\n<size=" + footnoteFontSize.ToString() + "><color=#FF0000FF>* Teams not eligible for high scores.</color></size>";
                if (restoredGame)
                    leftCol += "\n<size=" + footnoteFontSize.ToString() + "><color=#FF0000FF>** Restored games not eligible for high scores.</color></size>";
            }

            int fontSize = GetMin(rightSize, midSize, leftSize);

			bonusDataDescTextMesh[0].fontSize = fontSize;
			bonusDataScoreTextMesh[0].fontSize = fontSize;
			bonusDataValueTextMesh[0].fontSize = fontSize;

			bonusDataDescTextMesh[0].text = leftCol;
			bonusDataDescTextMesh[0].GetComponent<Renderer>().enabled = true;

			bonusDataScoreTextMesh[0].text = midCol;
			bonusDataScoreTextMesh[0].GetComponent<Renderer>().enabled = true;
			
			bonusDataValueTextMesh[0].text = rightCol;
			bonusDataValueTextMesh[0].GetComponent<Renderer>().enabled = true;
		}

		private int GetMin(int a, int b, int c)
		{
			if (a <= b && a <= c)
				return a;
			else if (b <= a && b <= c)
				return b;
			else
				return c;
		}

		private int setMaxFontSize(TextMesh mesh, string text, int startingFontSize, float bounds)
		{
			mesh.text = text;
			mesh.fontSize = startingFontSize;
			while (mesh.GetComponent<Renderer>().bounds.size.z > bounds)
			{
				mesh.fontSize--;
			}
			return mesh.fontSize;
		}

		public void ShowResultsEventHandler(string eventName, object eventData) {
			restoredGame = false;
			teamMember = false;
			OrderedDictionary nameScores = (OrderedDictionary) eventData;
			Scores = new List<long>();
			Names = new List<string>();
			foreach (DictionaryEntry de in nameScores)
			{
				Names.Add ((string)de.Key);
				Scores.Add ((long)de.Value);
			}
			SetPlaces();
			PrepareDisplayData();
			gameObject.GetComponent<Renderer>().enabled = true;
			UpdateDisplay();
			ShowTitle();
		}

		public void HideResultsEventHandler(string eventName, object eventData) {
			DeactivateDisplay();
		}

	}
}
