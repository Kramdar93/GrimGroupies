using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionBehavior : MonoBehaviour {

    public int damage;
    public float radius;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {

        Debug.DrawLine(transform.position, transform.position + radius * Vector3.right);
	}

    public void Eval()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position,radius, LayerMask.GetMask("ProjectileHitBoxes"));
        foreach(Collider2D collider in hits)
        {
            //preemptive tag check
            if (tag != collider.tag)
            {
                Transform ptrans = collider.transform.parent;
                if (ptrans != null) //has parent
                {
                    AIController ai = ptrans.gameObject.GetComponent<AIController>();
                    if (ai != null) //parent can be hurt, and hasn't already
                    {
                        ai.DealDamage(damage);
                    }
                }
            }
        }
        GameObject.FindObjectOfType<AudioManager>().playSFX("hitOther", transform.position);
    }
}
