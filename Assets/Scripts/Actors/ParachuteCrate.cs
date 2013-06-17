using UnityEngine;
using System.Collections;
using State = FallingPowerupController.State;

public class ParachuteCrate : TriggerCollideReciever
{
    public FallingPowerupController FallingPowerupController;

    public Transform character;
    public Transform DammagePosition;

    public Transform ParachuteCollider;
    public Transform ObjectCollider;

    public tk2dAnimatedSprite anim;
    private float lastStateChangeTimeDelta;
    public Rigidbody physObject;

    public GameObject Explode;

    // Use this for initialization
    void Start()
    {

    }

    private void Update()
    {
        switch (FallingPowerupController.currentState)
        {
            case State.Launching:
                anim.Play("Crate_Live");
                FallingPowerupController.currentState = State.Falling;
                break;
            case State.Falling:
                break;
            case State.FreeFalling:
                anim.Play("Crate_Fall");
                break;
        }
    }

    private void HandleHitInAir()
    {
        ParticleEmitter bloodSpray = null;

        Destroy(character.gameObject);

        bloodSpray = Instantiate(Explode, DammagePosition.transform.position, Explode.transform.rotation) as ParticleEmitter;

        if (bloodSpray != null)
            bloodSpray.emit = true;
    }

    private void HandleHitGround()
    {
        Destroy(character.gameObject);
    }

    public override void HandleCollision(string hitObject, Collider collision)
    {
        if (collision.gameObject.name == "BaseCollider")
        {
            ParachuteCollider.collider.enabled = false;

            HandleHitInAir();
        }
        else if (collision.name == "InstantBullet(Clone)")
        {
            if (FallingPowerupController.currentState == State.Falling)
            {
                switch (hitObject)
                {
                    case "Chute":
                        FallingPowerupController.currentState = State.FreeFalling;
                        ParachuteCollider.collider.enabled = false;
                        //GameManager.Instance.AddPoints(KillPoints + ParashootBonus);
                        break;
                    default:
                        HandleHitInAir();
                        break;
                }

                character.rigidbody.isKinematic = true;
                character.rigidbody.rotation = Quaternion.Euler(90, 180, 0);
            }
            else if (FallingPowerupController.currentState == State.Launching)
            {
                HandleHitInAir();
            }
        }
        else if (collision.name == "Parabomb" || collision.name == "BlastRadius")
        {
            HandleHitInAir();
        }
    }
}
