using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(LineRenderer))]

public class LazorBeam : MonoBehaviour
{
    // Mask for raycast to ignore
    int m_RayMaskList;

    // Laser attributes
    public float m_LaserWidth = 1.0f;
    public float m_Noise = 1.0f;
    public float m_MaxLength = 50.0f;
    public Color m_Color = Color.green;

    // Creating line renderer
    private LineRenderer m_LineRenderer;
    int m_Length = 0;
    Vector3[] m_Position; // list of positions for line renderer

    // Cache any transforms here
    Transform m_Transform;
    Transform m_EndEffectTransform;

    // Particle system, sparks created by laser on hit
    public ParticleSystem m_EndEffect;
    Vector3 offset;

    // Start is called before the first frame update
    private void Start()
    {
        m_LineRenderer = GetComponent<LineRenderer>();

        m_LineRenderer.material.color = m_Color;

        // List of things for raycast to ignore
        m_RayMaskList = ~LayerMask.GetMask("Player", "IgnoreRaycast");

        // Line initialization
        m_LineRenderer.startWidth = m_LaserWidth;
        m_LineRenderer.endWidth = m_LaserWidth;
        m_Transform = transform;
        offset = new Vector3(0, 0, 0);
        m_EndEffect = GetComponentInChildren<ParticleSystem>();
        if(m_EndEffect)
        {
            m_EndEffectTransform = m_EndEffect.transform;
        }
    }

    // Update is called once per frame
    private void Update()
    {
        RenderLaser();        
    }

    private void UpdateLength()
    {
        // Raycast from the location
        RaycastHit[] hit;
        hit = Physics.RaycastAll(m_Transform.position, m_Transform.forward, m_MaxLength, m_RayMaskList);

        int i = 0;
        while(i < hit.Length)
        {
            // Check to make sure we aren't hitting triggers but colliders
            if(!hit[i].collider.isTrigger)
            {
                m_Length = (int)Mathf.Round(hit[i].distance) + 2;
                m_Position = new Vector3[m_Length];

                // Move EndEffect particle system to hit point and start playing
                if(m_EndEffect)
                {
                    m_EndEffectTransform.position = hit[i].point;
                    if(!m_EndEffect.isPlaying)
                    {
                        m_EndEffect.Play();
                    }

                    m_LineRenderer.positionCount = m_Length;
                    return;
                }
            }
            // Keep looping to find all hit points it might be hitting
            i++;
        }
        // If we're not hitting anything, don't play particle effect
        if(m_EndEffect)
        {
            if(m_EndEffect.isPlaying)
            {
                m_EndEffect.Stop();
            }

            m_Length = (int)m_MaxLength;
            m_Position = new Vector3[m_Length];
            m_LineRenderer.positionCount = m_Length;
        }
    }

    private void RenderLaser()
    {
        // Shoot our laserbeam forward
        UpdateLength();

        m_LineRenderer.startColor = m_Color;
        m_LineRenderer.endColor = m_Color;

        // Move through array
        for(int i = 0; i < m_Length; i++)
        {
            // Set position to current location, project in forward direction of parent object
            offset.x = m_Transform.position.x + i * m_Transform.forward.x + Random.Range(-m_Noise, m_Noise);
            offset.z = i * m_Transform.forward.z + Random.Range(-m_Noise, m_Noise) + m_Transform.position.z;
            m_Position[i] = offset;
            m_Position[0] = m_Transform.position;

            m_LineRenderer.SetPosition(i, m_Position[i]);
        }
    }
}
