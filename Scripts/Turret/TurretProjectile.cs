using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]

public class TurretProjectile : MonoBehaviour
{
   
    

    // Start is called before the first frame update
    void Start()
    { 
    }

    // Update is called once per frame
    void FixedUpdate()
    {
    }

    private void OnTriggerEnter(Collider other)
    {
        //print("COLLIDES");
        if(other.gameObject.name != "Player")
        {
            Destroy(gameObject);
        }
    }
}
