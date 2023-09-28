using UnityEngine;
using System.Collections;
using Multimorphic.P3App.GUI;

namespace PinballClub.TestGame.GUI {
	public class AttractSceneController : TestGameSceneController {

		private GameObject currAnim;
		//private GameObject BonusSummaryObject;
		GameObject dialogRoot;
		GameObject titlesRoot;

		private float enableTitlesCountdown = 0;
		private const float ENABLE_TITLES_DURATION = 0.2f;

		// Use this for initialization
		public override void Start () {
			base.Start();

			//BonusSummaryObject = GameObject.Find("BonusSummary");
			//BonusSummaryObject = GameObject.FindGameObjectWithTag("HUDBonusSummary");

			//BonusSummaryObject.renderer.enabled = false;

			if (TestGameAudio.Instance)
				TestGameAudio.Instance.StopAllPlaylists();

			dialogRoot = GameObject.Find("Dialogs");
			titlesRoot = GameObject.Find("Titles");
		}

		protected override void CreateEventHandlers() {
			base.CreateEventHandlers ();
			AddModeEventHandler("Evt_StartGame", StartGameEventHandler);
			AddModeEventHandler ("Evt_ShowSelector", ShowSelectorEventHandler);
			AddModeEventHandler ("Evt_ShowDialog", ShowDialogEventHandler);
			AddModeEventHandler ("Evt_HideSelector", HideSelectorEventHandler);
			AddModeEventHandler ("Evt_HideDialog", HideDialogEventHandler);
			}

		// Update is called once per frame
		public override void Update () {
			base.Update ();

			if (dialogRoot == null)
				dialogRoot = GameObject.Find("Dialogs");

			if (enableTitlesCountdown > 0) {
				enableTitlesCountdown -= Time.deltaTime;
				EnableTitles();
			}
		}

		public void StartGameEventHandler(string eventName, object eventObject) {
			TestGameAudio.Instance.PlaySound3D("StartGame_PlayerOne", gameObject.transform);
		}

		public void ShowDialogEventHandler(string eventName, object eventObject) {
			enableTitlesCountdown = ENABLE_TITLES_DURATION;
		}

		public void HideDialogEventHandler(string eventName, object eventObject) {
			enableTitlesCountdown = ENABLE_TITLES_DURATION;
		}

		public void ShowSelectorEventHandler(string eventName, object eventObject) {
			enableTitlesCountdown = ENABLE_TITLES_DURATION;
		}

		public void HideSelectorEventHandler(string eventName, object eventObject) {
			enableTitlesCountdown = ENABLE_TITLES_DURATION;
		}

		/// <summary>
		/// If there are any dialogs showing, hide the titles.  Otherwise show them.
		/// </summary>
		private void EnableTitles() {
			bool titlesEnabled = true;

			if (dialogRoot && titlesRoot) {

				for (int i = 0; i < dialogRoot.transform.childCount; i++)
					titlesEnabled &= !(dialogRoot.transform.GetChild(i).gameObject.activeInHierarchy);
				
				titlesRoot.SetActive(titlesEnabled);
			}
		}

	}
}
