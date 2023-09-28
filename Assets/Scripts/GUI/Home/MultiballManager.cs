using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PinballClub.TestGame.Modes;
using Multimorphic.P3App.GUI;

namespace PinballClub.TestGame.GUI {

	public class MultiballManager : P3Aware {

		private GameObject smoke;
		private GameObject multiballStatus;
		private GameObject multiballSuperDuperJackpotLit;
		private float multiballFrameCounter = 0;
		private float multiballCalloutTimer;
		private string multiballCalloutName;
		private float multiballStatusKillTimer;

		private MultiballStatusDisplay display;
		private MultiballStatusStruct mbStatus;

		private int jackpotsBeforeCmd;
		private int jackpotsBeforeCmdSector;

		private const float MULTIBALL_CALLOUT_DELAY = 0.653f;
		private const float MULTIBALL_DELAY_BEFORE_POW = 0.5f;
		private const float MULTIBALL_DELAY_BETWEEN_POW = 0.25f;

		// Use this for initialization
		public override void Start () {
			base.Start();

			smoke = GameObject.Find ("Smoke");
		}

		protected override void CreateEventHandlers() {
			base.CreateEventHandlers ();
			AddModeEventHandler("Evt_MultiballJackpot", MultiballJackpotEventHandler);	
			AddModeEventHandler("Evt_MultiballSuperDuperJackpotLit", superDuperJackpotLitEventHandler);
			AddModeEventHandler("Evt_MultiballStart", MultiballStartEventHandler);	
			AddModeEventHandler("Evt_MultiballEnd", MultiballEndEventHandler);
			AddModeEventHandler("Evt_MultiballBallNotLocked", MultiballNoLockEventHandler);
			AddModeEventHandler("Evt_MultiballBallLocked", MultiballLockEventHandler);
			AddModeEventHandler("Evt_MultiballBallEject", MultiballEjectEventHandler);
			AddModeEventHandler("Evt_MultiballLockLit", MultiballLockLitEventHandler);

		}
		

		// Update is called once per frame
		public override void Update () {
			base.Update ();

			if (multiballFrameCounter > 0) {
				multiballFrameCounter--;
				if (multiballFrameCounter == 0) {
					display.SetStatus(mbStatus);
				}
			}

			if (multiballStatusKillTimer > 0) {
				multiballStatusKillTimer -= Time.deltaTime;
				if (multiballStatusKillTimer <= 0) {
					Destroy (multiballStatus);
				}
			}

			if (multiballCalloutTimer > 0) {
				multiballCalloutTimer -= Time.deltaTime;
				if (multiballCalloutTimer <= 0) {
					TestGameAudio.Instance.PlaySound3D(multiballCalloutName, gameObject.transform);
				}
			}
		}

		public void MultiballJackpotEventHandler(string eventName, object eventObject) {
			MultiballStatusStruct mbStatus = (MultiballStatusStruct)eventObject;

			string doublePrefix = "";
			if (mbStatus.dbl)
				doublePrefix = "SD_";

			GameObject jackpotPrefab = (GameObject)Instantiate(Resources.Load("Prefabs/Multiball/MultiPowJackpot"));
			MultiPowJackpot multiPowJackpot = jackpotPrefab.GetComponent<MultiPowJackpot>();
			multiPowJackpot.powPrefabName = "Prefabs/Multiball/JACKPOT_POW";
			multiPowJackpot.powSoundName = "MB_Jackpot_Pow_7";
			multiPowJackpot.jackpotSoundName = "MB_Jackpot_a";

			if (mbStatus.superDuper) {
				multiPowJackpot.numPows = 20;
				multiPowJackpot.timeBetweenPows = 0.10f;
				multiPowJackpot.jackpotPrefabName = "Prefabs/Multiball/JACKPOT_" + doublePrefix + "SUPERDUPER";
				multiPowJackpot.extraPow = true;
				multiPowJackpot.extraPowDelay = 1.5f;
			}
			else if (mbStatus.super) {
				multiPowJackpot.numPows = 10;
				multiPowJackpot.timeBetweenPows = 0.10f;
				multiPowJackpot.jackpotPrefabName = "Prefabs/Multiball/JACKPOT_" + doublePrefix + "SUPER";
			}
			else {
				multiPowJackpot.numPows = mbStatus.multiplier;
				multiPowJackpot.timeBetweenPows = 0.25f;
				multiPowJackpot.jackpotPrefabName = "Prefabs/Multiball/JACKPOT_" + doublePrefix + "X" + mbStatus.multiplier.ToString();
			}

			ScheduleJackpotCallouts(mbStatus);

			//Instantiate(Resources.Load("Prefabs/Multiball/JACKPOT_" + doublePrefix + "X" + mbStatus.multiplier.ToString()));

			//GameObject scorePopup = (GameObject)Instantiate(Resources.Load("Prefabs/Multiball/MultiballJackpotScorePopup"));
			//TextAnimated scorePopupText = scorePopup.GetComponent<TextAnimated>();
			//scorePopupText.text = mbStatus.jackpotScore.ToString("n0");  // score value should be passed in via eventObject
			string ramp;
			if (mbStatus.rightRamp)
				ramp = "rightRamp";
			else
				ramp = "leftRamp";
			popupScores.Spawn("", mbStatus.jackpotScore, ramp, "upperCentral", 1.0f, 1.0f, true );

		}

