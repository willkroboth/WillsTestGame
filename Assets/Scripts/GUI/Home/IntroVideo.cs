using UnityEngine;
using System.Collections;
using Multimorphic.P3App.GUI;

namespace PinballClub.TestGame.GUI {

	public class IntroVideo: P3Aware {
		
		public bool loop = false;
		public bool autoplay = true;

		private MovieTexture movie;
		private Renderer render;
		private bool wasPlaying = false;
		private AudioSource alternateAudio;
			
		// Use this for initialization
		public override void Start () {
			base.Start ();
			render = gameObject.GetComponent<Renderer>();
			movie = (MovieTexture)render.material.mainTexture;

			if (movie == null)  
				render.material.color = Color.black;  // no video to show.


			if (autoplay)
				PlayVideo ("", null);
			else
				render.enabled = false;

			GameObject obj = GameObject.Find ("AlternateAudio");
			if (obj)
				alternateAudio = obj.GetComponent<AudioSource>();
		}
		
		public override void Update () {
			base.Update ();
		
			if (movie && ((p3Interface != null) && p3Interface.IsReady)) {
				if (wasPlaying && !movie.isPlaying) {
					PostGUIEventToModes("Evt_IntroVideoStopped", null);
				}
				wasPlaying = movie.isPlaying;
			}
			else {
				// No video texture?  We've stopped before we've started.
				PostGUIEventToModes("Evt_IntroVideoStopped", null);
			}

			/*(
			if (PlayTime > 0)
			{
				PlayTime -= Time.deltaTime;
				if (PlayTime <= 0) {
					PostGUIEventToModes("Evt_IntroVideoStopped", null);
				}
			}
			*/
		}

		protected override void CreateEventHandlers() {
			base.CreateEventHandlers ();
			AddModeEventHandler("Evt_PlayIntroVideo", PlayVideo);
			AddModeEventHandler("Evt_StopIntroVideo", StopVideo);
		}

		private void PlayVideo(string eventName, object eventObject) {
			render.enabled = true;
			//SetAlpha (1f);
			if (movie)
 				movie.Play();

			if ((alternateAudio) && (alternateAudio.gameObject.activeSelf)) {
				alternateAudio.time = 0f;
				alternateAudio.Play ();
			}
		}
	
		private void StopVideo(string eventName, object eventObject) {
			render.enabled = false;
			if (movie)
				movie.Stop();

			if ((alternateAudio) && (alternateAudio.gameObject.activeSelf)) {
				alternateAudio.Stop ();
			}
		}

		private void SetAlpha(float alpha) {
			Color color = render.material.color;
			color.a = alpha;
			render.material.color = color;
		}
	}
}