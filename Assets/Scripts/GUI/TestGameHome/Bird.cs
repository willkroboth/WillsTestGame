using UnityEngine;
using Multimorphic.P3App.GUI;
using PinballClub.TestGame.Modes;

namespace PinballClub.TestGame.GUI
{
	public class Bird : P3Aware
	{
		public Vector3 velocity;
		public float movementSpeed;

		// Use this for initialization
		public override void Start() {
			base.Start();
		}

        protected override void CreateEventHandlers()
        {
            base.CreateEventHandlers();
        }

        // Update is called once per frame
        public override void Update() {
			base.Update();
			transform.position += velocity * movementSpeed * Time.deltaTime;
		}

		public int score = 100;

        public void OnTriggerEnter(Collider other)
        {
            if(other.name == "BallAvatarTrail")
            {
                PostGUIEventToModes(TestGameEventNames.BirdHit, score);
                popupScores.Spawn("PhotoTaken", score, transform.position, 1);
                Destroy(gameObject);
            }
        }
    }
}