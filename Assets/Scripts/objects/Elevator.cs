using System.Collections;
using UnityEngine;

public class Elevator : Interactable
{
    [SerializeField] private float waitTimeMax = 2; // wait till open or wait till close
    [SerializeField, ReadOnlyInspector] private float waitTimer;
    [SerializeField] private bool waitTimerTimeout = false;


    protected override void OnAwake()
    {
        prompt = "Enter";
    }

    void Start()
    {
        enterTrigger.enabled = false;

    }

    // wait 0.5s to open the door
    public void ButtonCall()
    {
        Debug.Log("press button");
        waitTimer = 1f; //Random.Range(0, waitTimeMax * 10f) / 0.1f; //todo add this back
    }

    // close the door
    protected override void OnPlayerExit(Collider other)
    {
        waitTimer = 2;
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

            if (enterTrigger.enabled)
            {
                // close the door
                Debug.Log("close door");
                enterTrigger.enabled = false;
                //fade out ui
            }
            else
            {
                //open the door
                Debug.Log("open door");

                enterTrigger.enabled = true;
                // add in ui
            }
        }
    }

}
