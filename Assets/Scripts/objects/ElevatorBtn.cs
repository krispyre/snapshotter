using UnityEngine;

public class ElevatorBtn : Interactable
{
    [SerializeField] private Elevator elevator;

    protected override void OnAwake()
    {
        prompt = "Wait for elevator";
        Debug.Assert(elevator != null);
    }

    protected override void OnInteract()
    {
        // Debug.Log("press btn");
        elevator.ButtonCall();
    }


}
