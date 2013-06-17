using UnityEngine;

public class ArmyGuyController : TriggerCollideReciever
{
    #region State enum

    public enum State
    {
        Launching,
        Falling,
        DeadFalling,
        FreeFalling,
        Landing,
        DeadLanding,
        Engaged,
        WaitForThrow,
        Throwing,
        Retreating,
        ExplodingInAir,
        ExplodingOnGround
    }

    #endregion

    private const int thorwDistance = 4;

    public Transform FeetCollider;
    public GameObject Grenade;
    public GameObject BloodSplat;
    public Transform HeadCollider;
    public Transform ParachuteCollider;
    public float RunSpeed = 5;
    public Transform TorsoCollider;
    public Transform DammagePosition;

    private GanjaPlant _targetedPlant;

    public tk2dAnimatedSprite anim;
    public Transform character;
    public tk2dSprite chute;
    public State currentState = State.Launching;
    private float lastStateChangeTimeDelta;
    public float parachuteDrag = 465;
    public Rigidbody physObject;

    private float retreatDirection;
    public float swingSpeed = 10;
    private float targetMoveLocation;
    public float throwForce = 8;

    public int KillPoints = 100;
    public int HeadShotBonus = 400;
    public int MidShotBonus = 70;
    public int ParashootBonus = 150;
    public int DeadBodyPoints = 20;
    public int DeadBodyFallingPoints = 50;

    // Use this for initialization
    private void Awake()
    {
    }

    // Update is called once per frame
    private void Update()
    {
        switch (currentState)
        {
            case State.Launching:
                anim.Play("OpenChute");
                currentState = State.Falling;
                chute.renderer.enabled = false;
                break;
            case State.Falling:
                //physObject.AddForceAtPosition (Vector3.up * Time.deltaTime * parachuteDrag, character.TransformPoint (Vector3.forward * -swingSpeed));
                //physObject.AddForceAtPosition(Vector3.up * (parachuteDrag * 0.0181f) , character.TransformPoint(Vector3.forward * -swingSpeed));	
                chute.renderer.enabled = true;
                break;
            case State.FreeFalling:
                chute.renderer.enabled = false;
                break;
            case State.Landing:
                character.rigidbody.useGravity = false;
                character.rigidbody.isKinematic = true;
                character.rigidbody.rotation = Quaternion.Euler(90, 180, 0);
                anim.Play("Landing");
                chute.renderer.enabled = false;
                TryEngageGanja();
                break;
            case State.DeadLanding:
                character.rigidbody.rotation = Quaternion.Euler(90, 180, 0);
                chute.renderer.enabled = false;
                break;
            case State.Retreating:
                character.transform.rotation = Quaternion.Euler(90, 180, 0);
                character.transform.Translate(RunSpeed*Time.deltaTime*retreatDirection, 0, 0);
                if (Mathf.Abs(character.transform.position.x) > 16)
                {
                    Destroy(character.gameObject);
                }
                break;
            case State.WaitForThrow:
                lastStateChangeTimeDelta += Time.deltaTime;
                if (lastStateChangeTimeDelta > 0.5f)
                {
                    currentState = State.Throwing;
                    lastStateChangeTimeDelta = 0;
                    ThrowGrenade();
                }
                break;
            case State.Throwing:
                lastStateChangeTimeDelta += Time.deltaTime;
                if (lastStateChangeTimeDelta > 0.9f)
                {
                    Retreat(targetMoveLocation < character.transform.position.x ? 1 : -1);
                }
                break;
            case State.Engaged:
                float distanceToTravel = targetMoveLocation - character.transform.position.x;
                float frameTravelDistance = RunSpeed*Time.deltaTime;

                character.transform.rotation = Quaternion.Euler(90, 180, 0);

                if (frameTravelDistance > Mathf.Abs(distanceToTravel))
                {
                    Debug.Log("Got to target!");
                    lastStateChangeTimeDelta = 0;
                    currentState = State.WaitForThrow;

                    if (_targetedPlant == null)
                        return;

                    float throwDirection = _targetedPlant.transform.position.x - character.transform.position.x;

                    if (throwDirection < 0)
                    {
                        anim.transform.localScale = new Vector3(1, 1, 1);
                    }
                    else
                    {
                        anim.transform.localScale = new Vector3(-1, 1, 1);
                    }
                    return;
                }

                if (distanceToTravel > 0)
                {
                    character.transform.Translate(frameTravelDistance*(-1f), 0, 0);
                    anim.transform.localScale = new Vector3(-1, 1, 1);
                }
                else
                {
                    character.transform.Translate(frameTravelDistance, 0, 0);
                    anim.transform.localScale = new Vector3(1, 1, 1);
                }

                break;
        }
    }

