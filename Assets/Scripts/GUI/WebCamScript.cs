using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WebCamScript : MonoBehaviour {

    static WebCamTexture webCam;

	// Use this for initialization
	void Start () {

        if (webCam == null)
            webCam = new WebCamTexture();

        if (WebCamTexture.devices.Length > 1)
            webCam.deviceName = WebCamTexture.devices[1].name;
        else
            webCam.deviceName = WebCamTexture.devices[0].name;

        GetComponent<Renderer>().material.mainTexture = webCam;

        if (!webCam.isPlaying)
            webCam.Play();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
