using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class showController : MonoBehaviour
{
    private CharacterController controller;

    void OnDrawGizmos()
    {
        if (controller == null) controller = GetComponent<CharacterController>();
        if (controller == null) return;

        // Set the wireframe color
        Gizmos.color = Color.green;

        // Calculate the physical position of the capsule center in the world
        Vector3 globalCenter = transform.TransformPoint(controller.center);

        // Draw a simple wireframe matrix matching the capsule shape
        Gizmos.matrix = Matrix4x4.TRS(globalCenter, transform.rotation, transform.lossyScale);
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(controller.radius * 2, controller.height, controller.radius * 2));
    }
}