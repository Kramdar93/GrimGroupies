using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorSwitchBehavior : MonoBehaviour {

    public GameObject[] doors;
    public float cooldown;

    private Animator myAnim;
    private float timer;

    void Start()
    {
        myAnim = GetComponent<Animator>();
    }

    void Update()
    {
        if(timer >= 0)
        {
            timer -= Time.deltaTime;
        }
    }

    public bool ToggleSwitch()
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
                }
            }
            myAnim.SetTrigger("Toggle");
            timer = cooldown;
            return true;
        }
        return false;
    }

}
