using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Multimorphic.P3App.Modes;
using PinballClub.TestGame.Modes;
using Multimorphic.P3App.GUI;
using Multimorphic.P3App.Data;
using Multimorphic.P3App.Logging;

namespace PinballClub.TestGame.GUI {
	public class TestGameSceneController : SceneController {
		
		const string APP_CODE = "TestGame";

		private bool shiftPressed;

		protected bool IntroActive;
		protected GameObject IntroObject;
		protected float IntroFinishTime;
		const float INTRO_FINISH_DELAY = 4.0f;

		private int noCreditCount;

		protected GameObject addedPlayerObject;
		private GameObject leftShotCounter, rightShotCounter;
		private GameObject buttonLegend;
		private GameObject confirmationBox;

		private bool highScoreEntryActive;

		private GameObject BallSaveManagerObject;
		private GameObject LanesManagerObject;

		protected string popBumperSound = "Pop"; 
		protected string slingshotSound = "Slingshot"; 

		public override void Awake() {
			appCode = APP_CODE;
			base.Awake();
			shiftPressed = false;
		}

        // Use this for initialization
        public override void Start() {
            base.Start();

            BallSaveManagerObject = new GameObject();
            BallSaveManagerObject.AddComponent<BallSaveManager>();

            LanesManagerObject = new GameObject();
            LanesManagerObject.AddComponent<LanesManager>();

            highScoreEntryActive = false;
            IntroActive = false;
            noCreditCount = 0;
            if (TestGameAudio.Instance != null)
            {
                TestGameAudio.Instance.StopAllPlaylists();
                TestGameAudio.Instance.ChangePlaylistByName("BG_Mode_Introduction");
            }
		}

		protected override void SceneLive() {
			base.SceneLive();
			PlayIntroEventHandler("name", 0);
		}

			
		protected override void CreateEventHandlers() {
			base.CreateEventHandlers ();
			AddModeEventHandler("Evt_SceneCompleted", CompleteEventHandler);	
			AddModeEventHandler("Evt_ScoringX", ScoringXEventHandler);

			//AddModeEventHandler("Evt_PlayIntro", PlayIntroEventHandler);
			AddModeEventHandler("Evt_FlipperAction", FlipperHitEventHandler);
			AddModeEventHandler("Evt_BallLaunched", BallLaunchedEventHandler);
			AddModeEventHandler("Evt_NewCoin", CoinAddedEventHandler);
			AddModeEventHandler("Evt_NewCredit", CreditAddedEventHandler);
			AddModeEventHandler("Evt_NoCredit", NoCreditEventHandler);
			AddModeEventHandler("Evt_PlaySceneInstruction", PlaySceneInstructionEventHandler);
			AddModeEventHandler("Evt_ShowPlayerAdded", ShowPlayerAddedEventHandler);
			AddModeEventHandler("Evt_ShowPlayerRemoved", ShowPlayerRemovedEventHandler);
			AddModeEventHandler("Evt_RightRampInc", RightRampHitEventHandler);
			AddModeEventHandler("Evt_LeftRampInc", LeftRampHitEventHandler);
			AddModeEventHandler("Evt_NullTargetHit", NullTargetHitEventHandler);
			AddModeEventHandler("Evt_SlingshotHit", SlingshotHitEventHandler);
			AddModeEventHandler("Evt_PopBumperHit", PopBumperHitEventHandler);
			AddModeEventHandler("Evt_SideTargetScore", SideTargetScoreEventHandler);
			AddModeEventHandler("Evt_Skillshot", SkillshotEventHandler);
			AddModeEventHandler("Evt_AwardRespawn", AwardRespawnEventHandler);

			AddModeEventHandler("Evt_ShowButtonLegend", ShowButtonLegendEventHandler);	
			AddModeEventHandler("Evt_HideButtonLegend", HideButtonLegendEventHandler);	

			AddModeEventHandler("Evt_ShowConfirmationBox", ShowConfirmationBoxEventHandler);	
			AddModeEventHandler("Evt_HideConfirmationBox", HideConfirmationBoxEventHandler);	
			AddModeEventHandler("Evt_DeleteProfile", DeleteProfileEventHandler);
			AddModeEventHandler("Evt_CreateProfile", CreateProfileEventHandler);

			AddModeEventHandler("Evt_TeamGameEnabled", TeamGameEnabledEventHandler);
			AddModeEventHandler("Evt_HighScoreAchieved", HighScoreAchievedEventHandler);
            AddModeEventHandler("Evt_Cheater", CheaterEventHandler);
            AddModeEventHandler("Evt_EnableBlackout", EnableBlackoutEventHandler);
            AddModeEventHandler("Evt_DisableBlackout", DisableBlackoutEventHandler);
            AddModeEventHandler("Evt_EnableReverseFlippers", EnableReverseFlippersEventHandler);
            AddModeEventHandler("Evt_DisableReverseFlippers", DisableReverseFlippersEventHandler);
            AddModeEventHandler("Evt_EnableInvertFlippers", EnableInvertFlippersEventHandler);
            AddModeEventHandler("Evt_DisableInvertFlippers", DisableInvertFlippersEventHandler);
        }

