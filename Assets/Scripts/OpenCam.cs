using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

public class OpenCam : MonoBehaviour
{
    public float MaxZoomLens = 10f;
    public float zoomSpeed = 1f;
    public float maxTurningAngle = 10f;
    public float verticalRotateScale = 0.15f;
    public float horizontalRotateScale = 0.5f;

    private GameReferences gameReferences;
    private GameEvent gameEvent;
    private CinemachineCamera droidCamera;
    private CinemachineCamera spiderCamera;
    private CinemachineCamera droidRecordCamera;
    private CinemachineCamera spiderRecordCamera;
    private CinemachineRotationComposer droidComposer;
    private CinemachineRotationComposer spiderComposer;
    private InputAction camToggleAction;
    private Vector3 spiderInitialTargetOffset;
    private Vector3 droidInitialTargetOffset;
    private float currentZoomLens;
    private float initialSpiderLens;

    private void Awake()
    {
        gameReferences = GameReferences.Instance;
        gameEvent = gameReferences.gameEvent;
        droidCamera = gameReferences.droidCamera;
        spiderCamera = gameReferences.spiderCamera;
        droidRecordCamera = gameReferences.droidRecordCamera;
        spiderRecordCamera = gameReferences.spiderRecordCamera;
        camToggleAction = gameReferences.playerInput.actions.FindAction("Camera");
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
        gameEvent.OnOpenCam.AddListener(HandleOpenCamera);
        gameEvent.OnShutCam.AddListener(HandleCloseCamera);
        gameEvent.SwitchToDroid.AddListener(HandlePlayerModeChanged);
        gameEvent.SwitchToSpider.AddListener(HandlePlayerModeChanged);
    }

    private void Start()
    {
        ApplyCameraPriorities();
        SetCameraUiVisible(gameEvent.CamMode);
    }

    private void OnDisable()
    {
        gameEvent.OnOpenCam.RemoveListener(HandleOpenCamera);
        gameEvent.OnShutCam.RemoveListener(HandleCloseCamera);
        gameEvent.SwitchToDroid.RemoveListener(HandlePlayerModeChanged);
        gameEvent.SwitchToSpider.RemoveListener(HandlePlayerModeChanged);
    }

    private void Update()
    {
        if (camToggleAction.WasPressedThisFrame())
        {
            gameEvent.ToggleCamera(!gameEvent.CamMode);
        }

        if (!gameEvent.CamMode)
        {
            return;
        }

        if (gameEvent.PlayerModeIsSpider)
        {
            Recording(spiderComposer, spiderRecordCamera, spiderInitialTargetOffset);
        }
        else
        {
            Recording(droidComposer, droidRecordCamera, droidInitialTargetOffset);
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

        if (Mouse.current != null && gameEvent.PlayerModeIsSpider)
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

    private void HandlePlayerModeChanged()
    {
        ApplyCameraPriorities();
        SetCameraUiVisible(false);
    }

    private void ApplyCameraPriorities()
    {
        SetPriorities(spiderCamera, spiderRecordCamera, gameEvent.PlayerModeIsSpider);
        SetPriorities(droidCamera, droidRecordCamera, !gameEvent.PlayerModeIsSpider);
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

        viewCamera.Priority = gameEvent.CamMode ? 10 : 11;
        recordCamera.Priority = gameEvent.CamMode ? 11 : 10;
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

        if (gameReferences.evidencePage != null)
        {
            gameReferences.evidencePage.SetActive(visible);
        }
        if (gameReferences.cameraFrame != null)
        {
            gameReferences.cameraFrame.SetActive(visible);
        }
    }
}
