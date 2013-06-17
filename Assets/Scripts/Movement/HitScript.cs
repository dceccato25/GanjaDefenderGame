using UnityEngine;

public class HitScript : MonoBehaviour
{
    public TriggerCollideReciever character;

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
        character.HandleCollision(gameObject.name, thisCollider);
    }
}