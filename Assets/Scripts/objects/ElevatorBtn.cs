using UnityEngine;

public class ElevatorBtn : MonoBehaviour
{
    [SerializeField] private Elevator elevator;

    void Awake()
    {
        Debug.Assert(elevator != null);
    }

    public void CallElevator(Collider player)
    {
        elevator.ButtonCall();
    }
}
