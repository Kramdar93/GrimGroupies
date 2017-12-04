using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpBehavior : MonoBehaviour {

    public int healthIncrease, heal;
    public GameObject newPlayerObj;
    public GameObject replaceWith;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnTriggerEnter2D(Collider2D collider)
    {
        PlayerController pc = collider.transform.root.GetComponentInChildren<PlayerController>();
        
        if(pc != null)
        {
            GameObject oldGo = pc.transform.parent.gameObject;
            AIController pcai = pc.GetComponentInParent<AIController>();
            pcai.maxHealth += healthIncrease;
            pcai.currentHealth += heal;

            if(newPlayerObj != null)
            {

                
                //make new host
                GameObject newgo = Instantiate(newPlayerObj, oldGo.transform.position, Quaternion.identity);
                AIController newai = newgo.GetComponent<AIController>();

                //copy over relavent values
                newai.maxHealth = pcai.maxHealth;
                //newai.currentHealth = pcai.maxHealth;  //lolbabbymoad
                newai.currentHealth = pcai.currentHealth;

                //save cameraTargetPosition
                Vector3 oldCTposition = pc.GetComponentInChildren<Rigidbody2D>().transform.position;
                //move player controller
                pc.transform.parent = newgo.transform;
                pc.transform.localPosition = Vector2.zero;

                //connect cameratarget
                pc.GetComponentInChildren<Rigidbody2D>().GetComponent<SpringJoint2D>().connectedBody = newgo.GetComponent<Rigidbody2D>();
                //reposition back
                pc.GetComponentInChildren<Rigidbody2D>().transform.position = oldCTposition;

                //now we're done so remove old husk
                GameObject.Destroy(oldGo);

            }

            if(replaceWith !=null)
            {
                Instantiate(replaceWith, transform.position, Quaternion.identity);
            }
            
            //finally remove yourself
            GameObject.Destroy(gameObject);
            GameObject.FindObjectOfType<AudioManager>().playSFX("powerup", transform.position);
        }
    }
}
