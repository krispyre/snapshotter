using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

public class OpenCam : MonoBehaviour
{
    public UnityEvent OnOpenCam = new UnityEvent();
    public UnityEvent OnCloseCam = new UnityEvent();

    public float MaxZoomLens = 10f;
    public float zoomSpeed = 1f;
    public float maxTurningAngle = 10f;
    public float verticalRotateScale = 0.15f;
    public float horizontalRotateScale = 0.5f;

    [SerializeField] private GameStates gameStates;
    [SerializeField] private CinemachineCamera droidCamera;
    [SerializeField] private CinemachineCamera spiderCamera;
    [SerializeField] private CinemachineCamera droidRecordCamera;
    [SerializeField] private CinemachineCamera spiderRecordCamera;
    [SerializeField] private GameObject frameMask;
    [SerializeField] private GameObject evidencePage;
    [SerializeField] private PlayerInput playerInput;

    private PlayerModeController playerMode;
    private CinemachineRotationComposer droidComposer;
    private CinemachineRotationComposer spiderComposer;
    private InputAction camToggleAction;
    private Vector3 spiderInitialTargetOffset;
    private Vector3 droidInitialTargetOffset;
    private float currentZoomLens;
    private float initialSpiderLens;

    private void Awake()
    {
        playerMode = GetComponent<PlayerModeController>();
        if (playerInput == null)
            playerInput = GetComponent<PlayerInput>();

        if (gameStates == null || playerInput == null || playerInput.actions == null)
        {
            Debug.LogError("OpenCam requires a GameStates asset and a PlayerInput.", this);
            enabled = false;
            return;
        }

        camToggleAction = playerInput.actions.FindAction("Camera");
        if (camToggleAction == null)
        {
            Debug.LogError("The PlayerInput action asset does not contain a 'Camera' action.", this);
            enabled = false;
            return;
        }
        droidComposer = droidRecordCamera != null
            ? droidRecordCamera.GetComponent<CinemachineRotationComposer>()
            : null;
        spiderComposer = spiderRecordCamera != null
            ? spiderRecordCamera.GetComponent<CinemachineRotationComposer>()
            : null;
        droidInitialTargetOffset = droidComposer.TargetOffset;
        spiderInitialTargetOffset = spiderComposer.TargetOffset;
        initialSpiderLens = spiderRecordCamera.Lens.FieldOfView;
    }

    private void OnEnable()
    {
        if (playerMode == null)
            return;

        playerMode.switchMode.AddListener(HandlePlayerModeChanged);
    }

    private void Start()
    {
        if (gameStates.CamMode)
            HandleOpenCamera();
        else
            HandleCloseCamera();
    }

    private void OnDisable()
    {
        if (playerMode == null)
            return;

        playerMode.switchMode.RemoveListener(HandlePlayerModeChanged);
    }

    private void Update()
    {
        if (camToggleAction.WasPressedThisFrame())
        {
            SetCameraOpen(!gameStates.CamMode);
        }

        if (!gameStates.CamMode)
        {
            return;
        }

        if (gameStates.IsDroid)
        {
            Recording(droidComposer, droidRecordCamera, droidInitialTargetOffset);
        }
        else
        {
            Recording(spiderComposer, spiderRecordCamera, spiderInitialTargetOffset);
        }
    }

    private void Recording(
        CinemachineRotationComposer composer,
        CinemachineCamera camera,
        Vector3 initialTargetOffset)
    {
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
        targetOffset.x += mouseOffset.x * maxTurningAngle * horizontalRotateScale;
        targetOffset.y += mouseOffset.y * maxTurningAngle * verticalRotateScale;

        if (Mouse.current != null && !gameStates.IsDroid)
        {
            float scrollDelta = Mouse.current.scroll.ReadValue().y;
            currentZoomLens = Mathf.Clamp(
                currentZoomLens + scrollDelta * zoomSpeed,
                -MaxZoomLens,
                MaxZoomLens);

            var lens = camera.Lens;
            lens.FieldOfView = initialSpiderLens - currentZoomLens;
            camera.Lens = lens;
        }

        composer.TargetOffset = targetOffset;
    }

    public void SetCameraOpen(bool open)
    {
        if (!gameStates.SetCamMode(open))
            return;

        if (open)
        {
            HandleOpenCamera();
            OnOpenCam.Invoke();
        }
        else
        {
            HandleCloseCamera();
            OnCloseCam.Invoke();
        }
    }

    private void HandleOpenCamera()
    {
        Mouse.current?.WarpCursorPosition(new Vector2(Screen.width * 0.5f, Screen.height * 0.5f));
        ResetZoom();
        ApplyCameraPriorities();
        SetCameraUiVisible(true);
    }

    private void HandleCloseCamera()
    {
        ResetZoom();
        ApplyCameraPriorities();
        SetCameraUiVisible(false);
    }

    private void HandlePlayerModeChanged(bool isDroid)
    {
        if (gameStates.CamMode)
            SetCameraOpen(false);

        ApplyCameraPriorities();
        SetCameraUiVisible(false);
    }

    private void ApplyCameraPriorities()
    {
        SetPriorities(droidCamera, droidRecordCamera, gameStates.IsDroid);
        SetPriorities(spiderCamera, spiderRecordCamera, !gameStates.IsDroid);
    }

    private void SetPriorities(
        CinemachineCamera viewCamera,
        CinemachineCamera recordCamera,
        bool isActiveMode)
    {

        if (!isActiveMode)
        {
            viewCamera.Priority = 0;
            recordCamera.Priority = 0;
            return;
        }

        viewCamera.Priority = gameStates.CamMode ? 10 : 11;
        recordCamera.Priority = gameStates.CamMode ? 11 : 10;
    }

    private void ResetZoom()
    {
        currentZoomLens = 0f;
        if (spiderRecordCamera == null)
        {
            return;
        }

        var lens = spiderRecordCamera.Lens;
        lens.FieldOfView = initialSpiderLens;
        spiderRecordCamera.Lens = lens;
    }

    private void SetCameraUiVisible(bool visible)
    {

        if (evidencePage != null)
        {
            evidencePage.SetActive(visible);
        }
        if (frameMask != null)
        {
            frameMask.SetActive(visible);
        }
    }
}
