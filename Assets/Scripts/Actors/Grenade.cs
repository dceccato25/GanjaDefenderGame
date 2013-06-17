using UnityEngine;

public class Grenade : MonoBehaviour
{
    public float ExplodeDistance = 2.0f;
    private float floorY;
    public Transform grenade;
    protected bool isExploding;
    public tk2dAnimatedSprite sprite;

    // Use this for initialization
    protected virtual void Start()
    {
        floorY = GameManager.Instance.GanjaManager.transform.position.y;
    }

    // Update is called once per frame
    protected virtual void Update()
    {
    }

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
}