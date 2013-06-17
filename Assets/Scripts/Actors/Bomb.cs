using UnityEngine;

public class Bomb : TriggerCollideReciever
{
    private bool _isFactingLeft;
    public float ExplodeDistance = 2.0f;
    private float floorY;
    public Transform grenade;
    protected bool isExploding;
    public tk2dAnimatedSprite sprite;
    public int KillPoints = 200;

    protected virtual void FixedUpdate()
    {
        if (!isExploding)
        {
            if (grenade.transform.position.y < floorY)
            {
                HandeGrenadeExplosion();
            }
        }
    }

    protected void HitCompleteDelegate(tk2dAnimatedSprite sprite, int clipId)
    {
        sprite.animationCompleteDelegate = null;
        Destroy(gameObject);
    }

    protected void HandeGrenadeExplosion()
    {
        foreach (GanjaPlant plant in GameManager.Instance.GanjaManager.GanjaPlants)
        {
            if (plant.CurrentState == GanjaPlant.State.Alive &&
                Mathf.Abs(plant.transform.position.x - grenade.transform.position.x) <= ExplodeDistance)
            {
                plant.StartBurning();
            }
        }

        grenade.rigidbody.isKinematic = true;

        isExploding = true;
        sprite.Play("Explode");

        sprite.animationCompleteDelegate = HitCompleteDelegate;

        sprite.transform.Translate(Vector3.up);
    }

    // Update is called once per frame
    protected void Start()
    {
        if (transform.localScale.x < 0)
            _isFactingLeft = true;

        floorY = GameManager.Instance.GanjaManager.transform.position.y;
    }

    // Update is called once per frame
    protected void Update()
    {
        if (_isFactingLeft)
        {
            transform.Rotate(Vector3.forward, -30*Time.deltaTime, Space.World);
        }
        else
        {
            transform.Rotate(Vector3.forward, 30*Time.deltaTime, Space.World);
        }
    }

    public override void HandleCollision(string hitObject, Collider collision)
    {
        if (collision.name == "InstantBullet(Clone)")
        {            
            grenade.rigidbody.isKinematic = true;

            isExploding = true;
            sprite.Play("Explode");

            sprite.animationCompleteDelegate = HitCompleteDelegate;

            sprite.transform.Translate(Vector3.up);

            GameManager.Instance.AddPoints(KillPoints);
        }
    }
}