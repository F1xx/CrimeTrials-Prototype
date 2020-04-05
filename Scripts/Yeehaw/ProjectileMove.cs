using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileMove : MonoBehaviour
{
    public float m_Speed;
    public float m_FireRate;

    public GameObject m_DoorToDestroy;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (m_Speed != 0)
        {
            transform.position += transform.forward * (m_Speed * Time.deltaTime);
        }
        else
        {
            Debug.Log("No Speed");
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        m_Speed = 0.0f;

        Destroy(gameObject);
    }
}
