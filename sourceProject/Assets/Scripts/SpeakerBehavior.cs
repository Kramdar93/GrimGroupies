using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeakerBehavior : MonoBehaviour {

    public string[] oneTimer;
    public string[] randomRepeats;
    public float cooldown;
    public Vector2 pos;

    private float timer = 0;
    private SimpleTextPopper textPopper;
    private bool isFirstTime = true;
    private GameObject textPosition;

	// Use this for initialization
	void Start () {
        textPopper = GameObject.FindObjectOfType<SimpleTextPopper>();
        if (transform.childCount > 0)
        {
            textPosition = transform.GetChild(0).gameObject; //only child is where to put it
        }
        else
        {
            textPosition = new GameObject("txtpos");
            textPosition.transform.parent = transform;
            textPosition.transform.localPosition = Vector3.zero;
        }
	}

    void Update()
    {
        if(timer >= 0)
        {
            timer -= Time.deltaTime;
        }
    }
	
    public bool Speak()
    {
        if (isFirstTime && timer <=0 )
        {
            textPopper.MakePopup(textPosition.transform.position.x + pos.x, textPosition.transform.position.y + pos.y, oneTimer);
            timer = cooldown;
            isFirstTime = false;
            return true;
        }
        else if (timer <= 0)
        {
            textPopper.MakePopup(textPosition.transform.position.x + pos.x, textPosition.transform.position.y + pos.y, new string[]{randomRepeats[Random.Range(0, randomRepeats.Length)]});
            timer = cooldown;
            return true;
        }
        return false;
    }
}
