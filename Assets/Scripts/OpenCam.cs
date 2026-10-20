using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

public class OpenCam : MonoBehaviour
{
    public CinemachineCamera driodCam;
    public CinemachineCamera driodRecordCam;
    public CinemachineCamera botCam;
    public CinemachineCamera botRecordCam;
    public float MaxZoomLens = 10f;
    public float zoomSpeed = 1f;

    public float maxTurningAngle = 10f;
    public float verticalRotateScale = 0.15f;
    public float horizontalRotateScale = 0.5f;
    [SerializeField] private cameraFrame cameraFrame;

    private CinemachineRotationComposer driodComposer;
    private CinemachineRotationComposer botComposer;

    private Vector3 BotinitialTargetOffset;
    private Vector3 DriodinitialTargetOffset;
    private bool botMode;
    [SerializeField] bool isOpenCam;
    private float currentZoomLens = 0f;
    private float initialBotLens;


    [SerializeField] PlayerInput playerInput; // drag the player here
    private InputAction camToggleAction;

    void Start()
    {
        driodComposer = driodRecordCam.GetComponent<CinemachineRotationComposer>();
        botComposer = botRecordCam.GetComponent<CinemachineRotationComposer>();
        DriodinitialTargetOffset = driodComposer.TargetOffset;
        BotinitialTargetOffset = botComposer.TargetOffset;
        botMode = true;
    }
    void Awake()
    {
        camToggleAction = playerInput.actions.FindAction("Camera");
        if (botRecordCam != null)
        {
            initialBotLens = botRecordCam.Lens.FieldOfView;
        }
    }

    void toggleCamera()
    {
        currentZoomLens = 0f;
        if (botRecordCam != null)
        {
            var lens = botRecordCam.Lens;
            lens.FieldOfView = initialBotLens;
            botRecordCam.Lens = lens;
        }

        if (botMode)
        {
            int tmp = botCam.Priority;
            botCam.Priority = botRecordCam.Priority;
            botRecordCam.Priority = tmp;
        }
        else
        {
            int tmp = driodCam.Priority;
            driodCam.Priority = driodRecordCam.Priority;
            driodRecordCam.Priority = tmp;
        }
    }

    void Update()
    {
        if (camToggleAction.WasPressedThisFrame())
        {
            isOpenCam = !isOpenCam;
            cameraFrame.cameraToggle(isOpenCam);

            // When [camera] key is pressed, call the toggleCamera function
            if (botRecordCam != null && driodRecordCam != null)
            {
                toggleCamera();
            }

            //When shotting camera takes control, place the mouse at the center of the screen
            Mouse.current?.WarpCursorPosition(new Vector2(Screen.width * 0.5f, Screen.height * 0.5f));
        }

        if (isOpenCam)
        {
            if (botMode)
            {
                BotRecording();
            }
            else
            {
                DroidRecording();
            }
        }
    }
    void BotRecording()
    {
        if (botComposer == null || botRecordCam == null)
        {
            return;
        }

        Vector2 screenSize = new Vector2(Screen.width, Screen.height);
        Vector2 mousePosition = Mouse.current != null
            ? Mouse.current.position.ReadValue()
            : screenSize * 0.5f;

        Vector2 mouseOffset = (mousePosition / screenSize - Vector2.one * 0.5f) * 2f;

        Vector3 targetOffset = BotinitialTargetOffset;
        targetOffset.x = BotinitialTargetOffset.x + mouseOffset.x * maxTurningAngle * horizontalRotateScale;
        targetOffset.y = BotinitialTargetOffset.y + mouseOffset.y * maxTurningAngle * verticalRotateScale;

        if (Mouse.current != null)
        {
            float scrollDelta = Mouse.current.scroll.ReadValue().y;
            currentZoomLens = Mathf.Clamp(
                currentZoomLens + scrollDelta * zoomSpeed,
                -MaxZoomLens,
                MaxZoomLens);

            var lens = botRecordCam.Lens;
            lens.FieldOfView = initialBotLens - currentZoomLens;
            botRecordCam.Lens = lens;
        }

        botComposer.TargetOffset = targetOffset;

    }
    void DroidRecording()
    {
        if (driodComposer == null || driodRecordCam == null)
        {
            return;
        }

        Vector2 screenSize = new Vector2(Screen.width, Screen.height);
        Vector2 mousePosition = Mouse.current != null
            ? Mouse.current.position.ReadValue()
            : screenSize * 0.5f;

        Vector2 mouseOffset = (mousePosition / screenSize - Vector2.one * 0.5f) * 2f;

        Vector3 targetOffset = DriodinitialTargetOffset;
        targetOffset.x = DriodinitialTargetOffset.x + mouseOffset.x * maxTurningAngle * horizontalRotateScale;
        targetOffset.y = DriodinitialTargetOffset.y + mouseOffset.y * maxTurningAngle * verticalRotateScale;

        driodComposer.TargetOffset = targetOffset;

    }
    //This function is going to be called by PlayerModeController.cs
    public void changeState(bool isBotMode)
    {
        botMode = isBotMode;
        isOpenCam = false;
        cameraFrame.cameraToggle(false);

        if (botMode)
        {
            botCam.Priority = 11;
            botRecordCam.Priority = 10;
            driodCam.Priority = 0;
            driodRecordCam.Priority = 0;
        }
        else
        {
            botCam.Priority = 0;
            botRecordCam.Priority = 0;
            driodCam.Priority = 11;
            driodRecordCam.Priority = 10;
        }

    }
}