    private void ThrowGrenade()
    {
        if (_targetedPlant == null || !_targetedPlant.IsAlive)
        {
            Retreat();
            return;
        }

        float throwDirection = _targetedPlant.transform.position.x - character.transform.position.x;

        var grenade =
            Instantiate(Grenade, character.transform.position + new Vector3(0.3f, 1.2f, 0), Grenade.transform.rotation)
            as GameObject;

        grenade.rigidbody.AddForce(throwDirection*throwForce, 7*throwForce, 0);

        if (throwDirection < 0)
        {
            anim.transform.localScale = new Vector3(1, 1, 1);
        }
        else
        {
            anim.transform.localScale = new Vector3(-1, 1, 1);
        }
    }

    private void TryEngageGanja()
    {
        GanjaPlant myTarget = GameManager.Instance.GanjaManager.TryAquireGanjaPlantTarget();

        if (myTarget == null)
        {
            Debug.Log("No more plants!");
            Retreat();
            return;
        }

        _targetedPlant = myTarget;

        Debug.Log("Target acquired: " + myTarget.gameObject.transform.position.x);

        float myTargetX = myTarget.transform.position.x;

        if (myTargetX < -9f)
        {
            targetMoveLocation = myTargetX + thorwDistance;
        }
        else if (myTargetX > 9f)
        {
            targetMoveLocation = myTargetX - thorwDistance;
        }
        else if (myTargetX < transform.position.x)
        {
            targetMoveLocation = myTargetX + thorwDistance;
        }
        else
        {
            targetMoveLocation = myTargetX - thorwDistance;
        }

        character.rigidbody.isKinematic = true;
        character.rigidbody.freezeRotation = true;
        character.rigidbody.rotation = Quaternion.Euler(90, 180, 0);

        //_targetedPlant.Sprite.scale = new Vector3(2f, 2f, 2f);
        //_targetedPlant.Sprite.color = Color.green;
        // _targetedPlant.Repair();

        currentState = State.Engaged;
    }

    private void Retreat(int direction = 0)
    {
        if (_targetedPlant != null)
        {
            _targetedPlant = null;
        }

        if (direction == 1 || character.transform.position.x > 0)
        {
            retreatDirection = 1f;
            anim.transform.localScale = new Vector3(1, 1, 1);
        }
        else
        {
            retreatDirection = -1f;
            anim.transform.localScale = new Vector3(-1, 1, 1);
        }

        //TODO: anim.Play(run);
        //TODO: switch facing direction

        character.rigidbody.isKinematic = true;
        character.rigidbody.freezeRotation = true;
        character.rigidbody.rotation = Quaternion.Euler(90, 180, 0);

        currentState = State.Retreating;
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        switch (currentState)
        {
            case State.Falling:
                physObject.AddForceAtPosition(Vector3.up*Time.deltaTime*parachuteDrag,
                                              character.TransformPoint(Vector3.forward*-swingSpeed));
                break;
            case State.DeadFalling:
                if (!character.rigidbody.freezeRotation && character.rigidbody.isKinematic)
                {
                    character.rigidbody.rotation = Quaternion.Euler(90, 180, 0);
                    character.rigidbody.freezeRotation = true;
                }
                else
                {
                    if (character.rigidbody.isKinematic)
                    {
                        character.rigidbody.isKinematic = false;
                        physObject.velocity = Vector3.up*-1;
                        physObject.drag = 0.8f;
                    }
                    physObject.AddForceAtPosition(Vector3.up*Time.deltaTime*parachuteDrag,
                                                  character.TransformPoint(Vector3.forward*-swingSpeed));
                }
                break;
            case State.FreeFalling:
                if (!character.rigidbody.freezeRotation && character.rigidbody.isKinematic)
                {
                    character.rigidbody.rotation = Quaternion.Euler(90, 180, 0);
                    character.rigidbody.freezeRotation = true;
                }
                else
                {
                    if (character.rigidbody.isKinematic)
                    {
                        character.rigidbody.isKinematic = false;
                        physObject.velocity = Vector3.up*-6.5f;
                        physObject.drag = 0.02f;
                    }
                }
                break;
        }
    }

