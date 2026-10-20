using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

public class cameraFrame : MonoBehaviour
{
    [Range(0f, 0.3f)] public float XMaxRatio = 0.1f;
    [Range(0f, 0.3f)] public float YMaxRatio = 0.1f;

    public float ratioOnScreen = 0.15f;
    public float Xratio = 16f;
    public float Yratio = 9f;

    [SerializeField] CinemachineCamera DroidviewCamera;//drag the camera that is used for viewing here
    [SerializeField] CinemachineCamera BotviewCamera;//drag the shotting camera here
    //this script should not do any changes to the camera, only refer to its variables


    private RectTransform rectTransform;
    [SerializeField] private PlayerInput playerInput;
    private InputAction camToggleAction;
    private bool isOpenCam;
    private Vector2 screenSize;
    private CinemachineCamera viewCamera;


    void Awake()
    {
        viewCamera = BotviewCamera; // Set the viewCamera to botviewCamera by default
        isOpenCam = false;
        rectTransform = GetComponent<RectTransform>();
        camToggleAction = playerInput.actions.FindAction("Camera");
        screenSize = new Vector2(Screen.width, Screen.height);
        AdjustFrameSize();
        SetUiVisible(false);
    }

    public void cameraToggle(bool state)
    {
        isOpenCam = state;
        SetUiVisible(state);
    }

    void SetUiVisible(bool visible)
    {
        foreach (UnityEngine.UI.Graphic graphic in GetComponentsInChildren<UnityEngine.UI.Graphic>(true))
            graphic.enabled = visible;
    }

    private void AdjustFrameSize()
    {
        if (rectTransform == null || Xratio <= 0f || Yratio <= 0f)
        {
            return;
        }

        Vector2 availableSize = screenSize;
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null && canvas.rootCanvas.scaleFactor > 0f)
        {
            availableSize /= canvas.rootCanvas.scaleFactor;
        }

        float aspectRatio = Xratio / Yratio;
        float targetArea = availableSize.x * availableSize.y * Mathf.Clamp01(ratioOnScreen);
        float width = Mathf.Sqrt(targetArea * aspectRatio);
        float height = width / aspectRatio;
        float fitScale = Mathf.Min(1f, availableSize.x / width, availableSize.y / height);

        rectTransform.sizeDelta = new Vector2(width * fitScale, height * fitScale);
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 currentScreenSize = new Vector2(Screen.width, Screen.height);
        if (currentScreenSize != screenSize)
        {
            screenSize = currentScreenSize;
            AdjustFrameSize();
        }

        if (!isOpenCam || rectTransform == null)
        {
            return;
        }

        Vector2 mousePosition = Mouse.current != null
            ? Mouse.current.position.ReadValue()
            : screenSize * 0.5f;

        Vector2 normalizedMouse = (mousePosition / screenSize - Vector2.one * 0.5f) * 2f;
        normalizedMouse.x = Mathf.Clamp(normalizedMouse.x, -1f, 1f);
        normalizedMouse.y = Mathf.Clamp(normalizedMouse.y, -1f, 1f);

