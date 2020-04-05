using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyObject : MonoBehaviour
{
    public List<GameObject> ObjectsToDestroy = new List<GameObject>(); // see whats getting populated in the inspector

    // Start is called before the first frame update
    void Start()
    {
        ObjectsToDestroy.AddRange(GameObject.FindGameObjectsWithTag("Destructible"));
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnCollisionEnter(Collision collision)
    {
        if (ObjectsToDestroy.Contains(collision.gameObject))
        {
            Destroy(collision.gameObject);
        }
        else
        {
            Debug.Log("Object Not Destructible");
        }
    }
}
