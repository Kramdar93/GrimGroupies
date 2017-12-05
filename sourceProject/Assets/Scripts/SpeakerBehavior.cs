using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeakerBehavior : MonoBehaviour {

    public string[] oneTimer;
    public ArrayWrapper[] randomRepeats;
    public float cooldown;
    public Vector2 pos;
    public bool lastOne;
    public bool givesObj;

    private float timer = 0;
    private SimpleTextPopper textPopper;
    private bool isFirstTime = true;
    private GameObject textPosition;
    private ArrayWrapper[] unusedRepeats;

    [System.Serializable]
    public class ArrayWrapper
    {
        public string[] array;

        public ArrayWrapper(string[] s)
        {
            array = s;
        }
    }

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

        unusedRepeats = randomRepeats;
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
            if (givesObj)
            {
                GameObject.FindObjectOfType<PlayerController>().objectives += 1;
            }
            return true;
        }
        else if (timer <= 0)
        {
            //check win condidition
            PlayerController pc = GameObject.FindObjectOfType<PlayerController>();
            if (pc.objectives >= 3)
            {
                pc.showEnd();
            }

            //get random index
            int index = Random.Range(0, unusedRepeats.Length);
            //poppit
            textPopper.MakePopup(textPosition.transform.position.x + pos.x, textPosition.transform.position.y + pos.y, unusedRepeats[index].array);
            timer = cooldown;

            //remove from unused
            if(unusedRepeats.Length > 1)
            {
                ArrayWrapper[] temp = new ArrayWrapper[unusedRepeats.Length - 1]; //shorten by 1
                int tempIndex = 0; // separate index for new array;
                for(int i = 0; i < unusedRepeats.Length; ++i)
                {
                    if(index != i) //not the used index
                    {
                        temp[tempIndex] = unusedRepeats[i]; //copy in
                        ++tempIndex; //increment external index
                    }
                    //else it's the used one so ignore.
                }
                //temp set up, set it to unused array
                unusedRepeats = temp;
            }
            else //<=1 so restart
            {
                unusedRepeats = randomRepeats;
            }

            return true;
        }
        return false;
    }
}
