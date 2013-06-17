using UnityEngine;

public class Bullet : MonoBehaviour
{
    // Use this for initialization
    private void Start()
    {
    }

    // Update is called once per frame
    private void Update()
    {
    }


    private void OnTriggerEnter(Collider thisCollider)
    {
        var armyGuy = thisCollider.attachedRigidbody.gameObject.GetComponent<TriggerCollideReciever>();
        
        if (armyGuy != null)
            armyGuy.HandleCollision(thisCollider.name, collider);

        Destroy(gameObject);
    }
}

public abstract class TriggerCollideReciever : MonoBehaviour
{
    public abstract void HandleCollision(string hitObject, Collider collision);
}