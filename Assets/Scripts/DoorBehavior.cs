using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorBehavior : MonoBehaviour {

    bool open = false;

    public void ToggleDoors()
    {
        if (open)
        {
            //so close
            foreach (Transform child in transform)
            {
                child.GetComponent<Collider2D>().enabled = true;
                child.GetComponent<SpriteRenderer>().enabled = true;
            }
            open = false;
        }
        else //otherwise it's closed
        {
            //so open
            foreach (Transform child in transform)
            {
                child.GetComponent<Collider2D>().enabled = false;
                child.GetComponent<SpriteRenderer>().enabled = false;
            }
            open = true;
        }
    }
}
