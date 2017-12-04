using UnityEngine;
using System.Collections;

public class GGCameraBehavior : MonoBehaviour {

    public float targetAspectRatio;
    public Color letterboxColor;

    private Camera backgroundCamera;
    private Camera mainCam;
    private int oldHeight, oldWidth;


	// called when we're done loading but before start.
	void Awake () {
        //get reference to main camera
        mainCam = GetComponent<Camera>();
        
        //set up background camera that only renders letterboxing.
        GameObject bgcamobj = new GameObject("BackgroundCamera", typeof(Camera));
        backgroundCamera = bgcamobj.GetComponent<Camera>();
        backgroundCamera.backgroundColor = letterboxColor;
        backgroundCamera.clearFlags = CameraClearFlags.SolidColor;
        backgroundCamera.cullingMask = 0; //only show the letterbox color
        backgroundCamera.depth = int.MinValue;  //render everything on top of it.

        SetLetterbox();
    }

    void SetLetterbox()
    {
        float newRatio = (float)Screen.width / (float)Screen.height;

        if (newRatio > targetAspectRatio) //wider screen than desired
        {
            float boxDistance = 1.0f - targetAspectRatio / newRatio; //get the total amount of letterbox needed
            mainCam.rect = new Rect(boxDistance / 2, 0.0f, 1.0f - boxDistance, 1.0f); //use it to set main camera.
            //background camera will resize to window and we can ignore it.
        }
        else //taller screen than desired
        {
            float boxDistance = 1.0f - newRatio / targetAspectRatio; //get the total amount of letterbox needed
            mainCam.rect = new Rect(0.0f, boxDistance / 2, 1.0f, 1.0f - boxDistance);
        }

        oldWidth = Screen.width;
        oldHeight = Screen.height;
    }

    //called on game start.
    void Start()
    {

    }

    // Update is called once per frame
    void Update () {
	    if(oldHeight != Screen.height || oldWidth != Screen.width) //need to update letterbox
        {
            SetLetterbox();
        }
	}
}
