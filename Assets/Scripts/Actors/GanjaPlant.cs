using UnityEngine;

public class GanjaPlant : MonoBehaviour
{
    #region State enum

    public enum State
    {
        Alive,
        Burning,
        Repairing,
        Burned,
        Poisoned,
        Exploded
    }

    #endregion

    private const string AliveAnim = "GanjaAlive";
    private const string BurningAnim = "GanjaBurning";
    private const string GanjaDead = "GanjaDead";
    private const string RepairingAnim = "GanjaRepairing";
    private const string PoisonedAnim = "GanjaPoisoned";
    private const string ExplodedAnim = "GanjaExploded";

    public State CurrentState;
    public GanjaManager Manager;
    public tk2dAnimatedSprite Sprite;
    private float lastStateChangeDelta;

    public bool IsAlive
    {
        get
        {
            return
                CurrentState == State.Alive ||
                CurrentState == State.Burning ||
                CurrentState == State.Repairing;
        }
    }

    // Use this for initialization
    private void Start()
    {
    }

    private void Destroy()
    {
    }

    public void StartExplode()
    {
        CurrentState = State.Exploded;
        Sprite.Play(ExplodedAnim);
    }

    public void StartBurning()
    {
        lastStateChangeDelta = 0;
        CurrentState = State.Burning;
        Sprite.Play(BurningAnim);
        Sprite.scale = new Vector3(1.9f, 1.9f, 1.9f);
    }

    public void StartPoisoning()
    {
        CurrentState = State.Poisoned;
        Sprite.Play(PoisonedAnim);
    }

    public void Repair()
    {
        lastStateChangeDelta = 0;
        CurrentState = State.Repairing;
        Sprite.Play(RepairingAnim);
        Sprite.scale = new Vector3(1.9f, 1.9f, 1.9f);
    }

    public void Reset()
    {
        lastStateChangeDelta = 0;
        CurrentState = State.Alive;
        Sprite.Play(AliveAnim);
        Sprite.scale = new Vector3(1f, 1f, 1f);
    }

    // Update is called once per frame
    private void Update()
    {
        lastStateChangeDelta += Time.deltaTime;

        switch (CurrentState)
        {
            case State.Burning:
                if (lastStateChangeDelta > 4)
                {
                    Sprite.Play(GanjaDead);
                    CurrentState = State.Burned;
                }
                break;
            case State.Repairing:
                if (lastStateChangeDelta > 3)
                {
                    Reset();
                }
                break;
        }
    }
}