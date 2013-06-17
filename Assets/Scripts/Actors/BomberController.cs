using System;
using Assets.Scripts.Shared;
using UnityEngine;
using Object = UnityEngine.Object;

public class BomberController : SpawnableEnemyBase, ISpawnableEnemy
{
    #region State enum

    public enum State
    {
        Dropping,
        Running,
        Exploding,
        Dead
    }

    #endregion

    private const float SPAWN_SEPARATION = (17f/3f);

    public Transform Character;
    public tk2dAnimatedSprite ExplodeAnimation;
    public Rigidbody PhysObject;
    public tk2dSprite Plane;
    public Transform SpawnObject;
    public Transform SpawnPoint;
    private float _explosionStartTime;
    private bool _isBombing;
    private float _lastSpawnedX;
    private int _spawnedObjects;
    public State currentState = State.Running;
    public int HealthSetting;
    public int KillPointsPerHitPoint = 2500;

    #region ISpawnableEnemy Members

    public override float Speed { get; set; }
    public override float Bias1 { get; set; }
    public override float Bias2 { get; set; }
    public override int SpawnCount { get; set; }
    public override Direction Direction { get; set; }

    public override int Health
    {
        get { return HealthSetting; }
        set { HealthSetting = value; }
    }

    #endregion

    // Use this for initialization
    private void Awake()
    {
        ExplodeAnimation.renderer.enabled = false;
    }

    // Update is called once per frame
    private void Update()
    {
        if (Math.Abs(Character.position.x) > 26)
        {
            Destroy(Character.gameObject);
            return;
        }

        switch (currentState)
        {
            case State.Running:
                Character.Translate(Character.right*(Speed*Time.deltaTime*(Direction == Direction.Right ? 1 : -1)));
                ExplodeAnimation.renderer.enabled = false;

                float spawnX = SpawnPoint.position.x;

                if ((Direction == Direction.Right && Character.transform.position.x >= ((15f*Bias1) - 15f)) ||
                    (Direction == Direction.Left && Character.transform.position.x <= ((-15f*Bias1) + 15f)))
                {
                    if (_spawnedObjects < 3)
                    {
                        if (_spawnedObjects == 0 || Math.Abs(_lastSpawnedX - spawnX) > SPAWN_SEPARATION)
                        {
                            SpawnItem(SpawnPoint.position);

                            return;
                        }
                    }
                }
                break;
            case State.Exploding:
                _explosionStartTime += Time.deltaTime;

                if (_explosionStartTime > 0.01)
                {
                    Plane.renderer.enabled = false;
                }
                break;
        }
    }

    private void HitCompleteDelegate(tk2dAnimatedSprite sprite, int clipId)
    {
        ExplodeAnimation.animationCompleteDelegate = null;
        Destroy(gameObject);
    }

    public override void HandleCollision(string hitObject, Collider collision)
    {
        //if(collision.rigidbody != null)		
        //Debug.Log(collision.name + " => " + hitObject);

        if (collision.name == "InstantBullet(Clone)")
        {
            Health--;

            if (Health < 0)
            {
                //Destroy(gameObject);
                _explosionStartTime = 0;
                currentState = State.Exploding;
                ExplodeAnimation.renderer.enabled = true;

                if (!ExplodeAnimation.isPlaying())
                    ExplodeAnimation.Play("Explode");

                Plane.collider.enabled = false;

                ExplodeAnimation.animationCompleteDelegate = HitCompleteDelegate;

                GameManager.Instance.AddPoints(KillPointsPerHitPoint);
            }
        }
    }

    private void SpawnItem(Vector3 spawnPosition)
    {
        _spawnedObjects++;
        _lastSpawnedX = spawnPosition.x;
        Object bomb = Instantiate(SpawnObject, spawnPosition, Quaternion.Euler(90f, 180f, 0f));

        if (Direction == Direction.Right)
        {
            ((Transform) bomb).transform.localScale = new Vector3(-1, 1, 1);
            ((Transform) bomb).rigidbody.AddForce(250, 0, 0);
        }
        else
        {
            ((Transform) bomb).rigidbody.AddForce(-250, 0, 0);
        }
    }
}