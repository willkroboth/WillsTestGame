using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Multimorphic.P3App.Modes;
using PinballClub.TestGame.Modes;
using Multimorphic.P3App.GUI;
using Multimorphic.P3App.Data;

namespace PinballClub.TestGame.GUI {
	public class LanesManager : P3Aware {

		private float awardSoundDelayTimer;
		private string soundName;
		private int glintCounter;
		private float glintDelayTimer;

		private const float AWARD_DELAY_TIME = 1.50f;
		private const float GLINT_DELAY_TIME = 0.25f;
		private const float GLINT_DELAY_START_TIME = 0.01f;
		private const int NUM_GLINTS = 5;

		// Use this for initialization
		public override void Start () {
			base.Start();
		}

		protected override void CreateEventHandlers() {
			base.CreateEventHandlers ();
			AddModeEventHandler("Evt_AnimateLanesRight", AnimateLanesRightEventHandler);
			AddModeEventHandler("Evt_AnimateLanesLeft", AnimateLanesLeftEventHandler);
			AddModeEventHandler("Evt_LanesCompleted", LanesCompletedEventHandler);
			AddModeEventHandler("Evt_LaneAlreadyActivated", LaneAlreadyActivatedEventHandler);
			AddModeEventHandler("Evt_LaneActivated", LaneActivatedEventHandler);
		}

		public override void Update () {
			base.Update();

			if (awardSoundDelayTimer > 0) {
				awardSoundDelayTimer -= Time.deltaTime;
				if (awardSoundDelayTimer <= 0) {
					TestGameAudio.Instance.PlaySound3D(soundName, gameObject.transform);
				}
			}

			if (glintDelayTimer > 0 && glintCounter > 0) {
				glintDelayTimer -= Time.deltaTime;
				if (glintDelayTimer <= 0) {
					TestGameAudio.Instance.PlaySound3D("FX_LaneCompleteGlints", gameObject.transform);
					glintDelayTimer = GLINT_DELAY_TIME;
					glintCounter--;
				}
			}
		}

			//TestGameAudio.Instance.PlaySound3D("Lane_Return_Lit", GetSidePos ((bool)eventObject));
			//TestGameAudio.Instance.PlaySound3D("Lane_Enables", GetSidePos ((bool)eventObject));

		public void AnimateLanesRightEventHandler(string evtName, object evtData) {
			List<bool> lanes = (List<bool>)evtData;
			TestGameAudio.Instance.PlaySound3D("LaneShiftRight", gameObject.transform);
			for (int i=0; i<lanes.Count; i++) {
				//if (lanes[i])
				//	Instantiate(Resources.Load("Prefabs/Lanes/InOutlaneTransitionRight_" + (i+1).ToString()));
			}
			
		}
		
		public void AnimateLanesLeftEventHandler(string evtName, object evtData) {
			TestGameAudio.Instance.PlaySound3D("LaneShiftLeft", gameObject.transform);
			List<bool> lanes = (List<bool>)evtData;
			for (int i=0; i<lanes.Count; i++) {
				//if (lanes[i])
					//Instantiate(Resources.Load("Prefabs/Lanes/InOutlaneTransitionLeft_" + (i+1).ToString()));
			}
			
		}
		
		public void LaneActivatedEventHandler(string evtName, object evtData) {
			int lane = (int)evtData;
			if (lane == 0)
				TestGameAudio.Instance.PlaySound3D("FX_LaneLight_0", gameObject.transform);
			else if (lane == 1)
				TestGameAudio.Instance.PlaySound3D("FX_LaneLight_1", gameObject.transform);
			else if (lane == 2)
				TestGameAudio.Instance.PlaySound3D("FX_LaneLight_2", gameObject.transform);
			else 
				TestGameAudio.Instance.PlaySound3D("FX_LaneLight_3", gameObject.transform);
		}

		public void LaneAlreadyActivatedEventHandler(string evtName, object evtData) {
			int lane = (int)evtData;
			if (lane == 0)
				TestGameAudio.Instance.PlaySound3D("FX_LaneOutNull", gameObject.transform);
			else if (lane == 1)
				TestGameAudio.Instance.PlaySound3D("FX_LaneReturnNull", gameObject.transform);
			else if (lane == 2)
				TestGameAudio.Instance.PlaySound3D("FX_LaneReturnNull", gameObject.transform);
			else 
				TestGameAudio.Instance.PlaySound3D("FX_LaneOutNull", gameObject.transform);
		}

		public void LanesCompletedEventHandler(string evtName, object evtData) {
			glintDelayTimer = GLINT_DELAY_START_TIME;
			glintCounter = NUM_GLINTS;
			LanesCompletedStatus status = (LanesCompletedStatus)evtData;

			LanesCompletedStatus data = (LanesCompletedStatus)evtData;
			GameObject lanesCompletedObject = (GameObject)Instantiate(Resources.Load("Prefabs/Lanes/InOutlane_Completion"));
			LaneCompletionDisplay lanesCompletedDisplay = lanesCompletedObject.GetComponent<LaneCompletionDisplay>();
			lanesCompletedDisplay.SetData(data);

			soundName = "ScoopAward" + status.award;
			if (status.award == "BonusX") {
				awardSoundDelayTimer = AWARD_DELAY_TIME;
				Instantiate(Resources.Load("Prefabs/Lanes/BonusX"));
			}
			else if (status.award == "Respawn") {
				awardSoundDelayTimer = AWARD_DELAY_TIME;
				Instantiate(Resources.Load("Prefabs/Lanes/Respawn"));
			}
			else {
				long score = Convert.ToInt64(status.award);
				popupScores.Spawn("", score, "flipperGapUp", "flipperGap", 2.0f, 0.5f, false );
				glintCounter = NUM_GLINTS - 1;
				soundName = "LanesCompleted";
				awardSoundDelayTimer = GLINT_DELAY_TIME * NUM_GLINTS;
			}
		}
		
	}

}