        public override void Update () {
			base.Update();

			// Use shift to isolate GUI keyboard events from NetProcMachine keyboard events.
			if ( Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift)) 
			    shiftPressed = true;
			if ( Input.GetKeyUp(KeyCode.LeftShift)  || Input.GetKeyUp(KeyCode.RightShift)) 
			    shiftPressed = false;

			if (shiftPressed) {
			}

			if (IntroActive && (Time.time > IntroFinishTime)) {
				Multimorphic.P3App.Logging.Logger.Log("Calling EndIntro Naturally " + sceneName);
				EndIntro();
	         }
		}

		public virtual void CompleteEventHandler(string eventName, object eventObject) {
			SceneCompleteInfo info = (SceneCompleteInfo)eventObject;
			popupScores.Spawn("", info.score, "upperCentral", "playableLCDCenter", 3.0f, 0.5f);
		}

		public virtual void PlayIntroEventHandler(string eventName, object eventObject) {
			IntroActive = true;
			string resourceName = "Prefabs/" + sceneName + "/ModeIntro"; 
			Object resource = Resources.Load(resourceName);
			if (resource != null)
				IntroObject = (GameObject)Instantiate(resource);
			else
				Multimorphic.P3App.Logging.Logger.Log ("Did not find " + resourceName);
			IntroFinishTime = Time.time + INTRO_FINISH_DELAY;
		}

		protected virtual void EndIntro() {
			IntroActive = false;
			Destroy(IntroObject);
			PostGUIEventToModes("Evt_IntroComplete", null);
            if (TestGameAudio.Instance != null)
			    TestGameAudio.Instance.ChangePlaylistByName(sceneName);
		}

		protected virtual void PlayIntroStage() {
		}
			
		public void FlipperHitEventHandler(string eventName, object eventObject) {
			if (IntroActive)
			{
				Multimorphic.P3App.Logging.Logger.Log("Flipper to EndIntro " + sceneName);
				EndIntro();
			}
		}


		public void ScoringXEventHandler(string eventName, object eventObject) {
			TestGameAudio.Instance.PlaySound3D("2X_Scoring", gameObject.transform);
			//TestGameAudio.Instance.ChangePlaylistByName("Multiball");
			int value = (int)eventObject;
			if ((value == 2) || (value == 3))
			{
				Instantiate(Resources.Load("Prefabs/X_Scoring_" + value.ToString() + "X"));
			}
		}

		public void CreditAddedEventHandler(string eventName, object eventObject) {
			TestGameAudio.Instance.PlaySound3D("Credits_New_Credit", gameObject.transform);
		}

		public void CoinAddedEventHandler(string eventName, object eventObject) {
			TestGameAudio.Instance.PlaySound3D("AddMoneyPartialCredit", gameObject.transform);
		}

		public void NoCreditEventHandler(string eventName, object eventObject) {
			if (noCreditCount < 3) {
				TestGameAudio.Instance.PlaySound3D("Credits_No_Credit", gameObject.transform);
				noCreditCount++;
			}
		}

		public void BallLaunchedEventHandler(string eventName, object eventObject) {
			TestGameAudio.Instance.PlaySound3D("Ball_Launch", gameObject.transform);
		}

		public void ShowPlayerAddedEventHandler(string eventName, object eventObject) {
			string name = (string)eventObject;
			Destroy(addedPlayerObject);
			TestGameAudio.Instance.PlaySound3D("PlayerAdded", gameObject.transform);
			addedPlayerObject = (GameObject)Instantiate(Resources.Load("Prefabs/PlayerXAdded/Added_Player"));
			TextMesh playerMesh = (TextMesh) addedPlayerObject.transform.Find("FlashingText/PlayerName").gameObject.GetComponent<TextMesh>();  
			playerMesh.text = name;
		}

		public void ShowPlayerRemovedEventHandler(string eventName, object eventObject) {
			string name = (string)eventObject;
			Destroy(addedPlayerObject);
			TestGameAudio.Instance.PlaySound3D("PlayerRemoved", gameObject.transform);
			addedPlayerObject = (GameObject)Instantiate(Resources.Load("Prefabs/PlayerXAdded/Removed_Player"));
			TextMesh playerMesh = (TextMesh) addedPlayerObject.transform.Find("FlashingText/PlayerName").gameObject.GetComponent<TextMesh>();  
			playerMesh.text = name;
		}

