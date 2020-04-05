using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(AudioSource))]
public class WinCondition : MonoBehaviour
{
    private bool m_Display = false;
    private GUIStyle guiStyle = new GUIStyle();

    AudioSource m_audioData;

    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;

        m_audioData = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        //if the player has entered
        if(other.gameObject.CompareTag("Player"))
        {
            Player player = other.gameObject.GetComponent<Player>();

            if(player)
            {
                player.SetMovementState(Player.MovementState.Disable);

                m_Display = true;
                guiStyle.fontSize = 50;
                guiStyle.alignment = TextAnchor.MiddleCenter;

                m_audioData.Play(0);
            }    
        }
    }

    void OnGUI()
    {
        if (m_Display)
        {
            GUI.Label(new Rect(Screen.width / 2, Screen.height / 2, 200f, 200f), "YOU WIN", guiStyle);
        }
    }
}
