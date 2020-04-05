using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnProjectiles : MonoBehaviour
{
    public GameObject m_FirePoint;
    public List<GameObject> m_Projectiles = new List<GameObject>();
    public GameObject m_ObjectToRotate;

    private GameObject m_EffectToSpawn;
    private float m_TimeToFire = 0;

    // Start is called before the first frame update
    void Start()
    {
        m_EffectToSpawn = m_Projectiles[0];
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButton(0) && Time.time >= m_TimeToFire)
        {
            m_TimeToFire = Time.time + 1 / m_EffectToSpawn.GetComponent<ProjectileMove>().m_FireRate;
            SpawnFX();
        }
    }

    void SpawnFX()
    {
        GameObject vfx;

        if (m_FirePoint != null)
        {
            vfx = Instantiate(m_EffectToSpawn, m_FirePoint.transform.position, Quaternion.identity);
            vfx.transform.localRotation = m_ObjectToRotate.transform.rotation;
            
        }
        else
        {
            Debug.Log("No fire point");
        }
    }
}
