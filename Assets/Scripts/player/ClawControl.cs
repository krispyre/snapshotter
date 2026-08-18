using System;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public partial class PlayerMovement : MonoBehaviour
{
    [Header("claw")]
    [SerializeField] private ClawParams clawParams;//idfk how to nameit
    [SerializeField] Transform clawPointer; // object for claw object to reference?
    [SerializeField] GameObject claw; // object for claw object to reference
    [SerializeField, ReadOnlyInspector] Vector3 landingTarget;
    [SerializeField, ReadOnlyInspector] bool missed;
    [SerializeField, ReadOnlyInspector] ClawState clawState = ClawState.Ready;
    [SerializeField, ReadOnlyInspector] int clawTimer; //frame count

    //todo is claw shooting from transform.position. decouple

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

        }
    }

    void SetClawState(bool shootPressed)
    {
        //shoot 
        switch (clawState)
        {
            case ClawState.Ready:
                //add delay todo
                {
                    if (shootPressed)
                    {
                        Vector3 aimDirection = clawPointer.position - transform.position;
                        Ray clawRay = new Ray(transform.position, aimDirection);

                        if (Physics.Raycast(clawRay, out RaycastHit hitInfo, clawParams.armLength))
                        {
                            landingTarget = hitInfo.point;
                            claw.transform.position = landingTarget;
                            missed = false;
                        }
                        else
                        {
                            Debug.LogAssertion("claw miss");
                            missed = true;
                        }
                        clawTimer = clawParams.flyTime;
                        // todotodotodo 
                        // claw.velocity = Vector3.Distance(landingTarget, transform.position) / (clawParams.flyTime * (1 / 60f));

                        // todo where to decide player state

                        state = PlayerState.Clawing;
                        clawState = ClawState.Shooting;
                    }
                }
                break;
            case ClawState.Shooting:
                //should arrive when 0
                if (clawTimer > 0)
                {
                    clawTimer--;

                }
                else
                {
                    clawTimer = clawParams.pullDelay;
                    if (missed)
                    {
                        clawState = ClawState.Miss;

                    }
                    else
                    {
                        clawState = ClawState.Hit;
                    }
                }
                break;
            case ClawState.Hit:
                if (clawTimer > 0)
                {
                    clawTimer--;
                }
                else //time to transition to the next!!
                {
                    clawTimer = clawParams.pullTime;
                    clawState = ClawState.Pulling;
                }
                break;

            case ClawState.Miss:
                if (clawTimer > 0)
                {
                    clawTimer--;

                }
                else
                {
                    clawTimer = clawParams.returnTime;
                    clawState = ClawState.Return;
                }
                break;

            case ClawState.Pulling:
                if (clawTimer > 0)
                {
                    // Vector3 vel = (landingTarget - transform.position).normalized * Vector3.Distance(landingTarget, transform.position) / (clawParams.flyTime * (1 / 60f));
                    // xVel = vel.x;
                    // yVel = vel.y;

                    clawTimer--;
                }
                else
                {
                    clawState = ClawState.Hanging;
                }
                break;
            case ClawState.Hanging:
                if (shootPressed)//todo should not capture???
                {
                    clawTimer = clawParams.returnTime;
                    clawState = ClawState.Return;
                }
                break;
            case ClawState.Return:
                if (clawTimer > 0)
                {
                    clawTimer--;
                }
                else
                {
                    //if claw is near body
                    clawState = ClawState.Ready;
                }
                break;

        }
    }
}
