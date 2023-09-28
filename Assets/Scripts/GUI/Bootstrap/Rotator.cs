using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotator : MonoBehaviour {

	public Vector3 minRotation;
	public Vector3 maxRotation;
	public float speed = 2f;
	private Vector3 targetRotation;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		transform.localEulerAngles = minRotation + Mathf.Abs(Mathf.Sin(Time.timeSinceLevelLoad * speed)) * (maxRotation - minRotation);
	}
}
