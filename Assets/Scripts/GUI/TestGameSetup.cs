using UnityEngine;
using System.Collections;
using System;
using Multimorphic.P3App.GUI;
using PinballClub.TestGame.Modes;

namespace PinballClub.TestGame.GUI
{
	public class TestGameSetup : Setup {

		public override void Awake() {
			baseAppModeType = typeof(TestGameBaseGameMode);
			base.Awake();

            if (Application.isEditor)   // Only filter the log in the Unity editor
            {
                // Filter the log here.  For performance reasons, don't overdo it.
                // P3App.Logging.Logger.IncludeOnlyMessagesContaining.Add("InterestingFoo");
                // P3App.Logging.Logger.IncludeOnlyMessagesContaining.Add("InterestingBar");
                // P3App.Logging.Logger.ExcludeMessagesContaining.Add("AnnoyingThing");
                // P3App.Logging.Logger.ExcludeMessagesContaining.Add("AnotherAnnoyingThing");
            }
        }

        // Use this for initialization
        public override void Start () {
			base.Start();

			if (GameObject.FindObjectOfType<TestGameAudio>() == null ) {
				GameObject mainCamera = GameObject.Find ("Main Camera");
				UnityEngine.Object resource = Resources.Load<GameObject>("Prefabs/TestGameAudio");
				GameObject audio = (GameObject) GameObject.Instantiate(resource, mainCamera.transform.position, mainCamera.transform.localRotation);
				GameObject.DontDestroyOnLoad(audio);
			}
		}
	}
}