		public void LeftRampHitEventHandler(string eventName, object eventData) {
			Destroy (leftShotCounter);
			leftShotCounter = (GameObject)Instantiate(Resources.Load("Prefabs/LeftRampCounter"));
			ShotCounterDisplay shotCounter = leftShotCounter.GetComponent<ShotCounterDisplay>();
			shotCounter.count = (int)eventData;
			TestGameAudio.Instance.PlaySound3D("Right_Ramp", gameObject.transform);
		}

		public void RightRampHitEventHandler(string eventName, object eventData) {
			Destroy (rightShotCounter);
			rightShotCounter = (GameObject)Instantiate(Resources.Load("Prefabs/RightRampCounter"));
			ShotCounterDisplay shotCounter = rightShotCounter.GetComponent<ShotCounterDisplay>();
			shotCounter.count = (int)eventData;
			TestGameAudio.Instance.PlaySound3D("Right_Ramp", gameObject.transform);
		}

		public virtual void PlaySceneInstructionEventHandler(string eventName, object eventData) {
		}

		public void NullTargetHitEventHandler(string eventName, object eventData) {
			TestGameAudio.Instance.PlaySound3D("ModeNullHit", gameObject.transform);
		}

		public virtual void SlingshotHitEventHandler(string evtName, object evtData)
		{
			LocationScore info = (LocationScore) evtData;
			bool right = info.location == "slingR";
			PlayEdgeSound(slingshotSound, right);
			popupScores.Spawn("", (int)info.score, info.location, "slingCenter", 1.0f, 0.5f);  // uses named location upperCentral
			if (right)
				Instantiate(Resources.Load("Prefabs/" + sceneName + "/RightSlingShotPopup"));
			else
				Instantiate(Resources.Load("Prefabs/" + sceneName + "/LeftSlingShotPopup"));
		}

		public virtual void PopBumperHitEventHandler(string evtName, object evtData)
		{
			LocationScore info = (LocationScore) evtData;
			popupScores.Spawn("", (int)info.score, info.location, "belowPops", 1.0f, 0.5f, true);  // uses named location upperCentral
			PlayEdgeSound(popBumperSound, true);
			Instantiate(Resources.Load("Prefabs/" + sceneName + "/PopsPopup"));
		}

		public void SideTargetScoreEventHandler(string evtName, object evtData)
		{
			LocationScore info = (LocationScore) evtData;
			popupScores.Spawn("", (int)info.score, info.location, info.location + "Center", 1.0f, 0.5f);  // uses named location upperCentral
		}

		public void SkillshotEventHandler(string evtName, object evtData)
		{
			LocationScore info = (LocationScore) evtData;
			popupScores.Spawn("Skillshot", (int)info.score, info.location, "flipperGap", 2.0f, 0.11f, true);  // uses named location upperCentral
			Instantiate(Resources.Load("Prefabs/AlienAttack/Skillshot_Hit"));
		}

		public void AwardRespawnEventHandler(string evtName, object evtData) {
			Instantiate(Resources.Load("Prefabs/Lanes/Respawn"));
			TestGameAudio.Instance.PlaySound3D("ScoopAwardRespawn", gameObject.transform);
		}

		/// <summary>
		/// Shows the button legend.  Event data contains a dictionary of the captions for the legend.
		/// Valid button names are:          "LeftWhiteButton", "LeftRedButton", "LeftYellowButton",
		///									 "RightWhiteButton", "RightRedButton", "RightYellowButton",
		///									 "StartButton", "LaunchButton"
		/// </summary>
		/// <param name="eventName"></param>
		/// <param name="eventData">A Dictionary<string, string>, where the key is the button name and the value is the caption.</param>
		public void ShowButtonLegendEventHandler(string eventName, object eventData) {
			if (buttonLegend == null) {
				buttonLegend = (GameObject)Instantiate(Resources.Load("Prefabs/Framework/ButtonLegend"));
			}
			ButtonLegend legendScript = buttonLegend.GetComponent<ButtonLegend>();
			legendScript.SetLegend ((Dictionary<string, string>)eventData);
		}

		public void HideButtonLegendEventHandler(string eventName, object eventData) {
			Destroy(buttonLegend);
		}

		public void ShowConfirmationBoxEventHandler(string eventName, object eventData) {
			if (confirmationBox == null) {
				TestGameAudio.Instance.PlaySound3D("ConfirmationPopup", gameObject.transform);
				confirmationBox = (GameObject)Instantiate(Resources.Load("Prefabs/Framework/ConfirmationWindow"));
			}
			ConfirmationBoxStruct data = (ConfirmationBoxStruct)eventData;
			Multimorphic.P3App.Logging.Logger.Log ("TestGame left highlight: " + data.leftHighlight.ToString());
			ConfirmationBox boxScript = confirmationBox.GetComponent<ConfirmationBox>();
			boxScript.SetData (data);
			
			Multimorphic.P3App.Logging.Logger.Log ("ShowConfirmationBox");
		}
		
