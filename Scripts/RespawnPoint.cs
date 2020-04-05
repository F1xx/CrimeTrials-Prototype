using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Sets the player's respawnpoint to this object's position
public class RespawnPoint : MonoBehaviour
{
    public Material m_InactiveMat;
    public Material m_ActiveMat;

    // Start is called before the first frame update
    void Start()
    {
        if(m_InactiveMat == null || m_ActiveMat == null)
        {
            GetComponent<Renderer>().enabled = false;
        }

        GetComponent<Renderer>().material = m_InactiveMat;

        Collider m_ObjectCollider;
        m_ObjectCollider = GetComponent<Collider>();

        if (m_ObjectCollider)
        {
            m_ObjectCollider.isTrigger = true;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        Player player = other.GetComponent<Player>();

        if (player != null)
        {
            player.SetRespawnPoint(this);

            if (m_ActiveMat)
            {
              GetComponent<Renderer>().material = m_ActiveMat;
            }
        }
    }

    public void SetInactive()
    {
        if (m_InactiveMat)
        {
            GetComponent<Renderer>().material = m_InactiveMat;
        }
    }
}
