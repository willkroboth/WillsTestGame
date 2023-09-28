using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Multimorphic.P3App.GUI;

namespace PinballClub.TestGame.GUI {

	public class MultiPowJackpot : MonoBehaviour {

		public int numPows = 1;
		private int numPowSounds = 1;
		public string powPrefabName = "";
		public float timeBetweenPows = 0.3f;
		public float timeBeforePowSound = 0.5f;
		public string jackpotPrefabName = "";
		public string jackpotSoundName = "";
		public string powSoundName = "";
		public bool extraPow = false;
		public float extraPowDelay = 0.5f;

		private float timer;
		private float powSoundTimer;
		private float extraPowTimer;

		// Use this for initialization
		public virtual void Start () {
			// Instantiate first pow prefab
			numPowSounds = numPows;
			if (numPows > 0)
				PlayPow();
			else
				PlayJackpot ();
			SetupTimer();
			powSoundTimer = timeBeforePowSound;
		}

		// Update is called once per frame
		public virtual void Update () {
			if (timer > 0) {
				timer -= Time.deltaTime;
				if (timer <= 0) {
					if (numPows > 0) {
						PlayPow ();
					}
					else if (numPows == 0) {
						PlayJackpot ();
					}
				}
			}

			if (powSoundTimer > 0) {
				powSoundTimer -= Time.deltaTime;
				if (powSoundTimer <= 0) {
					if (numPowSounds > 0) {
						PlayPowSound ();
					}
				}
			}

			if (extraPowTimer > 0) {
				extraPowTimer -= Time.deltaTime;
				if (extraPowTimer <= 0) {
					PlayPowSound();
				}
			}
		}

		private void PlayPow() {
			Instantiate(Resources.Load(powPrefabName));
			numPows--;
			SetupTimer ();
		}

		private void PlayPowSound() {
			TestGameAudio.Instance.PlaySound3D(powSoundName, gameObject.transform);
			numPowSounds--;
			SetupPowSoundTimer();
		}
		
		private void PlayJackpot() {
			TestGameAudio.Instance.PlaySound3D(jackpotSoundName, gameObject.transform);
			Instantiate(Resources.Load(jackpotPrefabName));
			numPows--;
			SetupTimer();
			if (extraPow) extraPowTimer = extraPowDelay;
		}

		private void SetupTimer() {
			if (numPows > 0) {
				timer = timeBetweenPows;
			}
			else if (numPows == 0) {

				CheckForJackpot ();
			}
			else if (numPows < 0) {
				if (extraPow) {
					timer = extraPowDelay;
				}
			}
		}

		private void SetupPowSoundTimer() {
			if (numPowSounds > 0) {
				powSoundTimer = timeBetweenPows;
			}
		}
		
		private void CheckForJackpot() {
			if (jackpotPrefabName != "") {
				timer = timeBetweenPows;
			}
		}

	}
}
