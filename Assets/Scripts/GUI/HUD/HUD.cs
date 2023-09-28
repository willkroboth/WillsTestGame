using UnityEngine;
using System.Collections;
using PinballClub.TestGame.Modes;
using Multimorphic.P3App.GUI;

namespace PinballClub.TestGame.GUI {

	public class HUD : P3Aware {

		public const int NUM_MODES = 6;
		private GameObject [] ModeHighlights;

		private GameObject [] OnIcons;
		private GameObject [] OffIcons;

		private GameObject [,] Chevrons;
		public const int Left_Loop_CHEVRON_INDEX = 0;
		public const int Left_Ramp_CHEVRON_INDEX = 0;
		public const int Saucer_CHEVRON_INDEX = 0;
		public const int Center_CHEVRON_INDEX = 0;
		public const int Right_Ramp_CHEVRON_INDEX = 0;
		public const int Mode_Hole_CHEVRON_INDEX = 0;

		// Use this for initialization
		public override void Start () {
			// Find the game objects before Start() is called, just in case start 
			// causes something to happen that actually uses the objects.
			InitModeHighlights();
			base.Start();
		}

		private void InitModeHighlights() {

			ModeHighlights = new GameObject[NUM_MODES];

            //for (int i=0; i<NUM_MODES; i++)
            //	if (ModeHighlights[i] == null) 
            //    Multimorphic.P3App.Logging.Logger.Log ("object " + i.ToString() + "is null");
			//else
			//	Multimorphic.P3App.Logging.Logger.Log ("object " + i.ToString() + "is not null");
		}

		public override void SetupGameItems() {
			base.SetupGameItems ();
			AddModeEventHandler("Evt_HUDClear", ClearEventHandler);
			AddModeEventHandler("Evt_SceneOutro", SceneOutroEventHandler);	
		}
		
		// Update is called once per frame
		public override void Update () {
			base.Update ();
		}

		public void ClearEventHandler(string eventName, object eventData) {
			foreach (Renderer item in gameObject.GetComponent<Transform>().GetComponentsInChildren<Renderer>())
			{
				item.enabled = false;
			}
		}
		
		public void HighlightModeEventHandler(string eventName, object eventData) {
			int newMode = (int)eventData;

			// TODO: Figure out why ModeHighlights are becoming null *after* InitModeHighlights is run in Start().
			// Then Remove this.  
			InitModeHighlights();

			for (int i=0; i<NUM_MODES; i++)
			{
				ModeHighlights[i].GetComponent<Renderer>().enabled = false;
			}
			//modeHighlights[newMode].SetActive(true);
			if (newMode >= 0)
				ModeHighlights[newMode].GetComponent<Renderer>().enabled = true;
		}

		public virtual void SceneOutroEventHandler(string eventName, object eventObject) {
		}

		protected override void OnDestroy ()
		{
			RemoveModeEventHandler("Evt_HUDClear", ClearEventHandler);
			RemoveModeEventHandler("Evt_HighlightMode", HighlightModeEventHandler);
			RemoveModeEventHandler("Evt_SceneOutro", SceneOutroEventHandler);	
			base.OnDestroy ();
		}
	}
}
