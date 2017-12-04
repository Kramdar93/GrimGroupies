using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class zfixer : MonoBehaviour {

    public static float granularity = 3;

    //offset is used to be able to still get relative layering 
    public float offset = 0;

    private SpriteRenderer sr;

	// Use this for initialization
	void Start () {
        sr = GetComponent<SpriteRenderer>();
	}
	
	// Update is called once per frame
	void Update () {
		//set the render order based on y position.
        sr.sortingOrder = Mathf.RoundToInt( offset - (transform.position.y * granularity) );
	}
}