    public override void HandleCollision(string hitObject, Collider collision)
    {
        ParticleEmitter bloodSpray = null;

        if (collision.gameObject.name == "BaseCollider")
        {
            ParachuteCollider.collider.enabled = false;

            if (currentState == State.DeadFalling)
            {
                Destroy(character.gameObject);
            }
            else if (currentState == State.FreeFalling)
            {
                Destroy(character.gameObject);

                bloodSpray = Instantiate(BloodSplat, DammagePosition.transform.position, BloodSplat.transform.rotation) as ParticleEmitter;
            }

            currentState = State.Landing;
            //FeetCollider.rigidbody.Sleep();
        }
        else if (collision.name == "InstantBullet(Clone)")
        {

            if (currentState == State.Falling || (currentState == State.DeadFalling && hitObject == "Chute"))
            {
                switch (hitObject)
                {
                    case "Head":
                        anim.Play("Dead_Head");
                        currentState = State.DeadFalling;
                        bloodSpray = Instantiate(BloodSplat, DammagePosition.transform.position + new Vector3(0, 0.1f, 0), BloodSplat.transform.rotation) as ParticleEmitter;
                        GameManager.Instance.AddPoints(KillPoints + HeadShotBonus);
                        break;
                    case "Torso":
                        //anim.Play("Dead_Torso_Head");
                        anim.Play("Dead_Torso_Center");
                        currentState = State.DeadFalling;
                        bloodSpray = Instantiate(BloodSplat, DammagePosition.transform.position, BloodSplat.transform.rotation) as ParticleEmitter;
                        GameManager.Instance.AddPoints(KillPoints + MidShotBonus);
                        //anim.Play("Dead_Torso_Head");
                        break;
                    case "Chute":
                        currentState = State.FreeFalling;
                        ParachuteCollider.collider.enabled = false;
                GameManager.Instance.AddPoints(KillPoints + ParashootBonus);
                        //anim.Play("Landing");
                        break;
                    case "Feet":
                    default:
                        anim.Play("Dead_Legs");
                        currentState = State.DeadFalling;
                        bloodSpray = Instantiate(BloodSplat, DammagePosition.transform.position + new Vector3(0, -0.8f, 0), BloodSplat.transform.rotation) as ParticleEmitter;
                GameManager.Instance.AddPoints(KillPoints);
                        break;
                }

                character.rigidbody.isKinematic = true;
                character.rigidbody.rotation = Quaternion.Euler(90, 180, 0);
                //Todo: explode!

                if (bloodSpray != null)
                    bloodSpray.emit = true;
            }
            else if (currentState == State.DeadFalling || currentState == State.FreeFalling)
            {
                Destroy(character.gameObject);

                //Todo: explode!
                bloodSpray = Instantiate(BloodSplat, DammagePosition.transform.position, BloodSplat.transform.rotation) as ParticleEmitter;

                if (currentState == State.FreeFalling)
                    GameManager.Instance.AddPoints(DeadBodyFallingPoints);
                else
                    GameManager.Instance.AddPoints(DeadBodyPoints);
            }
            else if (currentState == State.Launching)
            {
                Destroy(character.gameObject);

                //Todo: explode!
                bloodSpray = Instantiate(BloodSplat, DammagePosition.transform.position, BloodSplat.transform.rotation) as ParticleEmitter;

                GameManager.Instance.AddPoints(DeadBodyFallingPoints + KillPoints);
            }
        }
        else if (collision.name == "Parabomb" || collision.name == "BlastRadius")
        {
            Destroy(character.gameObject);

            //Todo: explode!
            bloodSpray = Instantiate(BloodSplat, DammagePosition.transform.position, BloodSplat.transform.rotation) as ParticleEmitter;

            GameManager.Instance.AddPoints(KillPoints);
        }

        if (bloodSpray != null)
            bloodSpray.emit = true;
    }
}