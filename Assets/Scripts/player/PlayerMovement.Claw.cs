using UnityEngine;
using UnityEngine.InputSystem;

public partial class PlayerMovement : MonoBehaviour
{
    [Header("claw")]
    [SerializeField] public ClawParams clawParams;//idfk how to nameit
    [SerializeField] public Transform clawPointer; // object for claw object to reference?
    [SerializeField] public Transform armOrigin; // where the claw fires from / returns to
    [SerializeField] public GameObject claw; // object for claw object to reference
    [SerializeField, ReadOnlyInspector] public float claw_xVel;
    [SerializeField, ReadOnlyInspector] public float claw_yVel;

    [SerializeField, ReadOnlyInspector] public Vector3 landingTarget;
    [SerializeField, ReadOnlyInspector] public Vector3 clawShootOrigin;
    [SerializeField, ReadOnlyInspector] public bool missed;
    [SerializeField, ReadOnlyInspector] string clawState;
    [SerializeField, ReadOnlyInspector] public int clawTimer; //frame count
    public ClawFSM clawFsm;

    // ready? => shoot => hit  => pull => hanging + playerCling => release
    //                 miss => return => ready

    // enum ClawState { Ready, Shooting, Grabbing, Miss, Return }

    // part of UpdateSensors()
    void ClawInit()
    {
        clawFsm = new ClawFSM(this);
        clawFsm.SetState(clawFsm.clawReady);
    }
    void UpdateClawPointerPos()
    {
        if (Mouse.current == null) return;

        Plane plane = new Plane(Vector3.back, new Vector3(0, 0, armOrigin.position.z));
        Ray mouseRay = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (plane.Raycast(mouseRay, out float enterDistance))
        {
            //update pointer pos
            Vector3 mouseWorldPos = mouseRay.GetPoint(enterDistance);
            clawPointer.position = mouseWorldPos;

        }
    }

    void ResetClaw()
    {
        if (Keyboard.current.bKey.wasPressedThisFrame)
        {
            clawFsm.SetState(clawFsm.clawReady);
        }
    }

    void ClawMoveAndSlide()
    {
        Vector3 moveDirection = new Vector3(claw_xVel, claw_yVel, 0f);
        claw.transform.position += moveDirection * Time.deltaTime;
    }

    // use to replace tthe messy copypaste in execuyte clawstate
    public Quaternion LookAt(Vector3 a, Vector3 b)
    {
        Vector3 dir = b - a;
        return Quaternion.LookRotation(dir, Vector3.up);
    }

    //velocity from a to b in timeLimit frames
    public Vector3 LinearVel(Vector3 a, Vector3 b, int timeLimit)
    {
        return (b - a).normalized * Vector3.Distance(b, a) / (timeLimit * (1 / 60f));
    }
}
