using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Hypothetically this can be used to heal as well ironically
//Note this requires the collider of this object to be set as a trigger
[RequireComponent(typeof(Collider))]
public class PainVolume : MonoBehaviour
{
    public float m_EntryDamage = 0; //Damage Taken Upon First Contact
    public float m_DPS = 0; //Damage taken per second while remaining within
    public float m_ExitDamage = 0; //Damage taken upon leaving the volume

    void Start()
    {
        Collider m_ObjectCollider;

        m_ObjectCollider = GetComponent<Collider>();

        if (m_ObjectCollider)
        {
            m_ObjectCollider.isTrigger = true;
        }
    }

    //Called only upon first contact
    void OnTriggerEnter(Collider other)
    {
        if (m_EntryDamage != 0.0f)
        {
            HealthComponent health = other.GetComponent<HealthComponent>();

            if (health != null)
            {
                health.OnTakeDamage(m_EntryDamage);
            }
        }
    }

    //Called every frame while within
    void OnTriggerStay(Collider other)
    {
        if (m_DPS != 0.0f)
        {
            HealthComponent health = other.GetComponent<HealthComponent>();

            if (health != null)
            {
                health.OnTakeDamage(m_DPS * Time.deltaTime);
            }
        }
    }

    //called on object exit
    void OnTriggerExit(Collider other)
    {
        if (m_ExitDamage != 0.0f)
        {
            HealthComponent health = other.GetComponent<HealthComponent>();

            if (health != null)
            {
                health.OnTakeDamage(m_ExitDamage);
            }
        }
    }
}