using UnityEngine;
using Multimorphic.P3App.GUI;
using PinballClub.TestGame.Modes;

namespace PinballClub.TestGame.GUI
{
	public class BirdLauncher : P3Aware
	{
		public GameObject[] bushes;
		public GameObject birdPrefab;
		public Vector3 spawnOffset = new Vector3(0, 0.5f, 0);

		public override void Start()
		{
			base.Start();
		}

		protected override void CreateEventHandlers()
        {
			base.CreateEventHandlers();
			AddModeEventHandler(TestGameEventNames.SpawnBird, SpawnBirdEventHandler);
			AddModeEventHandler(TestGameEventNames.SpawnBirdFromIndex, SpawnBirdFromIndexEventHandler);
        }

		// Debug helper method to spawn bird from keyboard shortcut
		private void SpawnBirdEventHandler(string eventName, object eventData)
        {
			SpawnBirdFromIndexEventHandler(eventName, 0);
        }

		private void SpawnBirdFromIndexEventHandler(string eventName, object eventData)
        {
			int bushIndex = (int)eventData;
			GameObject bush = bushes[bushIndex];

			// Flight vector
			Vector3 velocity = bush.GetComponent<Bush>().RandomDirection();

			// Setup bird
			GameObject bird = Instantiate(
				birdPrefab,
				bush.transform.position + spawnOffset,
				Quaternion.LookRotation(velocity, new Vector3(0, 1, 0))
			);
			bird.transform.SetParent(transform, true);
			bird.GetComponent<Bird>().velocity = velocity;
			Destroy(bird, 5);
        }

		// Update is called once per frame
		public override void Update()
		{
			base.Update();
		}
	}
}
