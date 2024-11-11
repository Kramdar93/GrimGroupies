using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AIController : MonoBehaviour {

    public float moveSpeed;
    public float controlDelayPerUnit;
    public float chargeDelay;
    public float maxAttackTime;
    public float minAttackTime;
    public float attackDelay;
    public float attackRange;
    public float damageDelay;
    public float flashDelay;
    public float arrowSpeed;
    public float sightDistance;
    public float maxWanderTime;
    public float afterHitDelay;
    public float interactionRadius;
    public float updateDistance;
    public int baseDamage;
    public float chargeMultiplier;
    public int maxHealth;
    public int currentHealth;
    public float baseRespawnDelay;
    public GameObject projectile;
    public GameObject chargeEffect;
    public GameObject hitEffect;
    public GameObject chargePositionU;
    public GameObject chargePositionD;
    public GameObject aura;

    
    public bool isPlayerControlled = false;
    private Rigidbody2D myRB2;
    private Animator myAnim;
    private SpriteRenderer mySprite;
    private PlayerController player;
    private SimpleTextPopper textPopper;
    private GameObject chargeEffectInstance;
    private AudioManager audioMan;

    private Vector2 dirInput;
    private bool isUnderSlaveMove = false;
    private float controlDelayToUse;
    private float chargeTimer = 0;
    private float attackDelayTimer = 0;
    private float attackInstanceTimer = 0;
    private float damageTimer = 0;
    private float flashTimer = 0;
    private float respawnTimer = 0;
    private float wanderTimer = 0;
    private float waitTimer = 0;
    private float waitDelayTimer = 0;
    private bool isAttacking = false;
    private bool startedAttack = false;
    private bool isInteracting = false;
    private bool isWandering = false;
    private bool justInteracted = false;
    private bool justPlayed = false;
    private List<GameObject> targets;

    private float stepDelay = .25f;
    private float stepTimer = 0;

	// Use this for initialization
	void Start () {
        //set up references
        myRB2 = GetComponent<Rigidbody2D>();
        mySprite = GetComponent<SpriteRenderer>();
        myAnim = GetComponent<Animator>();
        myAnim.SetInteger("Vertical", -1);

        //try to find player
        player = GameObject.FindObjectOfType<PlayerController>();
        if(player == null)
        {
            Debug.Log("AIScript: Player object not found!");
        }

        //try to get text popper
        textPopper = GameObject.FindObjectOfType<SimpleTextPopper>();
        if (textPopper == null)
        {
            Debug.Log("AIScript: textpopper object not found!");
        }

        audioMan = GameObject.FindObjectOfType<AudioManager>();

        //initialize input
        dirInput = Vector2.zero;

        if(GetComponentInChildren<PlayerController>() != null) //look at me, i'm the player now!
        {
            isPlayerControlled = true;
        }
        else
        {
            aura.GetComponent<SpriteRenderer>().enabled = false;
        }

        targets = new List<GameObject>();
	}
	
	// Update is called once per frame
	void Update () {

        //get player again in case we had to switch
        if (player == null)
        {
            player = GameObject.FindObjectOfType<PlayerController>();
            if (player == null)
            {
                Debug.Log("AIScript: Player object not found!");
            }
        }

        if (Vector2.Distance(player.transform.position, transform.position) < updateDistance) //don't even worry about distant enemies
        {

            //decrease general timers
            if (attackDelayTimer >= 0)
            {
                attackDelayTimer -= Time.deltaTime;
            }
            if (chargeTimer >= 0)
            {
                chargeTimer -= Time.deltaTime;
            }
            if (attackInstanceTimer >= 0)
            {
                attackInstanceTimer -= Time.deltaTime;
            }
            if (damageTimer >= 0)
            {
                damageTimer -= Time.deltaTime;
            }
            if (waitTimer >= 0)
            {
                waitTimer -= Time.deltaTime;
            }
            if (waitDelayTimer >= 0)
            {
                waitDelayTimer -= Time.deltaTime;
            }

            if (currentHealth > 0)
            {
                if (isPlayerControlled)
                {
                    //dirInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
                    GetInput();
                }
                else //control by ai
                {
                    if (waitTimer <= 0)
                    {
                        SetInput();
                    }
                    else //still waiting, set everything to no input
                    {
                        dirInput = Vector2.zero;
                        isAttacking = false;
                        isInteracting = false;
                    }
                }

                //handle timers that should not be updated when dead.
                if (stepTimer >= 0)
                {
                    stepTimer -= Time.deltaTime;
                }
                else if (myAnim.GetBool("isRunning"))
                {
                    audioMan.playSFX("step", transform.position);
                    stepTimer = stepDelay + Random.Range(-0.1f, 0.1f);
                }
                if (flashTimer >= 0)
                {
                    flashTimer -= Time.deltaTime;
                }
                else if (damageTimer >= 0) //flash timer ran out, still invuln so filp
                {
                    flashTimer = flashDelay;
                    mySprite.enabled = !mySprite.enabled;
                }
                else //no more invuln, reset
                {
                    mySprite.enabled = true;
                }

                if (Time.timeScale > 0) //time is passing so handle stuff.
                {
                    HandleMovement();
                    HandleInteract();
                    HandleAttack();

                }
            }
            else
            {
                if (respawnTimer > 0)
                {
                    respawnTimer -= Time.deltaTime; //wait...
                    justPlayed = false;
                }
                else
                {
                    myAnim.SetBool("isDying", false); //RISE

                    if (!justPlayed)
                    {
                        //ploy noise
                        audioMan.playSFX("getUp", transform.position);
                        justPlayed = true;
                    }
                }

                //make sure the bodies don't slide around, good for a laugh though
                myRB2.linearVelocity = Vector2.zero;
            }
        }
	}

    void SetInput()
    {
        targets.Clear();
        //try to get people to attack
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, sightDistance, LayerMask.GetMask("ProjectileHitBoxes"));
        foreach (Collider2D collider in hits)
        {
            //preemptive tag check
            if (tag != collider.tag)
            {
                Transform ptrans = collider.transform.parent;
                if (ptrans != null) //has parent
                {
                    AIController ai = ptrans.gameObject.GetComponent<AIController>();
                    if (ai != null && ai.currentHealth > 0) //parent exists and is alive
                    {
                        targets.Add(collider.gameObject);
                    }
                }
            }
        }

        if(targets.Count > 0)
        {
            isWandering = false; //no time to waste, we have targets to attack!
            GameObject closest = targets[0]; 
            foreach(GameObject go in targets)
            {
                if(Vector2.Distance(go.transform.position, gameObject.transform.position) < Vector2.Distance(closest.transform.position, gameObject.transform.position))
                {
                    closest = go;
                }
            }

            //ifNeed to move
            if (Vector2.Distance(closest.transform.position, transform.position)
                > Mathf.Max(mySprite.bounds.extents.x, mySprite.bounds.extents.y) + 0.1f)
            {
                //set walk direction
                dirInput = closest.transform.position - gameObject.transform.position;
            }
            else
            {
                dirInput = Vector2.zero;
            }

            //set attack
            if(Vector2.Distance(closest.transform.position, transform.position) < attackRange) //in Range, try to attack
            {
                if(attackInstanceTimer > 0) //attacking so keep attacking!
                {
                    isAttacking = true;
                }
                else if(attackInstanceTimer <= 0 && attackDelayTimer <=0) //not yet shooting and can shoot so shoot
                {
                    attackInstanceTimer = Random.Range(minAttackTime, maxAttackTime);
                    attackDelayTimer = attackInstanceTimer + attackDelay;
                }
                else
                {
                    isAttacking = false;
                }
            }
            //else not in range, no attack.
        }
        else
        {
            isWandering = true;
            isAttacking = false; //don't attack without target
            if(wanderTimer > 0)
            {
                wanderTimer -= Time.deltaTime; //tick
            }
            else //eval if we should wander
            {
                wanderTimer = Random.Range(0,maxWanderTime);
                if (dirInput.normalized.magnitude < 0.5f) //just were not wandering so wander
                {
                    dirInput = new Vector2(Random.Range((float)-1, 1), Random.Range((float)-1, 1));
                }
                else //stop wander.
                {
                    dirInput = Vector2.zero;
                }
            }
        }
    }

    void GetInput()
    {

        float delay;

        if (isUnderSlaveMove) //now we must preserve the delay so that things don't get dopplered out. Basically it makes the horde dissipate. go ahead and remove it to see how it affects the dynamics.
        {
            delay = controlDelayToUse;
        }
        else
        {
            //not yet being moved, calculate as expected
            delay = controlDelayPerUnit * Vector2.Distance(player.transform.position, transform.position);
            controlDelayToUse = delay;
            isUnderSlaveMove = true;
        }

        PlayerController pc = player.GetComponent<PlayerController>();
        if (pc == null)
        {
            Debug.Log("No player controller found on player!");
            return;
        }

        PlayerController.InputHistoryEntry entry = pc.controlHistory.Find(x => Time.time >= x.time + delay);
        dirInput = new Vector2(entry.x, entry.y);
        dirInput.Normalize();

        isAttacking = entry.attack;
        isInteracting = entry.interact;

        //now check to see if we stopped moving
        if (dirInput.magnitude < 0.1f)
        {
            isUnderSlaveMove = false;
        }
        //Debug.Log(dirInput);
    }

    void HandleMovement()
    {
        if (isAttacking || isWandering)
        {
            myRB2.linearVelocity = dirInput.normalized * moveSpeed * 0.5f;
            
        }
        else //move slower while attacking
        {
            myRB2.linearVelocity = dirInput.normalized * moveSpeed;
        }

        //handle movement animations

        //for y
        if (dirInput.normalized.y > 0.1f)
        {
            myAnim.SetInteger("Vertical", 1);
        }
        else if (dirInput.normalized.y < -0.1f)
        {
            myAnim.SetInteger("Vertical", -1);
        }

        //for x
        if (dirInput.normalized.x > 0.1f)
        {
            mySprite.flipX = true;
            
        }
        else if (dirInput.normalized.x < -0.1f)
        {
            mySprite.flipX = false;
        }
        //else do not change animation, let it stay in last state.

        //do update charge effect though
        if (chargeEffectInstance != null)
        {
            if(myAnim.GetInteger("Vertical") > 0)
            {
                chargeEffectInstance.transform.localPosition = chargePositionU.transform.localPosition;
            }
            else
            {
                chargeEffectInstance.transform.localPosition = chargePositionD.transform.localPosition;
            }

            if (mySprite.flipX)
            {
                chargeEffectInstance.transform.localPosition = new Vector3(-chargeEffectInstance.transform.localPosition.x,
                                                                        chargeEffectInstance.transform.localPosition.y,
                                                                        chargeEffectInstance.transform.localPosition.z);
            }
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

    void HandleInteract()
    {
        if (isInteracting && !justInteracted)
        {
            bool success = false;
            bool silence = false;
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, interactionRadius, LayerMask.GetMask("Interactables"));
            foreach (Collider2D collider in hits)
            {
                DoorSwitchBehavior dsb = collider.GetComponent<DoorSwitchBehavior>();
                if(dsb != null)
                {
                    success = dsb.ToggleSwitch();
                }
                SpeakerBehavior sb = collider.GetComponent<SpeakerBehavior>();
                if(sb!= null)
                {
                    success = sb.Speak();
                    silence = true;
                }
            }

            if (!silence)
            {
                if (!success)
                {
                    textPopper.MakePopup(transform.position.x, transform.position.y + .5f, new string[] { "huh?" }, gameObject);
                }
                else
                {
                    textPopper.MakePopup(transform.position.x, transform.position.y + .5f, new string[] { "yus", "diddit" }, gameObject);
                }
            }
            else
            {
                if(!success) //failure talking to someone
                {
                    textPopper.MakePopup(transform.position.x, transform.position.y + .5f, new string[] { "..." }, gameObject);
                }
            }

            if(success)
            {
                audioMan.playSFX("good", transform.position);
            }

            //don't double up.
            justInteracted = true;
        }
        else if(!isInteracting && justInteracted)
        {
            justInteracted = false;
        }
    }

    void HandleAttack()
    {
        if (isAttacking && !startedAttack)  //trying to attack, start of new attack
        {
            myAnim.SetBool("isAttacking", true);
            chargeTimer = chargeDelay;
            startedAttack = true;
        }
        else if(isAttacking && startedAttack && chargeTimer <= 0 && chargeEffectInstance == null) //still charging, timer done, no charge made, make charge effect
        {
            chargeEffectInstance = Instantiate(chargeEffect, transform);
            float x, y;
            if (myAnim.GetInteger("Vertical") > 0)
            {
                x = chargePositionU.transform.localPosition.x;
                y = chargePositionU.transform.localPosition.y;
            }
            else
            {
                x = chargePositionD.transform.localPosition.x;
                y = chargePositionD.transform.localPosition.y;
            }
            if(mySprite.flipX)
            {
                x = -x;
            }
            chargeEffectInstance.transform.localPosition = new Vector3(x, y, 0);
            audioMan.playSFX("ready", transform.position);
        }
        else if(!isAttacking && startedAttack) //execute attack
        {
            //calculate direction to attack
            float x;
            if (mySprite.flipX)
            {
                x = 1;
            }
            else
            {
                x = -1;
            }
            Vector3 attackDirection = new Vector3(x, myAnim.GetInteger("Vertical"), 0);
            attackDirection.Normalize();

            if (projectile == null) //melee character
            {
                GameObject go = Instantiate(hitEffect, transform.position, Quaternion.identity);
                ExplosionBehavior exp = go.GetComponent<ExplosionBehavior>();

                //set position
                exp.transform.position += attackDirection * 0.5f;

                //tag it
                exp.tag = tag;

                //set damage
                if (chargeTimer > 0) //early attack
                {
                    exp.damage = baseDamage;
                }
                else //charged attack
                {
                    exp.damage = Mathf.RoundToInt(baseDamage * chargeMultiplier);
                }
                exp.Eval();
            }
            else //projectile character
            {
                //try to get input direction
                if (dirInput.magnitude > 0)
                {
                    attackDirection = new Vector3(dirInput.x, dirInput.y, 0).normalized;
                }

                GameObject go = Instantiate(projectile, transform.position, Quaternion.FromToRotation(projectile.transform.right, attackDirection));
                ProjectileBehavior pb = go.GetComponent<ProjectileBehavior>();

                //setposition
                go.transform.position += attackDirection * 0.5f; ;

                //make sure it is tagged right
                go.tag = tag;

                //set damage & velocity
                if (chargeTimer > 0) //early attack
                {
                    pb.damage = baseDamage;
                    pb.init(attackDirection * arrowSpeed);
                }
                else //charged attack
                {
                    pb.damage = Mathf.RoundToInt(baseDamage * chargeMultiplier);
                    pb.init(attackDirection * arrowSpeed * chargeMultiplier);
                }
            }

            //set anim
            myAnim.SetBool("isAttacking", false);

            //finish chargeffect
            if (chargeEffectInstance != null)
            {
                chargeEffectInstance.GetComponent<Animator>().SetTrigger("done");
            }

            //reset bool
            startedAttack = false;
        }
        //else shouuld be no change, let timers tick
       
    }

    void CancelAttack()
    {
        isAttacking = false;
        startedAttack = false;
        chargeTimer = 0;
        if(chargeEffectInstance != null)
        {
            GameObject.Destroy(chargeEffectInstance);
        }

        myAnim.Play("standDL");
        myAnim.SetBool("isAttacking", false);
    }

    public void DealDamage(int d)
    {
        if (damageTimer <= 0 && currentHealth > 0) 
        {
            damageTimer = damageDelay;
            currentHealth -= d;

            if(currentHealth <= 0)
            {
                //cancel attack only on death
                CancelAttack();

                //grieve
                currentHealth = 0;
                damageTimer = 0;
                respawnTimer = baseRespawnDelay;
                myAnim.SetBool("isDying", true);
                myAnim.SetTrigger("Die");
            }

            if (isPlayerControlled && player.transform.root.gameObject != gameObject) //we're not the player but under control so frenzy
            {
                isPlayerControlled = false;
                aura.GetComponent<SpriteRenderer>().enabled = false;
                //untag
                tag = "Baddie";
                foreach(Transform child in transform)
                {
                    child.tag = tag;
                }
            }

            if(player.gameObject == gameObject)
            {
                audioMan.playSFX("hit", transform.position);
            }
        }
        //else invuln
    }

    public void ReviveToPlayer()
    {
        if (myAnim.GetBool("isDying"))
        {
            myAnim.SetBool("isDying", false);
            isPlayerControlled = true;
            aura.GetComponent<SpriteRenderer>().enabled = true;
            //retag
            tag = "Goodie";
            foreach (Transform child in transform)
            {
                child.tag = tag;
            }

            //ploy noise
            audioMan.playSFX("getUp", transform.position);
        }
    }

    public void setWaitTimer(float t)
    {
        if (waitDelayTimer <= 0)
        {
            waitTimer = t;
            waitDelayTimer = maxAttackTime + 2 * t; // make sure you can get an attack in
        }
    }

    public void setWaitTimer()
    {
        waitTimer = afterHitDelay;
    }
}
