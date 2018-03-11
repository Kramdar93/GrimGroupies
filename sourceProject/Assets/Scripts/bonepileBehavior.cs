using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bonepileBehavior : MonoBehaviour {

    private SpriteRenderer flames;

	// Use this for initialization
	void Start () {
        flames = transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>(); //only one child and just using getcomponentinchildren gets our sr as well so we have to use this bad method
        flames.enabled = false;
	}

    void OnTriggerEnter2D(Collider2D col)
    {
        PlayerController pc = col.transform.root.GetComponentInChildren<PlayerController>();
        if(pc != null)
        {
            pc.lastSpawnID = GetComponent<AutoID>().id;
            flames.enabled = true;

            foreach (bonepileBehavior bb in GameObject.FindObjectsOfType<bonepileBehavior>()) {
                if (bb.gameObject != gameObject) {
                    bb.flames.enabled = false;
                }
            }
        }
    }
	
}
