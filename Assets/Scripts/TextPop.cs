using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextPop : MonoBehaviour {

    private bool started;
    public string[] nextStrings;

    private float timer;
    private static float timePerLetter = 0.1f;

    private SimpleTextPopper popper;

	// Use this for initialization
	void Start () {
        popper = GameObject.FindObjectOfType<SimpleTextPopper>().GetComponent<SimpleTextPopper>();
	}
	
	// Update is called once per frame
	void Update () {
		if(started)
        {
            timer -= Time.deltaTime;

            if(timer <= 0)
            {
                if (transform.parent != null)
                {
                    popper.MakePopup(transform.position.x, transform.position.y, nextStrings, transform.parent.gameObject);
                }
                else
                {
                    popper.MakePopup(transform.position.x, transform.position.y, nextStrings, null);
                }
                GameObject.Destroy(gameObject);
            }
        }
	}

    public void init()
    {
        timer = timePerLetter * transform.childCount;
        started = true;
    }
}
