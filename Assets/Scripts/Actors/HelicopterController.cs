using System;
using Assets.Scripts.Shared;
using UnityEngine;
using Random = UnityEngine.Random;

public interface ISpawnableEnemy
{
    float Speed { get; set; }
    float Bias1 { get; set; }
    float Bias2 { get; set; }
    int SpawnCount { get; set; }
    int Health { get; set; }
    Direction Direction { get; set; }
}

public abstract class SpawnableEnemyBase : TriggerCollideReciever, ISpawnableEnemy
{
    #region ISpawnableEnemy Members

    public abstract float Speed { get; set; }
    public abstract float Bias1 { get; set; }
    public abstract float Bias2 { get; set; }
    public abstract int SpawnCount { get; set; }
    public abstract int Health { get; set; }
    public abstract Direction Direction { get; set; }

    #endregion
}

public class HelicopterController : SpawnableEnemyBase, ISpawnableEnemy
{
    #region State enum

    public enum State
    {
        Bursting,
        Running,
        Exploding,
        Dead
    }

    #endregion

    private const float SPAWN_SEPARATION = 1f;

    public Transform Character;
    public tk2dAnimatedSprite Chopper;
    public tk2dAnimatedSprite ExplodeAnimation;
    public Rigidbody PhysObject;
    public Transform SpawnObject;
    public Transform SpawnPoint;
    private float _explosionStartTime;
    private bool _isBursting;
    private float _lastSpawnedX;
    private int _spawnedObjects;
    public State currentState = State.Running;
    public int HealthSetting;
    public int KillPointsPerHitPoint = 2500;
    public ParticleEmitter Smoke;

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

        if (Direction == Direction.Right)
        {
        }
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

                if (_spawnedObjects < SpawnCount)
                {
                    float spawnX = SpawnPoint.position.x;

                    if (Math.Abs(spawnX) < 12 &&
                        ((_spawnedObjects == 0) || Math.Abs(_lastSpawnedX - spawnX) > SPAWN_SEPARATION))
                    {
                        Vector3 spawnPosition = SpawnPoint.position;

                        if (_isBursting)
                        {
                            SpawnItem(spawnPosition);

                            if (Random.Range(0, 0.99f) > (1.0f - ((1.0f - Bias1)*0.1f)))
                                _isBursting = false;

                            return;
                        }

                        float distanceLeftForSpawning = Direction == Direction.Right ? 12 - spawnX : 12 + spawnX;

                        float emitRate = (SpawnCount - _spawnedObjects)/(distanceLeftForSpawning/Speed);

                        if (Random.Range(0.0f, 1.0f) <= emitRate*Time.deltaTime*(1.1f - Bias1))
                        {
                            SpawnItem(spawnPosition);
                        }

                        if (Random.Range(0.0f, 1.0f) <= emitRate*Time.deltaTime*Bias1)
                        {
                            _isBursting = true;
                        }
                    }

                    if (Health <=2)
                    {
                        Smoke.enabled = true;
                        //Smoke.Emit(3);
                        Smoke.emit = true;
                        Smoke.minEnergy = 3;
                        Smoke.maxEnergy = 3;
                    }
                    else if (Health <=1)
                    {
                        Smoke.enabled = true;
                        Smoke.emit = true;
                        //Smoke.Emit(3);
                        Smoke.minEnergy = 6;
                        Smoke.maxEnergy = 6;
                    }
                }
                break;
            case State.Exploding:
                _explosionStartTime += Time.deltaTime;

                if (_explosionStartTime > 0.01)
                {
                    Chopper.renderer.enabled = false;
                }

                Smoke.enabled = true;
                Smoke.minEnergy = 6;
                Smoke.maxEnergy = 6;
                Smoke.emit = true;
                Smoke.enabled = false;

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

                Chopper.collider.enabled = false;

                ExplodeAnimation.animationCompleteDelegate = HitCompleteDelegate;

                GameManager.Instance.AddPoints(KillPointsPerHitPoint);
            }
        }
        else if (collision.name == "Parabomb" || collision.name == "BlastRadius")
        {
            Debug.Log("BOMB!");
            Health = 0;
            //if (Health < 0)
            {

                //Destroy(gameObject);
                _explosionStartTime = 0;
                currentState = State.Exploding;
                ExplodeAnimation.renderer.enabled = true;

                if (!ExplodeAnimation.isPlaying())
                    ExplodeAnimation.Play("Explode");

                Chopper.collider.enabled = false;

                ExplodeAnimation.animationCompleteDelegate = HitCompleteDelegate;

                GameManager.Instance.AddPoints(KillPointsPerHitPoint);
            }
        }
    }

    private void SpawnItem(Vector3 spawnPosition)
    {
        _spawnedObjects++;
        _lastSpawnedX = spawnPosition.x;
        Instantiate(SpawnObject, spawnPosition, Quaternion.Euler(73f, 270f, 90f));
    }
}