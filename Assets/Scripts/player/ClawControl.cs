using System;
using System.Drawing;
using UnityEngine;
using UnityEngine.InputSystem;

public partial class PlayerMovement : MonoBehaviour
{
    [Header("claw")]
    [SerializeField] private ClawParams clawParams;//idfk how to nameit
    [SerializeField] Transform clawPointer; // object for claw object to reference
    [SerializeField] GameObject claw; // object for claw object to reference
    [SerializeField, ReadOnlyInspector] Vector3 landingTarget;
    [SerializeField, ReadOnlyInspector] ClawState clawState = ClawState.Ready;
    [SerializeField, ReadOnlyInspector] int clawTimer; //frame count

    // ready? => shoot => hit  => pull => hanging + playerCling => release
    //                 miss => return => ready
    enum ClawState { Ready, Shooting, Hit, Hanging, Miss, Pulling, Return }

    // part of UpdateSensors()
    void UpdateMousePos()
    {
        if (Mouse.current == null) return;

        Plane plane = new Plane(Vector3.back, new Vector3(0, 0, transform.position.z));
        Ray mouseRay = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (plane.Raycast(mouseRay, out float enterDistance))
        {
            //update pointer pos
            Vector3 mouseWorldPos = mouseRay.GetPoint(enterDistance);

            clawPointer.position = mouseWorldPos;

            //shoot 
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                //move to setstate()
                Shoot(mouseWorldPos);
            }
        }
    }

    private void Shoot(Vector3 mousePos)
    {
        Vector3 aimDirection = mousePos - transform.position;
        Ray clawRay = new Ray(transform.position, aimDirection);

        if (Physics.Raycast(clawRay, out RaycastHit hitInfo, clawParams.armLength))
        {
            landingTarget = hitInfo.point;
            claw.transform.position = landingTarget;
            clawState = ClawState.Shooting;
            clawTimer = clawParams.arriveTime;
        }
        else
        {
            Debug.LogAssertion("claw miss");
            clawState = ClawState.Miss;
            clawTimer = clawParams.arriveTime;
        }
    }
}
