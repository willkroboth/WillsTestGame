using UnityEngine;
using System.Collections;
using Multimorphic.P3App.GUI;

namespace PinballClub.TestGame.GUI {

	public class TestGameHomeSceneController : TestGameSceneController {
	
		// Use this for initialization
		public override void Start () {
			base.Start ();
		}

		protected override void CreateEventHandlers() {
			base.CreateEventHandlers ();
			// AddModeEventHandler(("Evt_SomeEventName", SomeHandlerFunction);
		}
		
		// Update is called once per frame
		public override void Update () {
			base.Update ();
		}

		protected override void SceneLive() {
			base.SceneLive();
		}
	}
}
