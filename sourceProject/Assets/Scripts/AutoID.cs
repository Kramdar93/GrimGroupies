using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoID : MonoBehaviour {

    public int id = 0;

    private static int next = 0;

	// Use this for initialization
	void Awake () {
        //positive ids are set by this script. use negatives for hardcoded ids.
        if (id >= 0)
        {
            id = next++; //get next id, increment.
        }

        //redundant but don't know how else to do this.
        UnityEngine.SceneManagement.SceneManager.sceneUnloaded += OnSceneUnloaded;
	}

    void OnSceneUnloaded(UnityEngine.SceneManagement.Scene scene)
    {
        next = 0;
    }
}