        float canvasScaleFactor = 1f;
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null && canvas.rootCanvas.scaleFactor > 0f)
        {
            canvasScaleFactor = canvas.rootCanvas.scaleFactor;
        }

        Vector2 maxOffset = new Vector2(
            screenSize.x * Mathf.Clamp01(XMaxRatio),
            screenSize.y * Mathf.Clamp01(YMaxRatio)) / canvasScaleFactor;
        rectTransform.anchoredPosition = Vector2.Scale(normalizedMouse, maxOffset);

        // Cinemachine reports the vertical FOV in degrees; derive the horizontal FOV for this aspect ratio.
        if (viewCamera == null)
        {
            print("viewCamera is not assigned.");
            return;
        }

        //obtain FOV in angle form for later calculation
        float verticalFov = viewCamera.Lens.FieldOfView;
        float horizontalFov = 2f * Mathf.Atan(
            Mathf.Tan(verticalFov * 0.5f * Mathf.Deg2Rad) * (Xratio / Yratio)) * Mathf.Rad2Deg;

        Vector3[] frameCorners = new Vector3[4];
        rectTransform.GetWorldCorners(frameCorners);
        Camera uiCamera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? canvas.worldCamera
            : null;

        float frameLeft = screenSize.x;
        float frameRight = 0f;
        float frameBottom = screenSize.y;
        float frameTop = 0f;
        foreach (Vector3 frameCorner in frameCorners)
        {
            Vector2 screenCorner = RectTransformUtility.WorldToScreenPoint(uiCamera, frameCorner);
            frameLeft = Mathf.Min(frameLeft, screenCorner.x);
            frameRight = Mathf.Max(frameRight, screenCorner.x);
            frameBottom = Mathf.Min(frameBottom, screenCorner.y);
            frameTop = Mathf.Max(frameTop, screenCorner.y);
        }

        float horizontalFovLimit = Mathf.Tan(horizontalFov * 0.5f * Mathf.Deg2Rad);
        float verticalFovLimit = Mathf.Tan(verticalFov * 0.5f * Mathf.Deg2Rad);
        float leftViewport = Mathf.Clamp(frameLeft / screenSize.x * 2f - 1f, -1f, 1f);
        float rightViewport = Mathf.Clamp(frameRight / screenSize.x * 2f - 1f, -1f, 1f);
        float bottomViewport = Mathf.Clamp(frameBottom / screenSize.y * 2f - 1f, -1f, 1f);
        float topViewport = Mathf.Clamp(frameTop / screenSize.y * 2f - 1f, -1f, 1f);

        float leftBoundary = Mathf.Atan(leftViewport * horizontalFovLimit);
        float rightBoundary = Mathf.Atan(rightViewport * horizontalFovLimit);
        float lowerBoundary = Mathf.Atan(bottomViewport * verticalFovLimit);
        float upperBoundary = Mathf.Atan(topViewport * verticalFovLimit);


        //loop through all of the evidences\
        Camera outputCamera = Camera.main;
        GameObject closestEvidence = null;
        float closestDistance = float.PositiveInfinity;

        EvidenceDataContainer[] evidenceObjects = FindObjectsByType<EvidenceDataContainer>(FindObjectsSortMode.None);
        foreach (EvidenceDataContainer evidence in evidenceObjects)
        {
            GameObject evidenceObject = evidence.gameObject;
            Vector3 evidenceLocalPosition = viewCamera.transform.InverseTransformPoint(evidenceObject.transform.position);
            if (evidenceLocalPosition.z > 0f
                && evidenceLocalPosition.x / evidenceLocalPosition.z >= Mathf.Tan(leftBoundary)
                && evidenceLocalPosition.x / evidenceLocalPosition.z <= Mathf.Tan(rightBoundary)
                && evidenceLocalPosition.y / evidenceLocalPosition.z >= Mathf.Tan(lowerBoundary)
                && evidenceLocalPosition.y / evidenceLocalPosition.z <= Mathf.Tan(upperBoundary))
            {
                //Color the sprite green if it's inside the scanning area, for demo use
                evidenceObject.GetComponent<SpriteRenderer>().color = Color.green;
                Vector2 frameCenter = RectTransformUtility.WorldToScreenPoint(
                    uiCamera,
                    rectTransform.TransformPoint(rectTransform.rect.center)
                );

                Vector3 screenPos = outputCamera.WorldToScreenPoint(evidenceObject.transform.position);
                float distanceToCursor = Vector2.Distance(screenPos, frameCenter);
                if (distanceToCursor < closestDistance)
                {
                    closestDistance = distanceToCursor;
                    closestEvidence = evidenceObject;
                }
            }
            else
            {
                evidenceObject.GetComponent<SpriteRenderer>().color = Color.red; // Change color to red if outside frame
            }
        }
        //Read the evidence data that are closetest to the camera area's center
        if (closestEvidence == null)
        {
            return;
        }
        scanned(closestEvidence);


    }

    void scanned(GameObject evidenceObject)
    {
        //implement waht you want to do when the evidence is scanned
        EvidenceData evidenceData = evidenceObject.GetComponent<EvidenceDataContainer>().evidenceData;
        evidenceObject.GetComponent<SpriteRenderer>().color = Color.yellow;
        print("Scanned evidence: " + evidenceData.objectName + " - " + evidenceData.description + " - " + evidenceData.systemResponse);
    }

    public void changeMode(bool isBot)
    {
        if (isBot)
        {
            viewCamera = BotviewCamera;
        }
        else
        {
            viewCamera = DroidviewCamera;
        }
    }


}
