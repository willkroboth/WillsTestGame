using UnityEngine;
using System.Collections;
using Multimorphic.P3App.GUI;
using Multimorphic.P3App.Logging;

namespace PinballClub.TestGame.GUI {
	
	public class MovingTarget : P3Aware {
		private Vector3 destination;
        private float disabledCountdown = -1;
        private const float DISABLED_DURATION = 1.5f;  // seconds
        private const float SPEED = 3f;
		
		// Use this for initialization
		public override void Start () {
			base.Start ();
			gameObject.GetComponent<Collider>().enabled = true;
			destination = gameObject.transform.localPosition;
		}

		protected override void CreateEventHandlers() {
			base.CreateEventHandlers ();
			AddModeEventHandler("Evt_MoveTarget", MoveTargetEventHandler);
			// could add more AddModeEventHandler calls here
		}

		public void MoveTargetEventHandler(string eventName, object eventData) {
			// Mode is telling us to move
			Multimorphic.P3App.Logging.Logger.Log ("GUI layer: Received MoveTarget event from mode layer.");
			int index = (int) eventData;
			destination = new Vector3(-7f + index,
			                                  gameObject.transform.localPosition.y,
			                                  Random.Range (-2.5f, 2.5f));
		}

		public void OnTriggerEnter(Collider other) {
            if (disabledCountdown < 0)  // if we're not disabled
            {
                disabledCountdown = DISABLED_DURATION;  // Allow a debounce period
                // We've been hit!  Tell the modes about it.
                Multimorphic.P3App.Logging.Logger.Log("GUI layer: Target hit by " + other.name + ". Posting TargetHit event to mode layer.");
                PostGUIEventToModes("Evt_TargetHit", other.name);
                gameObject.GetComponent<Renderer>().material.color = Color.red;
                TestGameAudio.Instance.PlaySound("GroupTest");
            }
		}
		
		// Update is called once per frame
		public override void Update () {
			base.Update ();
			gameObject.GetComponent<Renderer>().material.color = Color.Lerp(gameObject.GetComponent<Renderer>().material.color, Color.blue, Time.deltaTime * 1.5f);
			gameObject.transform.localPosition = Vector3.Lerp(gameObject.transform.localPosition, destination, Time.deltaTime * SPEED);
			gameObject.transform.Rotate(new Vector3(1f, 2f, 3f));

            if (disabledCountdown > 0)
                disabledCountdown -= Time.deltaTime;
		}
	}
}
