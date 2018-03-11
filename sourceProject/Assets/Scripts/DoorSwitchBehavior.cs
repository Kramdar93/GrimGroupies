using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorSwitchBehavior : MonoBehaviour {

    public GameObject[] doors;
    public float cooldown;
    public bool preFlipped = false;

    private Animator myAnim;
    private float timer;

    void Start()
    {
        myAnim = GetComponent<Animator>();

        if(preFlipped)
        {
            ToggleSwitch(false);
        }
    }

    void Update()
    {
        if(timer >= 0)
        {
            timer -= Time.deltaTime;
        }
    }

    public bool ToggleSwitch(bool notifyPlayer = true)
    {
        if (timer <= 0)
        {
            foreach (GameObject go in doors)
            {
                //toggle
                DoorBehavior d = go.GetComponent<DoorBehavior>();
                if (d != null)
                {
                    d.ToggleDoors();
                    PlayerController pc = GameObject.FindObjectOfType<PlayerController>();
                    AutoID myID = GetComponent<AutoID>();
                    if(pc!=null && myID != null && notifyPlayer)
                    {
                        pc.flipItemState(myID.id);
                    }
                }
            }
            myAnim.SetTrigger("Toggle");
            timer = cooldown;
            return true;
        }
        return false;
    }

}
