using System.Collections;
using UnityEngine;

public class Elevator : Interactable
{
    [SerializeField] private float waitTimeMax = 2; // wait till open or wait till close
    [SerializeField, ReadOnlyInspector] private float waitTimer;
    [SerializeField] private bool waitTimerTimeout = false;
    [SerializeField] private bool doorOpen = false;


    protected override void OnAwake()
    {
        prompt = "Enter";
    }

    // wait 0.5s to open the door
    public void ButtonCall()
    {
        if (waitTimer <= 0 && !doorOpen)
        {
            Debug.Log("press button");
            waitTimer = 1f; //Random.Range(0, waitTimeMax * 10f) / 0.1f; //todo add this back
        }
    }

    protected override void OnInteract()
    {
        if (doorOpen) Debug.Log("open elevator ui");
    }

    protected override void OnUpdate()
    {
        if (waitTimer > 0)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0)
            {
                waitTimer = 0;
                waitTimerTimeout = true;
            }
        }


        if (waitTimerTimeout)
        {
            waitTimerTimeout = false;

            if (doorOpen)
            {
                // close the door
                Debug.Log("close door");
                doorOpen = false;
                //fade out ui
            }
            else
            {
                //open the door
                Debug.Log("open door");

                doorOpen = true;
                // add in ui

                // begin timer decrement
                waitTimer = 5;
            }
        }

        if (engaged && doorOpen)
        {
            waitTimer = 5f;
            Debug.Log("occupied, do not decrement");
        }
    }

}
