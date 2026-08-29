using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

public class OpenCam : MonoBehaviour
{
    public float maxTurningAngle = 30f;
    public float verticalRotateScale = 0.15f;
    public float horizontalRotateScale = 0.5f;
    public int priorityIncrement = 2;

    private CinemachineRotationComposer composer;
    private CinemachineCamera camera;
    private Vector3 initialTargetOffset;
    private int initialPriority;

    void Start()
    {
        composer = GetComponent<CinemachineRotationComposer>();
        camera = GetComponent<CinemachineCamera>();
        initialPriority = camera.Priority;

        if (composer != null)
        {
            initialTargetOffset = composer.TargetOffset;
        }
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {

            // When e key is pressed, increase the camera's priority to get control
            if (camera != null)
            {
                if (camera.Priority == initialPriority)
                {
                    camera.Priority += priorityIncrement;
                }
                else if (camera.Priority == initialPriority + priorityIncrement)
                {
                    camera.Priority -= priorityIncrement;
                }
            }

            //When shotting camera takes control, place the mouse at the center of the screen
            if (Mouse.current != null && camera.Priority == initialPriority + priorityIncrement)
            {
                Mouse.current.WarpCursorPosition(new Vector2(Screen.width * 0.5f, Screen.height * 0.5f));
            }
        }

        if (composer == null || camera == null)
        {
            return;
        }

        Vector2 screenSize = new Vector2(Screen.width, Screen.height);
        Vector2 mousePosition = Mouse.current != null
            ? Mouse.current.position.ReadValue()
            : screenSize * 0.5f;

        Vector2 mouseOffset = (mousePosition / screenSize - Vector2.one * 0.5f) * 2f;

        Vector3 targetOffset = initialTargetOffset;
        targetOffset.x = initialTargetOffset.x + mouseOffset.x * maxTurningAngle * horizontalRotateScale;
        targetOffset.y = initialTargetOffset.y + mouseOffset.y * maxTurningAngle * verticalRotateScale;

        composer.TargetOffset = targetOffset;
    }
}
