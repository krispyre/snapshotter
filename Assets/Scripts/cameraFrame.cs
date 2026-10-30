using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class cameraFrame : MonoBehaviour
{
    [Range(0f, 0.3f)] public float XMaxRatio = 0.1f;
    [Range(0f, 0.3f)] public float YMaxRatio = 0.1f;

    public float ratioOnScreen = 0.15f;
    public float Xratio = 16f;
    public float Yratio = 9f;

    private TextMeshProUGUI EvidenceName;
    private TextMeshProUGUI EvidenceDescription;
    private TextMeshProUGUI Systemprompt;

    private GameReferences gameReferences;
    private GameEvent gameEvent;
    private RectTransform rectTransform;
    private Vector2 screenSize;

    private void Awake()
    {
        gameReferences = GameReferences.Instance;
        gameEvent = gameReferences.gameEvent;
        EvidenceName = gameReferences.EvidenceName;
        EvidenceDescription = gameReferences.EvidenceDescription;
        Systemprompt = gameReferences.Systemprompt;
        rectTransform = GetComponent<RectTransform>();
        screenSize = new Vector2(Screen.width, Screen.height);
        AdjustFrameSize();
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
    private void Update()
    {
        if (!gameEvent.CamMode || rectTransform == null)
        {
            return;
        }

        Vector2 currentScreenSize = new Vector2(Screen.width, Screen.height);
        if (currentScreenSize != screenSize)
        {
            screenSize = currentScreenSize;
            AdjustFrameSize();
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

        Camera outputCamera = Camera.main;
        if (outputCamera == null)
        {
            Debug.LogError("No MainCamera is available for camera-frame evidence detection.", this);
            return;
        }

        Vector3[] frameCorners = new Vector3[4];
        rectTransform.GetWorldCorners(frameCorners);
        Camera uiCamera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? canvas.worldCamera
            : null;

        float frameLeft = float.PositiveInfinity;
        float frameRight = float.NegativeInfinity;
        float frameBottom = float.PositiveInfinity;
        float frameTop = float.NegativeInfinity;
        foreach (Vector3 frameCorner in frameCorners)
        {
            Vector2 screenCorner = RectTransformUtility.WorldToScreenPoint(uiCamera, frameCorner);
            Vector3 viewportCorner = outputCamera.ScreenToViewportPoint(screenCorner);
            frameLeft = Mathf.Min(frameLeft, viewportCorner.x);
            frameRight = Mathf.Max(frameRight, viewportCorner.x);
            frameBottom = Mathf.Min(frameBottom, viewportCorner.y);
            frameTop = Mathf.Max(frameTop, viewportCorner.y);
        }

        // Loop through all evidence objects.
        GameObject closestEvidence = null;
        float closestDistance = float.PositiveInfinity;

        EvidenceDataContainer[] evidenceObjects = FindObjectsByType<EvidenceDataContainer>(FindObjectsSortMode.None);
        foreach (EvidenceDataContainer evidence in evidenceObjects)
        {
            GameObject evidenceObject = evidence.gameObject;
            Vector3 evidenceViewportPosition = outputCamera.WorldToViewportPoint(evidenceObject.transform.position);
            if (evidenceViewportPosition.z > 0f
                && evidenceViewportPosition.x >= frameLeft
                && evidenceViewportPosition.x <= frameRight
                && evidenceViewportPosition.y >= frameBottom
                && evidenceViewportPosition.y <= frameTop)
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
        // Read the evidence data closest to the camera area's center.
        if (closestEvidence == null)
        {
            EvidenceName.text = "";
            EvidenceDescription.text = "";
            Systemprompt.text = "";
            return;
        }
        LookAt(closestEvidence);

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Take(closestEvidence);
        }
    }

    private void LookAt(GameObject evidenceObject)
    {
        EvidenceData evidenceData = evidenceObject.GetComponent<EvidenceDataContainer>().evidenceData;
        if (evidenceData.state == 0)
        {
            gameEvent.SeeEvidence(evidenceData);
        }
        evidenceObject.GetComponent<SpriteRenderer>().color = Color.yellow;
        EvidenceName.text = evidenceData.objectName;

    }

    private void Take(GameObject evidenceObject)
    {
        EvidenceData evidenceData = evidenceObject.GetComponent<EvidenceDataContainer>().evidenceData;
        gameEvent.TakeEvidence(evidenceData);
        EvidenceName.text = evidenceData.objectName;
        EvidenceDescription.text = evidenceData.description;
        Systemprompt.text = evidenceData.systemResponse;
    }
}
