using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthComponent : MonoBehaviour
{
    public float m_MaxHealth = 100.0f;
    public float m_MinHealth = 0.0f;
    public float m_CurrentHealth = 100.0f;

    public void OnTakeDamage(float amount)
    {
        m_CurrentHealth = m_CurrentHealth - amount;

        CheckHealthBounds();
    }

    private void OnDeath()
    {
        Player player = GetComponent<Player>();

        if (player != null)
        {
            player.OnDeath();
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public bool IsAlive()
    {
        return m_CurrentHealth > m_MinHealth;
    }

    //Sets the object's health back to initial values
    public void Reset()
    {
        m_CurrentHealth = m_MaxHealth;
    }

    public void Heal(float amount)
    {
        m_CurrentHealth += amount;

        CheckHealthBounds();
    }

    //true if has less than max health
    public bool NeedsHealing()
    {
        return m_CurrentHealth != m_MaxHealth;
    }

    private void CheckHealthBounds()
    {
                if (m_CurrentHealth <= m_MinHealth)
        {
            OnDeath();
        }
        else if (m_CurrentHealth > m_MaxHealth)
        {
            m_CurrentHealth = m_MaxHealth;
        }
    }
}
