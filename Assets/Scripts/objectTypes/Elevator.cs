using System.Collections;
using UnityEngine;

public class Elevator : MonoBehaviour
{
    private Collider enterTrigger;
    [SerializeField] private float waitTimeMax = 2; // wait till open or wait till close
    [SerializeField, ReadOnlyInspector] private float waitTimer;
    [SerializeField] private bool waitTimerWasZero = false;

    void Awake()
    {
        enterTrigger.enabled = false;
    }

    // wait 0.5s to open the door
    public void ButtonCall()
    {
        waitTimer = 0.5f; //Random.Range(0, waitTimeMax * 10f) / 0.1f; //todo add this back
    }

    // close the door
    void OnTriggerExit()
    {
        waitTimer = 2;
        ;
    }

    void Update()
    {
        if (waitTimer > 0)
        {
            waitTimer -= Time.deltaTime;
            waitTimerWasZero = false;
        }

        if (!waitTimerWasZero && waitTimer <= 0)
        {
            if (enterTrigger.enabled)
            {
                // close the door
                enterTrigger.enabled = false;
                //fade out ui
            }
            else
            {
                //open the door
                enterTrigger.enabled = true;
                // add in ui
            }
        }
    }

}
