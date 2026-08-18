using System;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public partial class PlayerMovement : MonoBehaviour
{
    [Header("claw")]
    [SerializeField] private ClawParams clawParams;//idfk how to nameit
    [SerializeField] Transform clawPointer; // object for claw object to reference?
    [SerializeField] GameObject claw; // object for claw object to reference
    [SerializeField, ReadOnlyInspector] float claw_xVel;
    [SerializeField, ReadOnlyInspector] float claw_yVel;

    [SerializeField, ReadOnlyInspector] Vector3 landingTarget;
    [SerializeField, ReadOnlyInspector] bool missed;
    [SerializeField, ReadOnlyInspector] ClawState clawState = ClawState.Ready;
    [SerializeField, ReadOnlyInspector] int clawTimer; //frame count

    //todo is claw shooting from transform.position. decouple

    // ready? => shoot => hit  => pull => hanging + playerCling => release
    //                 miss => return => ready
    enum ClawState { Ready, Shooting, Grabbing, Miss, Return }

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
                        Debug.Log("shoot");
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
                            landingTarget = transform.position + clawParams.armLength * aimDirection;
                            Debug.LogAssertion("claw miss");
                            missed = true;
                        }

                        // decide player state here!!!!!!!!!!!!
                        prevState = state;

                        claw.transform.position = transform.position;
                        claw.SetActive(true);
                        state = PlayerState.Clawing;
                        clawState = ClawState.Shooting;
                    }
                }
                break;
            case ClawState.Shooting:
                Debug.Log("owowoo");
                if (Vector3.Distance(claw.transform.position, landingTarget) < 0.02)
                {
                    Debug.Log("arrive");
                    claw.transform.position = landingTarget;
                    clawTimer = clawParams.pullDelay;
                    if (missed)
                    {
                        clawState = ClawState.Miss;

                    }
                    else
                    {
                        clawState = ClawState.Grabbing;
                    }
                }
                break;

            case ClawState.Miss:
                if (clawTimer > 0)
                {
                    clawTimer--;

                }
                else
                {
                    clawState = ClawState.Return;
                }
                break;
            case ClawState.Grabbing:
                if (shootPressed)
                {
                    clawTimer = clawParams.returnTime;
                    clawState = ClawState.Return;
                }
                break;
            case ClawState.Return:
                if (Vector3.Distance(claw.transform.position, transform.position) < 0.02)
                {
                    //if claw is near body
                    clawState = ClawState.Ready;
                    state = prevState;
                    claw.SetActive(false);
                }
                break;
        }
    }
    void ExecuteClawState()
    {
        Vector3 vel;// no meaning, for reusing
        Vector3 clawDir;
        Quaternion r;
        switch (clawState)
        {
            //todo remove copypaste
            case ClawState.Shooting:
                clawDir = landingTarget - transform.position;
                r = Quaternion.LookRotation(clawDir, Vector3.up);
                claw.transform.rotation = r;

                vel = (landingTarget - transform.position).normalized * Vector3.Distance(landingTarget, transform.position) / (clawParams.flyTime * (1 / 60f));
                Debug.Log(vel + " aaaa");
                claw_xVel = vel.x;
                claw_yVel = vel.y;
                break;
            case ClawState.Grabbing:
                clawDir = claw.transform.position - transform.position;
                r = Quaternion.LookRotation(clawDir, Vector3.up);
                claw.transform.rotation = r;

                claw_xVel = 0;
                claw_yVel = 0;
                break;
            case ClawState.Return:
                clawDir = transform.position - claw.transform.position;
                r = Quaternion.LookRotation(clawDir, Vector3.up);
                claw.transform.rotation = r;

                vel = (claw.transform.position - transform.position).normalized * Vector3.Distance(claw.transform.position, transform.position) / (clawParams.returnTime * (1 / 60f));
                claw_xVel = vel.x;
                claw_yVel = vel.y;
                break;
            case ClawState.Ready:

                break;

        }
    }

    void ClawMoveAndSlide()
    {
        Vector3 moveDirection = new Vector3(claw_xVel, claw_yVel, 0f);
        claw.transform.position += moveDirection * Time.deltaTime;
    }

    // use to replace tthe messy copypaste in execuyte clawstate
    private void LookAt(Vector3 a, Vector3 b)
    {
        ;
    }
}
