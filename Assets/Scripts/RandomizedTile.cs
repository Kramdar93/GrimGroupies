using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomizedTile : MonoBehaviour {

    public Sprite[] acceptableTiles;

	// Use this for initialization
	void Start () {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        int index = Random.Range(0, acceptableTiles.Length);
        sr.sprite = acceptableTiles[index];
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
