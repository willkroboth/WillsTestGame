using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Multimorphic.P3App.Modes;
using Multimorphic.P3App.GUI;
using Multimorphic.P3App.Logging;

namespace PinballClub.TestGame.GUI {

	public class Bonus : BonusBackgroundBase {

		protected const int NUM_LINES = 10;

		private BonusInfo BonusData;

		List<string> descriptionLines;
		List<string> valueLines;

		int LineIndex;
		float TimeBetweenLines;
		float NextLineTime;
		public float LeftColWidth = 5.0f;
		public float RightColWidth = 5.0f;

		float RemainVisibleTime;

		// Use this for initialization
		public override void Start () {
			TimeBetweenLines = 0.5f;
			RemainVisibleTime = 3.0f;
			NextLineTime = 0;

			base.Start();
		}

		protected override void CreateEventHandlers() {
			base.CreateEventHandlers ();
			AddModeEventHandler("Evt_SetBonusData", SetBonusDataEventHandler);
			AddModeEventHandler("Evt_AccelerateBonus", AccelerateBonusEventHandler);
		}

		// Update is called once per frame
		public override void Update () {
			base.Update ();

			if (NextLineTime > 0) {
				NextLineTime -= Time.deltaTime;
				if (NextLineTime <= 0) {
					UpdateDisplay();
				}
			}
		}

		private void UpdateDisplay() {

			if (LineIndex < (BonusData.NumItems() + 2)) {
				// Music
				if (LineIndex == BonusData.NumItems() + 1) {
                    TestGameAudio.Instance.ChangePlaylistByName("Bonus_Total");
				}
				
				// FX
				if (LineIndex == BonusData.NumItems()) {
					TestGameAudio.Instance.PlaySound3D("Bonus_X", gameObject.transform);
					Multimorphic.P3App.Logging.Logger.Log ("Bonus_X");
				}
				else if (LineIndex < BonusData.NumItems()) {
					TestGameAudio.Instance.PlaySound3D("Bonus_Dings", gameObject.transform);
				}

				int startIndex;
				if (LineIndex < NUM_LINES) 
					startIndex = 0;
				else 
					startIndex = LineIndex - NUM_LINES;

				string leftCol = "";
				string rightCol = "";

				for (int i=startIndex; i<=LineIndex; i++)
				{
					if (i-startIndex > NUM_LINES)
						break;

					leftCol += descriptionLines[i] + "\n"; 
					rightCol += valueLines[i] + "\n";
				}

				// Make sure both columns use the same font size.
				if (bonusDataDescTextMesh[0].fontSize < bonusDataValueTextMesh[0].fontSize) {
					bonusDataValueTextMesh[0].fontSize = bonusDataDescTextMesh[0].fontSize;
				}
				else {
					bonusDataDescTextMesh[0].fontSize = bonusDataValueTextMesh[0].fontSize;
				}

				bonusDataDescTextMesh[0].text = leftCol;
				bonusDataDescTextMesh[0].GetComponent<Renderer>().enabled = true;
				
				bonusDataValueTextMesh[0].text = rightCol;
				bonusDataValueTextMesh[0].GetComponent<Renderer>().enabled = true;

				if (LineIndex == BonusData.NumItems()+1)
					NextLineTime = RemainVisibleTime;
				else
					NextLineTime = TimeBetweenLines;
				LineIndex++;
			}
			else
				DeactivateDisplay();

		}

		private void SetMaxFontSizes()
		{
			for (int i=0; i<descriptionLines.Count; i++)
			{
				SetMaxFontSize(bonusDataDescTextMesh[0], descriptionLines[i], LeftColWidth);
				SetMaxFontSize(bonusDataValueTextMesh[0], valueLines[i], RightColWidth);
			}
		}

		private void SetMaxFontSize(TextMesh mesh, string text, float bounds)
		{
			mesh.text = text;
			while (mesh.GetComponent<Renderer>().bounds.size.z > bounds)
			{
				mesh.fontSize--;
			}
		}
		
		private void PrepareDisplayData()
		{
			descriptionLines = new List<string>();
			valueLines = new List<string>();
			
			for (int i=0; i<BonusData.NumItems(); i++)
			{
				BonusItem item = BonusData.GetItem(i);
				descriptionLines.Add (item.Description);
				valueLines.Add (item.Total.ToString("n0"));
			}
			
			descriptionLines.Add ("<color=#0000ffff>Multiplier</color>");
			valueLines.Add ("<color=#0000ffff>" + BonusData.GetMultiplier().ToString() + "</color>");
			
			descriptionLines.Add ("<color=#ff0000ff>Total</color>");
			valueLines.Add ("<color=#ff0000ff>" + BonusData.GetTotal().ToString("n0") + "</color>");
		}

		protected override void DeactivateDisplay()
		{
			base.DeactivateDisplay();
			PostGUIEventToModes("Evt_BonusComplete", null);
		}

		public void SetBonusDataEventHandler(string eventName, object eventData) {
			TestGameAudio.Instance.ChangePlaylistByName("Bonus_Start");
			Audio.RefillSoundGroupPool("Bonus_Dings");
			Multimorphic.P3App.Logging.Logger.Log ("SetBonusDataHandler");
			BonusData = (BonusInfo) eventData;
			Multimorphic.P3App.Logging.Logger.Log ("NumBonusItems = " + BonusData.NumItems());
			LineIndex = 0;
			TimeBetweenLines = 0.5f;
			RemainVisibleTime = 3.0f;
			NextLineTime = TimeBetweenLines;
			gameObject.GetComponent<Renderer>().enabled = true;

			titleTextMesh[0].color = Color.red;
			titleTextMesh[0].text = "Bonus";
			titleTextMesh[0].GetComponent<Renderer>().enabled = true;
			titleTextMesh[1].GetComponent<Renderer>().enabled = true;
			titleTextMesh[2].GetComponent<Renderer>().enabled = true;

			PrepareDisplayData();
			SetMaxFontSizes();
		}

		public void AccelerateBonusEventHandler(string eventName, object eventData) {
			TimeBetweenLines = 0.1f;
			RemainVisibleTime = 2.0f;
			float potentialNewNextLineTime = Time.time + TimeBetweenLines;
			if (NextLineTime > potentialNewNextLineTime)
				NextLineTime = potentialNewNextLineTime;
		}

		protected override void OnDestroy ()
		{
			base.OnDestroy ();
		}
	}
}
