using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootTimer : MonoBehaviour
{
    public bool bIsTimerStarted { get; private set; }
    public bool bHasTimerEnded { get; private set; }

    float TimerLength = 1.5f;

    // Start is called before the first frame update
    void Start()
    {
        bIsTimerStarted = false;
        bHasTimerEnded = true;
    }

    // Update is called once per frame
    void Update()
    {

    }

    void FixedUpdate()
    {
        if (bIsTimerStarted)
        {
            TimerLength -= Time.deltaTime;
            //print("Timer: " + TimerLength);

            if (TimerLength <= 0)
            {
                StopTimer();
            }
        }
    }

    public void StartTimer()
    {
        bIsTimerStarted = true;
        bHasTimerEnded = false;
    }

    public void StopTimer()
    {
        bHasTimerEnded = true;
        bIsTimerStarted = false;
        TimerLength = 1.5f;
    }

    public void StopTimerEarly()
    {
        bHasTimerEnded = false;
        bIsTimerStarted = false;
        TimerLength = 1.5f;
    }
}
