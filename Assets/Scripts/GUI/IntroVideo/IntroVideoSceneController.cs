using UnityEngine;
using System.Collections;
using PinballClub.TestGame.GUI;
using Multimorphic.P3App.GUI;

namespace PinballClub.TestGame.GUI {

	public class IntroVideoSceneController : TestGameSceneController {

		public bool useAlternateAudio;
		private AudioSource alternateAudio;
		private GameObject P3PlayfieldObj;
		private GameObject TestGameAudioObj;

		// Use this for initialization
		public override void Start () {
			base.Start ();

			GameObject obj = GameObject.Find ("AlternateAudio");
			if (obj) {
				alternateAudio = obj.GetComponent<AudioSource>();
				alternateAudio.gameObject.SetActive (useAlternateAudio);
			}

			// Turn off playfield so that video runs smoothly
			P3PlayfieldObj = GameObject.Find ("P3Playfield(Clone)");
			if (P3PlayfieldObj)
				P3PlayfieldObj.SetActive(false);

			TestGameAudioObj = GameObject.Find ("TestGameAudio(Clone)");
			if (TestGameAudioObj) {
				TestGameAudioObj.SetActive(!useAlternateAudio);
				TestGameAudioObj.GetComponent<TestGameAudio>().moveWithCamera = false;
			}

			TestGameAudio.Instance.ChangePlaylistByName("IntroAnimAudio");
		}

		protected override void CreateEventHandlers() {
			base.CreateEventHandlers ();
			AddModeEventHandler("Evt_StopIntroVideo", StopVideo);
		}
		
		// Update is called once per frame
		public override void Update () {
			IntroActive = false;
			base.Update ();
		}

		protected override void SceneLive() {
			base.SceneLive();
		}

		private void StopVideo(string eventName, object eventObject) {
            if (TestGameAudio.Instance)
                TestGameAudio.Instance.StopAllPlaylists();
		}

		protected override void OnDestroy () {
            if (TestGameAudio.Instance)
                TestGameAudio.Instance.StopAllPlaylists();

			// Reneable objects for normal gameplay
			if (P3PlayfieldObj)
				P3PlayfieldObj.SetActive(true);

			if (TestGameAudioObj) {
				TestGameAudioObj.SetActive(true);
				TestGameAudioObj.GetComponent<TestGameAudio>().moveWithCamera = true;
			}

			base.OnDestroy();
		}
	}
}