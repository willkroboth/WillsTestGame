using UnityEngine;
using System.Collections;

namespace PinballClub.TestGame.GUI {

	public class CameraZoom : MonoBehaviour {
		private Camera theCamera;
		//private float startSize;
		public float endSize = 1.4f;
		public bool zooming = false;
		public Vector3 startPosition;
		public GameObject targetPosition;
		public GameObject targetObject;
		public float rotationSpeed = 5.0f;
		public float zoomSpeed = 5.0f;
		public float movementSpeed = 5.0f;
		private float startTime = 0;
		private GameObject cameraTarget;
		private bool wasZooming = false;
		public float duration = 1.0f;  // seconds
		public float startDelay = 0;

		void Awake() {
			cameraTarget = new GameObject(); // GameObject.CreatePrimitive(PrimitiveType.Sphere);
		}
		// Use this for initialization
		void Start () {
			theCamera = (Camera) gameObject.GetComponent<Camera>();
		}
		
		// Update is called once per frame
		void Update () {
			if (!wasZooming && zooming) {
				// Just started zooming
				//startSize = theCamera.orthographicSize;
				startPosition = theCamera.transform.position;
				// set the camera target's start point to be the direction the camera is currently 
				// looking at, about as far away from the camera as the target object we'll be moving
				// to look at.

				if (targetObject != null)
					cameraTarget.transform.position = theCamera.transform.position + theCamera.transform.forward * (startPosition - targetObject.transform.position).magnitude;
				// Assumption: The target position is an empty game object and its 
				// (otherwise unused) scale.x indicates the destination camera zoom factor.
				if (targetPosition != null)
					endSize = targetPosition.transform.localScale.x;
				startTime = Time.time;
			}

			if (zooming) {

				if (Time.time >= (startTime + startDelay) ) {
					// Zoom and truck
					if (targetPosition != null) {
						theCamera.orthographicSize = Mathf.Lerp(theCamera.orthographicSize, endSize, Time.deltaTime * zoomSpeed);
						theCamera.transform.position = Vector3.Lerp(theCamera.transform.position, targetPosition.transform.position, Time.deltaTime * movementSpeed);
					}

					// Follow the camera target, which moves towards the target object
					if (targetPosition != null) {
						cameraTarget.transform.position = Vector3.Lerp(cameraTarget.transform.position, targetObject.transform.position, Time.deltaTime * rotationSpeed);
						GetComponent<Camera>().transform.LookAt(cameraTarget.transform.position); 
					}
				}

				zooming = (Time.time <= (startTime + duration + startDelay) );
			}

			wasZooming = zooming;
		}

		public void Stop() {
			wasZooming = false;
			zooming = false;
		}

		public void JumpToEnd() {
			theCamera.orthographicSize = endSize;
			theCamera.transform.position = targetPosition.transform.position;
			cameraTarget.transform.position = targetObject.transform.position;
			GetComponent<Camera>().transform.LookAt(cameraTarget.transform.position); 
			zooming = false;
			this.enabled = false;
		}
	}
}
