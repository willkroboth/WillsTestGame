using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Multimorphic.P3App.Modes;
using PinballClub.TestGame.Modes;
using Multimorphic.P3App.GUI;
using Multimorphic.P3App.Data;

namespace PinballClub.TestGame.GUI {
	public class BallSaveManager : P3Aware {

		private GameObject ballSaveActiveObject;
		private MeshRenderer ballSaveHardShell;
		private float ballSaveHardShellStartingAlpha;
		private MeshRenderer ballSaveSoftShell;
		private float ballSaveSoftShellStartingAlpha;
		private TextMesh [] ballSaveTimerText = new TextMesh[5];
		private TextMesh [] respawnCountText = new TextMesh[5];
		private int ballSaveTime;
		private int startingBallSaveTime;
		private GameObject respawnActiveObject;
		private bool respawnActive;
		private int respawnCount;

		// Use this for initialization
		public override void Start () {
			base.Start();

			respawnCount = 0;
			respawnActive = false;
			ballSaveTime = 0;
		}

		protected override void CreateEventHandlers() {
			base.CreateEventHandlers ();
			AddModeEventHandler("Evt_BallSaveStatus", BallSaveStatusEventHandler);
			AddModeEventHandler("Evt_BallSavePlayAnimation", BallSavePlayAnimationEventHandler);
			AddModeEventHandler("Evt_RespawnStatus", RespawnStatusEventHandler);
		}

		public override void Update () {
			base.Update();
		}

		public void BallSaveStatusEventHandler(string eventName, object eventObject) {
			int time = (int)eventObject;
			if (time > 0) {
				if (ballSaveActiveObject == null) {
					startingBallSaveTime = time;
					ballSaveTime = time;
					ballSaveActiveObject = (GameObject)Instantiate(Resources.Load("Prefabs/InvincibilityActive"));
					ballSaveHardShell = (MeshRenderer) ballSaveActiveObject.transform.Find("Shield/HardShell").gameObject.GetComponent<MeshRenderer>();  
					ballSaveSoftShell = (MeshRenderer) ballSaveActiveObject.transform.Find("Shield/SoftShell").gameObject.GetComponent<MeshRenderer>();  
					Color hardColor = ballSaveHardShell.GetComponent<Renderer>().material.color;
					ballSaveHardShellStartingAlpha = hardColor.a;
					Color softColor = ballSaveSoftShell.GetComponent<Renderer>().material.color;
					ballSaveSoftShellStartingAlpha = softColor.a;

					for (int i=1; i<6; i++) {
						ballSaveTimerText[i-1] = (TextMesh) ballSaveActiveObject.transform.Find("TimerText/TimerText_0" + i.ToString()).gameObject.GetComponent<TextMesh>();
						ballSaveTimerText[i-1].text = time.ToString();
					}
				}
				else {
					if (time > ballSaveTime)
						startingBallSaveTime = time;
					ballSaveTime = time;
					Color hardColor = ballSaveHardShell.GetComponent<Renderer>().material.color;
					hardColor.a = (((float)ballSaveTime) / ((float)startingBallSaveTime)) * ballSaveHardShellStartingAlpha;
					ballSaveHardShell.material.color = hardColor;
					Color softColor = ballSaveSoftShell.GetComponent<Renderer>().material.color;
					softColor.a = (((float)ballSaveTime) / ((float)startingBallSaveTime)) * ballSaveSoftShellStartingAlpha;
					ballSaveSoftShell.material.color = softColor;
					for (int i=0; i<5; i++) {
						ballSaveTimerText[i].text = time.ToString();
					}
				}
			}
			else if (time <= 0) {
				ballSaveTime = time;
				if (ballSaveActiveObject != null) {
					TestGameAudio.Instance.PlaySound3D("BallSaveDisabled", gameObject.transform);
				}
				Destroy (ballSaveActiveObject);
			}

			UpdateRespawn();
		}

		public void RespawnStatusEventHandler(string eventName, object eventObject) {
			respawnCount = (int)eventObject;
			respawnActive = respawnCount > 0; 
			UpdateRespawn();
		}

		private void UpdateRespawn()
		{
			if (ballSaveTime <= 0)
			{
				if (respawnActive) {
					if (respawnActiveObject == null) {
						respawnActiveObject = (GameObject)Instantiate(Resources.Load("Prefabs/RespawnActive"));
						for (int i=1; i<6; i++) {
							respawnCountText[i-1] = (TextMesh) respawnActiveObject.transform.Find("CountText/CountText_0" + i.ToString()).gameObject.GetComponent<TextMesh>();
							respawnCountText[i-1].text = respawnCount.ToString();
						}
					}
					else {
						for (int i=0; i<5; i++) {
							respawnCountText[i].text = respawnCount.ToString();
						}
					}
				}
				else if (!respawnActive) {
					Destroy (respawnActiveObject);
				}
			}
			else {
				// Ball save is active.  Kill respawn object for now.
				Destroy (respawnActiveObject);
			}
		}

		public void BallSavePlayAnimationEventHandler(string eventName, object eventObject) {
			string sceneName = (string)eventObject;

			TestGameAudio.Instance.PlaySound3D("Ball_Save_" + sceneName, gameObject.transform);

			Instantiate(Resources.Load("Prefabs/" + sceneName + "/BallSave"));
		}

	}

}
