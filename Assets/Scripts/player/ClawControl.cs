using UnityEngine;
using UnityEngine.InputSystem;

public partial class PlayerMovement : MonoBehaviour
{

    [SerializeField] GameObject claw;
    void UpdateMousePos()
    {
        //FACT CHECK THIS!!!!!!

        if (Mouse.current == null) return;

        // Read screen position
        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();

        // 1. Create plane at character depth facing camera (+Z / -Z)
        Plane plane = new Plane(Vector3.back, new Vector3(0, 0, transform.position.z));

        // 2. Convert mouse screen point to 3D ray
        Ray ray = mainCamera.ScreenPointToRay(mouseScreenPos);

        // 3. Calculate ray intersection with plane
        if (plane.Raycast(ray, out float enterDistance))
        {
            Vector3 mouseWorldPos = ray.GetPoint(enterDistance);

            // Aim vector from shoulder height, not feet
            Vector3 aimDirection = mouseWorldPos - transform.position;

            claw.transform.position = mouseWorldPos;
        }
    }
}

