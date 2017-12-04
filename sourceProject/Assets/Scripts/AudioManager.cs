using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour {

    public AudioClip[] sfx;
    public AudioClip[] music;

    private AudioSource source;
    private PlayerController pc;

	// Use this for initialization
	void Start () {
        source = new GameObject("source", typeof(AudioSource)).GetComponent<AudioSource>();
        source.transform.parent = transform;
        
	}

    public void playSFX(string s, Vector3 pos)
    {
        pc = GameObject.FindObjectOfType<PlayerController>();
        foreach(AudioClip c in sfx)
        {
            if(c.name == s)
            {
                source.PlayOneShot(c, Mathf.Min(1/Vector3.Distance(pc.transform.position, pos), 0.5f));
                return;
            }
        }
    }

}
