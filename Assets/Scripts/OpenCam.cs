using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

public class OpenCam : MonoBehaviour
{
    public GameEvent gameEvent;
    public CinemachineCamera DriodCam;
    public CinemachineCamera DriodRecordCam;
    public CinemachineCamera SpiderCam;
    public CinemachineCamera SpiderRecordCam;
    [SerializeField] private cameraFrame cameraFrame;
    [SerializeField] public GameObject EvidenceDataPage;
    [SerializeField] public GameObject frameMask;
    public float MaxZoomLens = 10f;
    public float zoomSpeed = 1f;

    public float maxTurningAngle = 10f;
    public float verticalRotateScale = 0.15f;
    public float horizontalRotateScale = 0.5f;

    private CinemachineRotationComposer DriodComposer;
    private CinemachineRotationComposer SpiderComposer;

    private Vector3 SpiderinitialTargetOffset;
    private Vector3 DriodinitialTargetOffset;
    private bool botMode;
    [SerializeField, ReadOnlyInspector] public bool isOpenCam;
    private float currentZoomLens = 0f;
    private float initialSpiderLens;


    [SerializeField] PlayerInput playerInput; // drag the player here
    private InputAction camToggleAction;

    void Start()
    {
        EvidenceDataPage.SetActive(isOpenCam);
        frameMask.SetActive(isOpenCam);
        DriodComposer = DriodRecordCam.GetComponent<CinemachineRotationComposer>();
        SpiderComposer = SpiderRecordCam.GetComponent<CinemachineRotationComposer>();
        DriodinitialTargetOffset = DriodComposer.TargetOffset;
        SpiderinitialTargetOffset = SpiderComposer.TargetOffset;
    }

    void OnEnable(){
         gameEvent.OnOpenCam.AddListener(HandleOpenCamera);
         gameEvent.OnShutCam.AddListener(HandleCloseCamera);
    }
    void OnDisable(){
         gameEvent.OnOpenCam.RemoveListener(HandleOpenCamera);
         gameEvent.OnShutCam.RemoveListener(HandleCloseCamera);
    }
    void Awake()
    {
        camToggleAction = playerInput.actions.FindAction("Camera");
        if (SpiderRecordCam != null)
        {
            initialSpiderLens = SpiderRecordCam.Lens.FieldOfView;
        }
    }

    void Update()
    {
        if (camToggleAction.WasPressedThisFrame())
        {
            gameEvent.ToggleCamera(!gameEvent.CamMode);// broadcast the signal of camera toggle

            cameraFrame.cameraToggle(gameEvent.CamMode);
            EvidenceDataPage.SetActive(gameEvent.CamMode);
            frameMask.SetActive(gameEvent.CamMode);
        }

        if (gameEvent.CamMode)
        {
            if (gameEvent.PlayerModeIsSpider)
            {
                Recording(SpiderComposer, SpiderRecordCam, SpiderinitialTargetOffset);
            }
            else
            {
                Recording(DriodComposer, DriodRecordCam, DriodinitialTargetOffset);
            }
        }
    }
    void Recording(CinemachineRotationComposer Composer,CinemachineCamera Camera, Vector3 initialTargetOffset)
    {
        if (Composer == null || Camera == null)
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

        if (Mouse.current != null && gameEvent.PlayerModeIsSpider)
        {
            float scrollDelta = Mouse.current.scroll.ReadValue().y;
            currentZoomLens = Mathf.Clamp(
                currentZoomLens + scrollDelta * zoomSpeed,
                -MaxZoomLens,
                MaxZoomLens);

            var lens = Camera.Lens;
            lens.FieldOfView = initialSpiderLens - currentZoomLens;
           Camera.Lens = lens;
        }

        Composer.TargetOffset = targetOffset;

    }

    void HandleOpenCamera()
    {
        Mouse.current?.WarpCursorPosition(new Vector2(Screen.width * 0.5f, Screen.height * 0.5f));
        ToggleCamera();
    }

    void HandleCloseCamera()
    {
        ToggleCamera();
    }
    //This function is going to be called by PlayerModeController.cs
    public void ChangeState(bool isSpiderMode)
    {
        gameEvent.Changemode(isSpiderMode);
        cameraFrame.cameraToggle(false);
        EvidenceDataPage.SetActive(gameEvent.CamMode);
        frameMask.SetActive(gameEvent.CamMode);

        if (gameEvent.PlayerModeIsSpider)
        {
            SpiderCam.Priority = 11;
            SpiderRecordCam.Priority = 10;
            DriodCam.Priority = 0;
            DriodRecordCam.Priority = 0;
        }
        else
        {
            SpiderCam.Priority = 0;
            SpiderRecordCam.Priority = 0;
            DriodCam.Priority = 11;
            DriodRecordCam.Priority = 10;
        }

    }

    void ToggleCamera()
    {
        currentZoomLens = 0f;
        if (SpiderRecordCam != null)
        {
            var lens = SpiderRecordCam.Lens;
            lens.FieldOfView = initialSpiderLens;
            SpiderRecordCam.Lens = lens;
        }

        if (gameEvent.PlayerModeIsSpider)
        {
            int tmp = SpiderCam.Priority;
            SpiderCam.Priority = SpiderRecordCam.Priority;
            SpiderRecordCam.Priority = tmp;
        }
        else
        {
            int tmp = DriodCam.Priority;
            DriodCam.Priority = DriodRecordCam.Priority;
            DriodRecordCam.Priority = tmp;
        }
    }


}
