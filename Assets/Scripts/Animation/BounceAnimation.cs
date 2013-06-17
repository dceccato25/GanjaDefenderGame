using UnityEngine;

public class BounceAnimation : MonoBehaviour
{
    public Rigidbody physObject;

    // Use this for initialization
    private void Start()
    {
    }

    // Update is called once per frame
    private void Update()
    {
        if (Random.Range(1, 240) == 2)
        {
            if (physObject != null)
            {
                float height = Random.Range(0.03f, 0.3f);
                physObject.AddForceAtPosition(Vector3.up*height, Vector3.zero, ForceMode.Impulse);
            }
        }
    }
}