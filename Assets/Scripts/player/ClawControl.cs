using System;
using UnityEngine;
using UnityEngine.InputSystem;

public partial class PlayerMovement : MonoBehaviour
{
    [SerializeField] private ClawParams data;//idfk how to nameit
    [SerializeField] Transform clawPointer; // object for claw object to reference
    [SerializeField] GameObject claw; // object for claw object to reference
    [SerializeField, ReadOnlyInspector] bool engaged = false;
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
                engaged = !engaged;
                Vector3 aimDirection = mouseWorldPos - transform.position;
                Ray clawRay = new Ray(transform.position, aimDirection);

                if (Physics.Raycast(clawRay, out RaycastHit hitInfo, data.armLength))
                {
                    Vector3 landingTarget = hitInfo.point;
                    claw.transform.position = landingTarget;
                }
                else
                {
                    Debug.LogAssertion("claw miss");
                }
            }
        }

    }
}

