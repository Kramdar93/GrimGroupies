using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeakerBehavior : MonoBehaviour {

    public string[] oneTimer;
    public string[] onSuccess;
    public ArrayWrapper[] randomRepeats;
    public float cooldown;
    public Vector2 pos;
    public bool lastOne;
    public int givesObjNumber = -1;
    public int objPrerequisite = -1;
    public Sprite spriteOnReapeat = null;
    public bool deleteOnRepeat = false;

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
            //get pc ref
            PlayerController pc = GameObject.FindObjectOfType<PlayerController>();
            //check prereqs
            if (givesObjNumber >= 0)
            {
                if (pc.objectives[givesObjNumber] >= objPrerequisite)
                {
                    //pop success message
                    textPopper.MakePopup(textPosition.transform.position.x + pos.x, textPosition.transform.position.y + pos.y, onSuccess);
                    timer = cooldown;
                    //record progress
                    isFirstTime = false;
                    pc.objectives[givesObjNumber] += 1;
                }
                else
                {
                    //pop first time text again to remind player.
                    textPopper.MakePopup(textPosition.transform.position.x + pos.x, textPosition.transform.position.y + pos.y, oneTimer);
                    timer = cooldown;
                }
            }
            else //no connected objective, just progress
            {
                //pop text
                textPopper.MakePopup(textPosition.transform.position.x + pos.x, textPosition.transform.position.y + pos.y, oneTimer);
                timer = cooldown;
                isFirstTime = false;
            }
            return true;
        }
        else if (timer <= 0)
        {
            if (deleteOnRepeat)
            {
                //increment again to remember it's picked up
                if (givesObjNumber >= 0)
                {
                    GameObject.FindObjectOfType<PlayerController>().objectives[givesObjNumber] += 1;
                }
                //now delete us
                GameObject.Destroy(gameObject);
                //not sure if this will still execute after deletion but return just in case as to not hit any following code.
                return true;
            }
            //check win condidition
            PlayerController pc = GameObject.FindObjectOfType<PlayerController>();
            if (givesObjNumber == 0 && pc.objectives[givesObjNumber] >= 3)
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
