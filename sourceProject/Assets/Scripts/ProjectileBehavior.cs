using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileBehavior : MonoBehaviour {

    public GameObject hitEffect;
    public int damage;

    private Rigidbody2D myRB2;
    private Vector2 initialVelocity;
    private Quaternion rotation;

	// Use this for initialization
	void Awake () {
        myRB2 = GetComponent<Rigidbody2D>();
	}

    public void init(Vector2 initvel)
    {
        myRB2.linearVelocity = initvel;
        initialVelocity =  initvel;
        rotation = transform.rotation;
        GameObject.FindObjectOfType<AudioManager>().playSFX("bow", transform.position);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        //ignore same-tagged enemies for maximum mayhem
        if (tag != collision.collider.tag)
        {
            //need to get to the parent gameobject of what we hit since all important colliders are children of their objects
            GameObject hitObject;
            Transform ptrans = collision.collider.transform.parent;
            if (ptrans != null)
            {
                hitObject = ptrans.gameObject; //most cases this will be true
            }
            else
            {
                hitObject = collision.collider.gameObject; //on failure forget about parent
            }

            //else innanimate object, just destroy
            GameObject go = Instantiate(hitEffect, transform.position, Quaternion.identity);
            ExplosionBehavior explosion = go.GetComponent<ExplosionBehavior>();
            explosion.damage = damage;

            //tag it
            explosion.tag = tag;

            explosion.Eval();
            GameObject.Destroy(gameObject);
        }
        else
        {
            //ignore and reset to originals
            Physics2D.IgnoreCollision(collision.collider, collision.otherCollider);
            myRB2.linearVelocity = initialVelocity;
            myRB2.angularVelocity = 0;
            transform.rotation = rotation;
        }
    }
}
