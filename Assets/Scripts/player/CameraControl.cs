using UnityEngine;
using UnityEngine.InputSystem;

public class CameraControl : MonoBehaviour
{
    private static readonly int zoomState = Animator.StringToHash("Zoom");
    private static readonly int unzoomState = Animator.StringToHash("Unzoom");

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private Animator perspectiveChange;
    public float maxTurningAngle = 50;
    public Vector3 position1 = new Vector3(-1.2f,5.3f,51.5f);

    public float FOV1 = 5.73f;

    public Vector3 position2 = new Vector3(-1.2f,4.1f,-15f);
    public float FOV2 = 25f;

    private Camera cameraComponent;
    private Quaternion initialLocalRotation;

    void Start()
    {
        perspectiveChange = GetComponent<Animator>();
        cameraComponent = GetComponent<Camera>();
        initialLocalRotation = transform.localRotation;
    }

    // Update is called once per frame
    void Update()
    {
        if(perspectiveChange != null)
        {
            AnimatorStateInfo state = perspectiveChange.GetCurrentAnimatorStateInfo(0);
            //Press e to get in camera mode, triggering the animation
           if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame && state.shortNameHash == unzoomState)
            {
                perspectiveChange.SetTrigger("ZoomTrigger");     
            }
            else if(Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame && state.shortNameHash == zoomState)
            {
                perspectiveChange.SetTrigger("UnzoomTrigger");
            }

        }
    }

    void LateUpdate()
    {
        if (perspectiveChange == null || cameraComponent == null)
        {
            return;
        }
        AnimatorStateInfo state = perspectiveChange.GetCurrentAnimatorStateInfo(0);

        if (state.shortNameHash == zoomState)
        {
            transform.position = position2;
            cameraComponent.fieldOfView = FOV2;

            Vector2 screenSize = new Vector2(Screen.width, Screen.height);
            Vector2 mousePosition = Mouse.current != null
                ? Mouse.current.position.ReadValue()
                : screenSize * 0.5f;
            Vector2 mouseOffset = (mousePosition / screenSize - Vector2.one * 0.5f) * 2f;

            float yRotateAngle = mouseOffset.x * maxTurningAngle;
            float xRotateAngle = -mouseOffset.y * maxTurningAngle/2f;

            transform.localRotation = initialLocalRotation
                * Quaternion.Euler(xRotateAngle, yRotateAngle, 0f);
        }
        else if (state.shortNameHash == unzoomState)
        {
            transform.position = position1;
            cameraComponent.fieldOfView = FOV1;
            transform.localRotation = initialLocalRotation;
        }
    }
}
