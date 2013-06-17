using UnityEngine;
using System.Collections;

public class FallingPowerupController : MonoBehaviour
{
    #region State enum

    public enum State
    {
        Launching,
        Falling,
        FreeFalling,
        Landing,
        ExplodingInAir,
        ExplodingOnGround
    }

    #endregion

    public Transform character;
    public Transform DammagePosition;

    public tk2dAnimatedSprite anim;
    public State currentState = State.Launching;
    private float lastStateChangeTimeDelta;
    public float parachuteDrag = 465;
    public float swingSpeed = 10;
    public Rigidbody physObject;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void FixedUpdate()
    {
        switch (currentState)
        {
            case State.Falling:
                physObject.AddForceAtPosition(Vector3.up * Time.deltaTime * parachuteDrag,
                                              character.TransformPoint(Vector3.forward * -swingSpeed));
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
                        physObject.velocity = Vector3.up * -6.5f;
                        physObject.drag = 0.02f;
                    }
                }
                break;
        }
    }
}
