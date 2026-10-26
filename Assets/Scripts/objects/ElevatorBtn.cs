using UnityEngine;

public class ElevatorBtn : Interactable
{
    [SerializeField, ReadOnlyInspector] private Elevator elevator;

    protected override void OnAwake()
    {
        elevator = GetComponentInChildren<Elevator>();
        prompt = "Wait for elevator";
        Debug.Assert(elevator != null);
    }

    protected override void OnInteract()
    {
        // Debug.Log("press btn");
        elevator.ButtonCall();
    }


}
