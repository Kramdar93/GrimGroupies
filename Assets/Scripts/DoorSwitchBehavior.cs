using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorSwitchBehavior : MonoBehaviour {

    public GameObject[] doors;

    private Animator myAnim;

    void Start()
    {
        myAnim = GetComponent<Animator>();
    }

    public void ToggleSwitch()
    {
        foreach(GameObject go in doors)
        {
            //toggle
            DoorBehavior d = go.GetComponent<DoorBehavior>();
            if(d!=null)
            {
                d.ToggleDoors();
            }
        }
        myAnim.SetTrigger("Toggle");
    }

}
