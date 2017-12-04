using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleTextPopper : MonoBehaviour {

    public Sprite[] font; //yeah I know this is dumb but I can't get it do work dynamically.
    public float spacing;

    //creates a gameobject from strings.
    public void MakePopup(float posx, float posy, string[] text, GameObject parent)
    {
        if (text.Length > 0)
        {
            bool isStarted = false;

            GameObject container = new GameObject("textpop", typeof(TextPop));
            container.transform.position = new Vector3(posx, posy, -1);
            if(text.Length > 1)
            {
                string[] newtext = new string[text.Length - 1];
                for(int i = 1; i < text.Length; ++i)
                {
                    newtext[i-1] = text[i];
                }

                container.GetComponent<TextPop>().nextStrings = newtext;
            }
            else
            {
                container.GetComponent<TextPop>().nextStrings = new string[0];
            }

            foreach (char c in text[0])
            {
                if (char.IsWhiteSpace(c))
                {
                    if (isStarted)
                    {
                        posx += spacing/2;
                    }
                }
                else
                {
                    Sprite s = getFont(c);
                    if (s != null)
                    {
                        GameObject go = new GameObject("fontmember", typeof(SpriteRenderer));
                        go.transform.position = new Vector3(posx, posy, -1);
                        go.GetComponent<SpriteRenderer>().sprite = s;
                        go.AddComponent<zfixer>().offset = 2;
                        go.transform.parent = container.transform;
                        posx += spacing;
                        isStarted = true;
                    }
                }
            }

            if(parent != null)
            {
                container.transform.parent = parent.transform;
            }

            //done, start timer
            container.GetComponent<TextPop>().init();
            //maek noise
            GameObject.FindObjectOfType<AudioManager>().playSFX("dud", container.transform.position);
        }
    }

    public void MakePopup(float posx, float posy, string[] text)
    {
        MakePopup(posx, posy, text, null);
    }

    //god awful function right here.
    public Sprite getFont(char c)
    {
        //make it consistent
        c = char.ToLower(c);

        //test all alpha
        char testChar = 'a';
        for (int i = 0; i < 25; i++)
        {
            if(c == testChar)
            {
                return font[i];
            }

            testChar++;
        }

        //test all num
        testChar = '0';
        for (int i = 26; i < 36; i++)
        {
            if (c == testChar)
            {
                return font[i];
            }

            testChar++;
        }

        //handle special characters
        switch (c)
        {
            case '.':
                return font[36];
            case '!':
                return font[37];
            case '?':
                return font[38];
            default:
                return null;
        }
    }
}