		private void ScheduleJackpotCallouts(MultiballStatusStruct mbStatus)
		{
			int prevJackpotsRemaining = mbStatus.jackpotsRemaining + mbStatus.multiplier;
			float mbCalloutDelay;

			if (mbStatus.multiplier > 1)
				mbCalloutDelay = MULTIBALL_DELAY_BEFORE_POW + (mbStatus.multiplier-1) * MULTIBALL_DELAY_BETWEEN_POW + MULTIBALL_CALLOUT_DELAY;
			else
				mbCalloutDelay = MULTIBALL_DELAY_BEFORE_POW + MULTIBALL_CALLOUT_DELAY;

			if (!mbStatus.super && mbStatus.jackpotsRemaining <= 0 && prevJackpotsRemaining > 0)
			{
				multiballCalloutTimer = mbCalloutDelay;
				multiballCalloutName = "AMB_AreaClearedPrepare";
			}
			else if (mbStatus.jackpotsRemaining <= 10 && prevJackpotsRemaining > 10)
			{
				multiballCalloutTimer = mbCalloutDelay;
				multiballCalloutName = "AMB_10AgentsToGo";
			}
			else if (mbStatus.jackpotsRemaining <= 30 && prevJackpotsRemaining > 30)
			{
				multiballCalloutTimer = mbCalloutDelay;
				multiballCalloutName = "AMB_20AgentsDown";
			}
			else if (mbStatus.jackpotsRemaining <= 40 && prevJackpotsRemaining > 40)
			{
				multiballCalloutTimer = mbCalloutDelay;
				multiballCalloutName = "AMB_40AgentsLeft";
			}
			else
			{
				jackpotsBeforeCmdSector--;

				if (jackpotsBeforeCmdSector == 0)
				{
					jackpotsBeforeCmdSector = Random.Range(4,6);
					multiballCalloutTimer = mbCalloutDelay;
					multiballCalloutName = "AMB_AgentCmdSector";
				}
				else {
					jackpotsBeforeCmd--;
					if (jackpotsBeforeCmd == 0)
					{
						jackpotsBeforeCmd = Random.Range(2,4);
						multiballCalloutTimer = mbCalloutDelay;
						multiballCalloutName = "AMB_AgentCmd";
					}
				}
			}
		}

		public void superDuperJackpotLitEventHandler(string eventName, object eventObject) {
			bool on = (bool)eventObject;
			if (on) {
				multiballSuperDuperJackpotLit = (GameObject) Instantiate(Resources.Load("Prefabs/Multiball/SDJ_Lit"));
				TestGameAudio.Instance.PlaySound3D("SuperDuperJackpotLit", gameObject.transform);
			}
			else
				Destroy (multiballSuperDuperJackpotLit);
		}
			
		public void MultiballStartEventHandler(string eventName, object eventObject) {
			MultiballStatusStruct status = (MultiballStatusStruct)eventObject;
            // TestGameAudio.Instance.RefillSoundGroupPool("SaucerEject_Fire");
			Instantiate(Resources.Load("Prefabs/Multiball/MULTIBALL_0" + status.totalBalls.ToString()));
			multiballStatus = (GameObject)Instantiate(Resources.Load("Prefabs/Multiball/MultiballStatusWindow"));
			display = multiballStatus.GetComponent<MultiballStatusDisplay>();
			mbStatus = status;
			multiballFrameCounter = 2;

			jackpotsBeforeCmd = Random.Range(2,3);
			jackpotsBeforeCmdSector = Random.Range(4,5);

			smoke.SetActive(false);
		}

		public void MultiballEndEventHandler(string eventName, object eventObject) {
			multiballStatusKillTimer = 5.0f;
			smoke.SetActive(true);
		}

		public void MultiballNoLockEventHandler(string eventName, object eventObject) {
			TestGameAudio.Instance.PlaySound3D("Saucer_Ball_NoLock", gameObject.transform);
		}

		public void MultiballLockEventHandler(string eventName, object eventObject) {
			TestGameAudio.Instance.PlaySound3D("Saucer_Ball_Lock", gameObject.transform);
			Instantiate(Resources.Load("Prefabs/Multiball/BallLocked_0" + (int)eventObject));
		}

		public void MultiballEjectEventHandler(string eventName, object eventObject) {
			TestGameAudio.Instance.PlaySound3D("SaucerEject_Fire", gameObject.transform);
		}

		public void MultiballLockLitEventHandler(string eventName, object eventObject) {
			TestGameAudio.Instance.PlaySound3D("LockIsLit", gameObject.transform);
			Instantiate(Resources.Load("Prefabs/Multiball/LockIsLit"));
		}



	}
}