		public void HideConfirmationBoxEventHandler(string eventName, object eventData) {
			Destroy(confirmationBox);
			Multimorphic.P3App.Logging.Logger.Log ("HideConfirmationBox");
		}

		public void DeleteProfileEventHandler(string eventName, object eventData) {
			TestGameAudio.Instance.PlaySound3D("ProfileDeleted", gameObject.transform);
		}

		public void CreateProfileEventHandler(string eventName, object eventData) {
			TestGameAudio.Instance.PlaySound3D("ProfileCreated", gameObject.transform);
		}

		public void TeamGameEnabledEventHandler(string eventName, object eventData) {
			if ((bool)eventData)
				TestGameAudio.Instance.PlaySound3D("TeamGameEnabled", gameObject.transform);
			else
				TestGameAudio.Instance.PlaySound3D("TeamGameDisabled", gameObject.transform);
		}

		public void HighScoreAchievedEventHandler(string eventName, object eventData) {
			highScoreEntryActive = (bool)eventData;
			if (highScoreEntryActive)
			{
				TestGameAudio.Instance.ChangePlaylistByName("HighScoreEntry");
//				highScoreBackground = (GameObject)Instantiate(Resources.Load("Prefabs/Framework/HighScore"));
			}
		}

		public void CheaterEventHandler(string eventName, object eventData) {
            if (TestGameAudio.Instance)
                TestGameAudio.Instance.PlaySound3D("Cheater", gameObject.transform);
		}

        GameObject BlackoutObj;
        public void EnableBlackoutEventHandler(string eventName, object eventData)
        {
            List<string> names = (List<string>)eventData;
            Destroy(BlackoutObj);
            BlackoutObj = (GameObject)Instantiate(Resources.Load("Prefabs/ViewerInteractions/Blackout"));
            Text playedByText = (Text)BlackoutObj.transform.Find("PlayedBy_Text").GetComponent<Text>();
            playedByText.text = "By\n";
            if (names.Count > 30)
                playedByText.text += "<size=30>";
            else if (names.Count > 20)
                playedByText.text += "<size=45>";
            else
                playedByText.text += "<size=55>";
            foreach (string name in names)
                playedByText.text += name + "\n";
            playedByText.text += "</size>";

            TestGameAudio.Instance.PlaySound("FX/Blackout");
        }

        public void DisableBlackoutEventHandler(string eventName, object eventData)
        {
            Destroy(BlackoutObj);
        }

        public void EnableReverseFlippersEventHandler(string eventName, object eventData)
        {
            List<string> names = (List<string>)eventData;
            GameObject ReverseFlippersObj = (GameObject)Instantiate(Resources.Load("Prefabs/ViewerInteractions/ReverseFlippers"));
            Text reverseText = (Text)ReverseFlippersObj.transform.Find("offset/Text").GetComponent<Text>();
            reverseText.text = "Reversed Flippers!\n\nBy\n";
            if (names.Count > 30)
                reverseText.text += "<size=30>";
            else if (names.Count > 20)
                reverseText.text += "<size=45>";
            else
                reverseText.text += "<size=55>";
            foreach (string name in names)
                reverseText.text += name + "\n";
            reverseText.text += "</size>";

            TestGameAudio.Instance.PlaySound("FX/Reverse_flippers");
        }

        public void DisableReverseFlippersEventHandler(string eventName, object eventData)
        {
            TestGameAudio.Instance.PlaySound("FX/Regular_flippers");
        }

        public void EnableInvertFlippersEventHandler(string eventName, object eventData)
        {
            List<string> names = (List<string>)eventData;
            GameObject InvertFlippersObj = (GameObject)Instantiate(Resources.Load("Prefabs/ViewerInteractions/InverseFlippers"));
            Text invertText = (Text)InvertFlippersObj.transform.Find("offset/Text").GetComponent<Text>();
            invertText.text = "Inverted Flippers!\n\nBy\n";
            if (names.Count > 30)
                invertText.text += "<size=30>";
            else if (names.Count > 20)
                invertText.text += "<size=45>";
            else
                invertText.text += "<size=55>";
            foreach (string name in names)
                invertText.text += name + "\n";
            invertText.text += "</size>";

            TestGameAudio.Instance.PlaySound("FX/Invert_flippers");
        }

        public void DisableInvertFlippersEventHandler(string eventName, object eventData)
        {
            TestGameAudio.Instance.PlaySound("FX/Regular_flippers");
        }

    }
}
