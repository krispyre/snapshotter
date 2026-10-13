using NUnit.Framework;
using UnityEngine;

public class ElevatorBtn : Interactable
{
    [SerializeField] private Elevator elevator;

    void Awake()
    {
        base.Awake();
        prompt = "Wait for elevator";
        Debug.Assert(elevator != null);
    }

    override protected void OnInteract()
    {
        elevator.ButtonCall();
    }


}
