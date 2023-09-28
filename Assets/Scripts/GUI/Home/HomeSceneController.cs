using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Multimorphic.P3App.Modes;
using Multimorphic.P3App.GUI;
using PinballClub.TestGame.Modes;
using Multimorphic.P3App.Logging;

namespace PinballClub.TestGame.GUI {
	public class HomeSceneController : TestGameSceneController {

		private bool outroInProgress = false;

		private GameObject MultiballManagerObject;
		private GameObject ScoopAwardManagerObject;
		private GameObject AlienAttackManagerObject;

		private GameObject modeSummary;
		private GameObject HUDObject;

		//private ApproachingHUD approachingHUD;
		protected GameObject launchBallObject;
		private float modeSummaryKillTimer;
		private bool launchSide;
		private float VUKStageTimer;

		private ModeSummaryDisplay modeSummaryDisplay;

		// Use this for initialization
		public override void Start () {
			base.Start();

            // Start the default music track
            if (TestGameAudio.Instance)
                TestGameAudio.Instance.ChangePlaylistByName(sceneName);
			// Now instantiate the music manager.  It will handle necessary music changes.
			MultiballManagerObject = new GameObject();
			MultiballManagerObject.AddComponent<MultiballManager>();

			HUDObject = GameObject.Find ("HUD");
		}

		protected override void CreateEventHandlers() {
			base.CreateEventHandlers ();
			AddModeEventHandler("Evt_SceneOutro", SceneOutroEventHandler);	
			AddModeEventHandler("Evt_SideTargetHit", SideTargetHitEventHandler);
			AddModeEventHandler("Evt_SideTargetMiss", SideTargetMissEventHandler);
			AddModeEventHandler("Evt_LaunchSide", LaunchSideEventHandler);
			AddModeEventHandler("Evt_ShowLaunchBall", ShowLaunchBallEventHandler);
			AddModeEventHandler("Evt_RemoveLaunchBall", HideLaunchBallEventHandler);
			AddModeEventHandler("Evt_PopEscapeHit", PopEscapeHitEventHandler);
			AddModeEventHandler("Evt_ShowLightMode", ShowLightModeEventHandler);
			AddModeEventHandler("Evt_ModeSummary", ModeSummaryEventHandler);
			AddModeEventHandler("Evt_CloseModeSummary", CloseModeSummaryEventHandler);
		}
		

		// Update is called once per frame
		public override void Update () {
			base.Update ();

			// Check for outro completion
			if (outroInProgress) {
				bool outroIsDone = true; // if you have an outro, this is where it needs to be checked for completion.
				if (outroIsDone) {       
					HUDObject.SetActive(true);
					PostGUIEventToModes("Evt_SceneOutroComplete", null);
					Multimorphic.P3App.Logging.Logger.Log ("Finished outro");
				}
			}

			// If the mode summary is complete, destroy it.
			if (modeSummaryKillTimer > 0) {
				modeSummaryKillTimer -= Time.deltaTime;
				if (modeSummaryKillTimer <= 0) {
					Destroy (modeSummary);
				}
			}

			// TODO    Is this generic?
			if (VUKStageTimer > 0) {
				VUKStageTimer -= Time.deltaTime;
				if (VUKStageTimer <= 0) {
					TestGameAudio.Instance.PlaySound3D("VUKRampUp", this.GetSidePos(launchSide));
				}
			}
		}

		public void SideTargetHitEventHandler(string eventName, object eventObject) {
			PlayEdgeSound("SideTargetUnlit", (bool)eventObject);
		}
		
		public void SideTargetMissEventHandler(string eventName, object eventObject) {
			PlayEdgeSound("SideTargetLit", (bool)eventObject);
		}

		/// <summary>
		/// Event handler to start the scene outro.
		/// </summary>
		/// <param name="eventName">Event name.</param>
		/// <param name="eventObject">Event object.  May contain data relevant to the outro.</param>
		public virtual void SceneOutroEventHandler(string eventName, object eventObject) {
			string data = "";  // (string)eventObject;
			SceneOutro(data);
		}

		public void SceneOutro(string sceneName) {
			HUDObject.SetActive(false);
		}

		public void HideLaunchBallEventHandler(string eventName, object eventObject) {
			Destroy(launchBallObject);
		}
		
		public void ShowLaunchBallEventHandler(string eventName, object eventObject) {
			//int num = (int)eventObject;
			//launchBallObject = (GameObject)Instantiate(Resources.Load("Prefabs/PlayerXLaunch/Launch_Player" + (num+1).ToString()));

			string name = (string) eventObject;
			launchBallObject = (GameObject)Instantiate(Resources.Load("Prefabs/PlayerXLaunch/Launch_Player"));
			TextMesh playerMesh = (TextMesh) launchBallObject.transform.Find("FlashingText/PlayerName").gameObject.GetComponent<TextMesh>();  
			playerMesh.text = name;
		}
		
		public void ShowLightModeEventHandler(string eventName, object eventObject) {
			Instantiate(Resources.Load("Prefabs/NoModeStart"));
			TestGameAudio.Instance.PlaySound3D("TractorBeamBallToVUKs", gameObject.transform);
			VUKStageTimer = 1.5f;
		}

		public void PopEscapeHitEventHandler(string eventName, object eventObject) {
			Instantiate(Resources.Load("Prefabs/NoModeStart"));
			// Don't show text.  It confuses people.
			// Instantiate(Resources.Load("Prefabs/NoModeStart_Text"));
			TestGameAudio.Instance.PlaySound3D("TractorBeamBallToVUKs", gameObject.transform);
			VUKStageTimer = 1.5f;
		}

		public void LaunchSideEventHandler(string eventName, object eventObject) {
			launchSide = ((string)eventObject) == "right";
		}

		public void ModeSummaryEventHandler(string eventName, object eventObject) {
			TestGameAudio.Instance.PlaySound3D("ModeSummary", gameObject.transform);
			ModeSummary modeSummaryData = (ModeSummary)eventObject;
			modeSummary = (GameObject)Instantiate(Resources.Load("Prefabs/ModeSummaryWindow"));
			modeSummaryDisplay = modeSummary.GetComponent<ModeSummaryDisplay>();

			modeSummaryDisplay.SetTitle(modeSummaryData.Title);
			for (int i=0; i<3; i++)
			{
				modeSummaryDisplay.SetItemText(i, modeSummaryData.Items[i], modeSummaryData.Values[i]);
			}

			if (modeSummaryData.useCompleted) {
				modeSummaryDisplay.SetComplete(modeSummaryData.Completed);
			}
        }

		private void CloseModeSummaryEventHandler(string evtName, object evtData)
		{
			TestGameAudio.Instance.PlaySound3D("ModeSummaryClose", gameObject.transform);
			modeSummaryKillTimer = 2.0f;
		}

	}
}
