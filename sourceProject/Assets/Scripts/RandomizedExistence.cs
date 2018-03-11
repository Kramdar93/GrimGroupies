using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomizedExistence : MonoBehaviour {

    public float chanceToSpawn;

	// Use this for initialization
	void Start () {
		if(Random.Range(0f,1f) > chanceToSpawn)  //means we failed the check so despawn
        {
            GameObject.Destroy(gameObject);
        }
	}
	
}
