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
    [SerializeField] ClawLandingTarget landingTarget;
    [SerializeField, ReadOnlyInspector] bool engaged = false;

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
                Shoot(mouseWorldPos);
            }
        }
    }

    private void Shoot(Vector3 mousePos)
    {
        engaged = !engaged;
        Vector3 aimDirection = mousePos - transform.position;
        Ray clawRay = new Ray(transform.position, aimDirection);

        if (Physics.Raycast(clawRay, out RaycastHit hitInfo, clawParams.armLength))
        {
            landingTarget.position = hitInfo.point;
            landingTarget.hit = true;
            claw.transform.position = (Vector3)landingTarget.position;
        }
        else
        {
            Debug.LogAssertion("claw miss");
            landingTarget.hit = false;
        }
    }
}

[Serializable]
public struct ClawLandingTarget
{
    public Vector3 position;
    public bool hit;
}

