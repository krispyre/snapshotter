using UnityEngine;
using UnityEngine.InputSystem;

public class CameraControl : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public bool openCamera = false;
    // The distance traveled by the camera when the key is pressed
    public Vector3 cameraPos = new Vector3(0f, -2f, 45f);
    public Vector3 originalPos = new Vector3(0f,0f,0f);
    public float FOVparal = (6.7f);
    public float FOVpersp = (57);
    public float maxTurningAngle = 30;
    public float maxFollowRange = 0.4f;
    public float followSpeed = 3;

    [Min(0)] public int toggleCooldownFrames = 60;

    private Vector3 currentTranslate = Vector3.zero;
    private float currentFOV;
    private int cooldownFramesRemaining;
    private Camera cameraComponent;
    private Vector3 initialLocalPosition;
    private Quaternion initialLocalRotation;
    private float currentFollowDistance = 0f;

    void Start()
    {
        cameraComponent = GetComponent<Camera>();
        currentFOV = FOVparal;
        initialLocalPosition = transform.localPosition;
        initialLocalRotation = transform.localRotation;
    }

    // Update is called once per frame
    void Update()
    {
        if (cooldownFramesRemaining > 0)
        {
            cooldownFramesRemaining--;
            return;
        }

        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            openCamera = !openCamera;
            currentTranslate = openCamera ? cameraPos : originalPos;
            currentFOV = openCamera ? FOVpersp : FOVparal;
            cooldownFramesRemaining = toggleCooldownFrames;

            if (openCamera == true && Mouse.current != null)
            {
                Mouse.current.WarpCursorPosition(
                    new Vector2(Screen.width * 0.5f, Screen.height * 0.5f));
            }
        }

        if (Keyboard.current != null)
        {
            float horizontalInput = 0f;

            if (Keyboard.current.dKey.isPressed)
            {
                horizontalInput += 1f;
            }
            if (Keyboard.current.aKey.isPressed)
            {
                horizontalInput -= 1f;
            }

            currentFollowDistance = Mathf.Clamp(
                currentFollowDistance + horizontalInput * followSpeed * Time.deltaTime,
                -maxFollowRange,
                maxFollowRange);
        }
    }

// Using late update to ensure that player control 
    void LateUpdate()
    {
        // Change the camera's FOV and position according to the camera mode
        transform.localPosition = initialLocalPosition + currentTranslate + new Vector3(currentFollowDistance,0f,0f);
        cameraComponent.fieldOfView = currentFOV;

        //Record the mouse position on the game screen, rotate the camera accoring to the mouse position
        if(openCamera == true){
            Vector2 screenSize = new Vector2(Screen.width, Screen.height);
            Vector2 mousePosition = Mouse.current != null
                ? Mouse.current.position.ReadValue()
                : screenSize * 0.5f;
            Vector2 mouseOffset = (mousePosition / screenSize - Vector2.one * 0.5f) * 2f;

            float yRotateAngle = mouseOffset.x * maxTurningAngle;
            float xRotateAngle = -mouseOffset.y * maxTurningAngle/4f;

            transform.localRotation = initialLocalRotation
            * Quaternion.Euler(xRotateAngle, yRotateAngle, 0f);
        }
        else{
            transform.localRotation = initialLocalRotation;
        }

    }
}
