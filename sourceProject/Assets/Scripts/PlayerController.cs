using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour {

    public float controlBufferTimeLength;
    public float reanimateRadius;
    public float respawnDelay;
    public GameObject[] hearts;
    public int[] objectives = {0};
    public GameObject endScreen;

    public List<InputHistoryEntry> controlHistory; //see declaration below

    //references for our obj
    /* //unused, ai will handle
    private Rigidbody2D myRB2;
    private Animator myAnim;
    private SpriteRenderer mySprite; */
    private AIController myAI;
    public GameObject heartStart;
    public GameObject cameraTarget;

    public int lastSpawnID = 0;

    //references for others
    private SimpleTextPopper textPopper;
    private AudioManager audioMan;
    private bool justPlayed = false;
    private bool justAttacked = false;

    //private variables
    //private float attackTimer;
    private bool justPaused = false;
    private bool justPausedByAttack = false;
    private bool done = false;
    private bool justDied = true;
    private int lastHealth = 0;
    private float respawnTimer = 0;

    private Dictionary<string, Dictionary<int, bool>> InteractedItems;

    public struct InputHistoryEntry
    {
        public float x;
        public float y;
        public bool attack;
        public bool interact;
        public float time;

        public InputHistoryEntry(float newx, float newy, float newattack, float newinteract)
        {
            x = newx;
            y = newy;

            if(newattack > 0.1f)
            {
                attack = true;
            }
            else
            {
                attack = false;
            }

            if (newinteract > 0.1f)
            {
                interact = true;
            }
            else
            {
                interact = false;
            }

            time = Time.time;
        }
    }

    //onEnable runs just after awake
    void OnEnable()
    {
        //make sure our doormap is initiated.
        InteractedItems = new Dictionary<string, Dictionary<int, bool>>();

        //add delegate
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

	// Use this for initialization
	void Start () {
        DontDestroyOnLoad(transform.root);

        textPopper = GameObject.Find("TextPopper").GetComponent<SimpleTextPopper>();
        audioMan = GameObject.FindObjectOfType<AudioManager>();

        controlHistory = new List<InputHistoryEntry>();

        //set up camera target
        GetComponentInChildren<Rigidbody2D>().GetComponent<SpringJoint2D>().connectedBody = transform.parent.GetComponent<Rigidbody2D>();

        TogglePause(false);
    }
	
	// Update is called once per frame
	void Update () {
        myAI = GetComponentInParent<AIController>();

        //update timers
        if(respawnTimer >= 0)
        {
            respawnTimer -= Time.deltaTime;
        }

        //check health
        if(lastHealth != myAI.currentHealth)
        {
            updateHealth();
        }

        //set respawn timer
        if (myAI.currentHealth <= 0 && justDied)
        {
            respawnTimer = respawnDelay;
            justDied = false;
        }

        //check pause
        if (Input.GetAxisRaw("Pause") > 0.1f)
        {
            if (!justPaused)
            {
                if (Time.timeScale < 1)
                {
                    Time.timeScale = 1;
                    TogglePause(false);
                }
                else
                {
                    Time.timeScale = 0;
                    TogglePause(true);
                }
                justPaused = true;
                audioMan.playSFX("ready", transform.position);
            }
        }
        else
        {
            justPaused = false;
        }

        if (Time.timeScale > 0)
        {

            //log controller input
            float attackInput = 0;
            if (Input.GetAxisRaw("Attack") > 0.1f && !justPausedByAttack)
            {
                attackInput = 1;
            }
            else if (Input.GetAxisRaw("Attack") < 0.1f && justPausedByAttack)
            {
                justPausedByAttack = false;
            }
            controlHistory.Insert(0, new InputHistoryEntry(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"), attackInput, Input.GetAxisRaw("Interact")));

            //check for respawn quickening
            if(myAI.currentHealth <=0 && (attackInput > 0 || Input.GetAxisRaw("Interact") > 0) && respawnTimer <= 0)
            {
                myAI.quickenRespawn();
            }

            //limit history length
            if (Time.time - controlHistory[controlHistory.Count - 1].time > controlBufferTimeLength) // last entry is beyond point we care
            {
                controlHistory.RemoveAt(controlHistory.Count - 1); //remove it.
            }
        }
        else
        {
            //pause menu controls, quick and dirty
            if(done && Input.GetAxisRaw("Attack") > 0.1f)
            {
                Application.Quit();
            }
            Animator cursor = cameraTarget.GetComponentInChildren<Animator>();
            if(Input.GetAxisRaw("Vertical") > 0.1f)
            {
                cursor.transform.localPosition = new Vector3(-2, 0, 5);
                if (!justPlayed)
                {
                    audioMan.playSFX("dud", transform.position);
                    justPlayed = true;
                }
            }
            else if(Input.GetAxisRaw("Vertical") < -0.1f)
            {
                cursor.transform.localPosition = new Vector3(-2, -1, 5);
                if (!justPlayed)
                {
                    audioMan.playSFX("dud", transform.position);
                    justPlayed = true;
                }
            }
            else
            {
                justPlayed = false;
            }

            if(Input.GetAxisRaw("Attack") > 0.1f)
            {
                if(cursor.transform.localPosition.y >- 0.5f) //upper option resume
                {
                    //shortcut
                    Time.timeScale = 1;
                    TogglePause(false);
                    audioMan.playSFX("ready", transform.position);
                    justPausedByAttack = true;
                }
                else //lower option: quit
                {
                    Application.Quit(); //oh you wanted to save? maybe in postLD
                }
            }
        }

        //if we're alive, try to animate nearby peeps
        if(lastHealth > 0)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, reanimateRadius, LayerMask.GetMask("ProjectileHitBoxes")); //anything sentient will have hitbox
            foreach(Collider2D collider in hits)
            {
                //preemptive tag check
                if (tag != collider.tag)
                {
                    Transform ptrans = collider.transform.parent;
                    if (ptrans != null) //has parent
                    {
                        AIController ai = ptrans.gameObject.GetComponent<AIController>();
                        if (ai != null) //parent can be hurt, and hasn't already
                        {
                            ai.ReviveToPlayer();
                        }
                    }
                }
            }
            justDied = true;
        }
    }

    void updateHealth()
    {
        lastHealth = myAI.currentHealth;
        foreach(Transform child in heartStart.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        for(int i = 0; i < myAI.maxHealth/2; ++i)
        {
            GameObject h;
            if(i == lastHealth/2 && lastHealth % 2 == 1) //odd amount and last heart -> half heart
            {
                h = Instantiate(hearts[1], heartStart.transform);
            }
            else if(i < lastHealth/2) //in good health range
            {
                h = Instantiate(hearts[2], heartStart.transform);
            }
            else //beyond current health, show blank hearts
            {
                h = Instantiate(hearts[0], heartStart.transform);
            }
            h.transform.localPosition = new Vector3(i * (h.GetComponent<SpriteRenderer>().bounds.extents.x * 3),
                                                    0, 
                                                    heartStart.transform.position.z);
        }
    }

    void TogglePause(bool show)
    {
        SpriteRenderer[] sprites = cameraTarget.GetComponentsInChildren<SpriteRenderer>();
        foreach (SpriteRenderer sr in sprites)
        {
            sr.enabled = show;
        }
    }

    public void showEnd()
    {
        Time.timeScale = 0;
        Instantiate(endScreen, transform.position, Quaternion.identity);
        done = true;
    }

    void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode sceneMode)
    {
        //reposition to last important interaction. if an object is deemed important, then set lastspawnid in that script
        //eg level transitions, weapon pickups, bonepiles
        AutoID[] spoints = GameObject.FindObjectsOfType<AutoID>();
        foreach(AutoID aid in spoints)
        {
            if(lastSpawnID == aid.id)
            {
                transform.root.position = aid.transform.position;
                break;
            }
        }

        if (InteractedItems.ContainsKey(scene.name)) //we have a record for this area
        {
            //get dictionary for this area 
            Dictionary<int, bool> switchStates = InteractedItems[scene.name];

            //get all doorswitches
            DoorSwitchBehavior[] switchObjs = GameObject.FindObjectsOfType<DoorSwitchBehavior>();
            foreach(DoorSwitchBehavior s in switchObjs)
            {
                int id = s.GetComponent<AutoID>().id;
                if (switchStates.ContainsKey(id) && switchStates[id]) //has an entry and it is set to switch from default.
                {
                    s.preFlipped = true;
                }
            }

            //get all collectibles
            PowerUpBehavior[] powerupObjs = GameObject.FindObjectsOfType<PowerUpBehavior>();
            foreach(PowerUpBehavior pow in powerupObjs)
            {
                AutoID aid = pow.GetComponent<AutoID>();
                if(aid != null)
                {
                    int id = aid.id;
                    if (switchStates.ContainsKey(id) && switchStates[id]) //has an entry and it is set to collected
                    {
                        Destroy(pow.gameObject); //EXTERMINATE
                    }
                }
            }
        }
        else //no record so we must create a fresh one.
        {
            //make empty record
            Dictionary<int, bool> switchStates = new Dictionary<int, bool>();
            //get all AutoID carriers
            AutoID[] trackedObjs = GameObject.FindObjectsOfType<AutoID>();
            foreach (AutoID someObj in trackedObjs)
            {
                if (someObj.enabled)
                {
                    Debug.Log(someObj.transform.root.name + " " + someObj.id);
                    switchStates.Add(someObj.id, false); //set all to false, ie no change from default
                }
            }

            //add record to master dictionary
            InteractedItems.Add(scene.name, switchStates);
        }
    }

    public void flipItemState(int id)
    {
        InteractedItems[UnityEngine.SceneManagement.SceneManager.GetActiveScene().name][id] ^= true;
    }

    public void Respawn()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    //unused, let ai of player character query the control history.
    /*void HandleMovement()
    {
        //handle movement
        Vector2 dirInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        myRB2.velocity = dirInput.normalized * moveSpeed;

        //handle movement animations
        if (dirInput.normalized.x > 0.1f)
        {
            mySprite.flipX = true;
        }
        else if (dirInput.normalized.x < -0.1f)
        {
            mySprite.flipX = false;
        }
        //else do not change animation, let it stay in last state.

        //now for y
        if (dirInput.normalized.y > 0.1f)
        {
            myAnim.SetInteger("Vertical", 1);
        }
        else if (dirInput.normalized.y < -0.1f)
        {
            myAnim.SetInteger("Vertical", -1);
        }
        //else do not change animation, let it stay in last state.

        //determine if we're running or not
        if (dirInput.magnitude > 0.1f)
        {
            myAnim.SetBool("isRunning", true);
        }
        else
        {
            myAnim.SetBool("isRunning", false);
        }
    }

    void HandleAttack()
    {
        if(attackTimer <= 0)
        {
            attackTimer = attackDelay;
            myAnim.SetBool("isAttacking", true);


        }
    }*/
}
