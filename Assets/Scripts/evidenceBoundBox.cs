using UnityEngine;
using UnityEngine.UI;

public class evidenceBoundBox : MonoBehaviour
{
    [SerializeField] private GameEvent gameEvent;

    private readonly Vector3[] worldCorners = new Vector3[8];
    private readonly Vector2[] screenCorners = new Vector2[4];
    private RectTransform imageRect;
    private RectTransform parentRect;
    private Canvas canvas;
    private Image boundImage;
    private GameObject evidenceObject;

    private void Awake()
    {
        imageRect = GetComponent<RectTransform>();
        parentRect = transform.parent as RectTransform;
        boundImage = GetComponent<Image>();
        canvas = GetComponentInParent<Canvas>();
        boundImage.enabled = false;
    }

    private void OnEnable()
    {
        gameEvent.OnEvidenceSeen.AddListener(EvidenceBound);
    }

    private void OnDisable()
    {
        if (gameEvent != null)
        {
            gameEvent.OnEvidenceSeen.RemoveListener(EvidenceBound);
        }

        evidenceObject = null;
        if (boundImage != null)
        {
            boundImage.enabled = false;
        }
    }

    private void Update()
    {
        if (evidenceObject != null)
        {
            UpdateBoundBox();
        }
    }

    private void EvidenceBound(GameObject seenEvidence)
    {
        evidenceObject = seenEvidence;
        UpdateBoundBox();
    }

    private void UpdateBoundBox()
    {
        if (evidenceObject == null)
        {
            HideBoundBox();
            return;
        }

        Renderer evidenceRenderer = evidenceObject.GetComponent<Renderer>();
        if (evidenceRenderer == null)
        {
            Debug.LogError("Evidence object requires a Renderer to display its screen bounds.", evidenceObject);
            evidenceObject = null;
            HideBoundBox();
            return;
        }
        Camera outputCamera = Camera.main;
        Camera uiCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : canvas.worldCamera;
        if (canvas.renderMode != RenderMode.ScreenSpaceOverlay && uiCamera == null)
        {
            Debug.LogError("The Canvas requires a camera to position the evidence bound box.", canvas);
            evidenceObject = null;
            HideBoundBox();
            return;
        }

        Bounds bounds = evidenceRenderer.bounds;
        Vector3 min = bounds.min;
        Vector3 max = bounds.max;
        int cornerIndex = 0;
        for (int x = 0; x <= 1; x++)
        {
            for (int y = 0; y <= 1; y++)
            {
                for (int z = 0; z <= 1; z++)
                {
                    worldCorners[cornerIndex++] = new Vector3(
                        x == 0 ? min.x : max.x,
                        y == 0 ? min.y : max.y,
                        z == 0 ? min.z : max.z);
                }
            }
        }

        float left = float.PositiveInfinity;
        float right = float.NegativeInfinity;
        float bottom = float.PositiveInfinity;
        float top = float.NegativeInfinity;
        foreach (Vector3 worldCorner in worldCorners)
        {
            Vector3 screenCorner = outputCamera.WorldToScreenPoint(worldCorner);
            if (screenCorner.z <= 0f)
            {
                continue;
            }

            left = Mathf.Min(left, screenCorner.x);
            right = Mathf.Max(right, screenCorner.x);
            bottom = Mathf.Min(bottom, screenCorner.y);
            top = Mathf.Max(top, screenCorner.y);
        }

        if (float.IsInfinity(left))
        {
            HideBoundBox();
            return;
        }

        left = Mathf.Clamp(left, 0f, Screen.width);
        right = Mathf.Clamp(right, 0f, Screen.width);
        bottom = Mathf.Clamp(bottom, 0f, Screen.height);
        top = Mathf.Clamp(top, 0f, Screen.height);
        if (left >= right || bottom >= top)
        {
            HideBoundBox();
            return;
        }

        screenCorners[0] = new Vector2(left, bottom);
        screenCorners[1] = new Vector2(left, top);
        screenCorners[2] = new Vector2(right, bottom);
        screenCorners[3] = new Vector2(right, top);

        float localLeft = float.PositiveInfinity;
        float localRight = float.NegativeInfinity;
        float localBottom = float.PositiveInfinity;
        float localTop = float.NegativeInfinity;
        foreach (Vector2 screenCorner in screenCorners)
        {
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    parentRect,
                    screenCorner,
                    uiCamera,
                    out Vector2 localCorner))
            {
                HideBoundBox();
                return;
            }

            localLeft = Mathf.Min(localLeft, localCorner.x);
            localRight = Mathf.Max(localRight, localCorner.x);
            localBottom = Mathf.Min(localBottom, localCorner.y);
            localTop = Mathf.Max(localTop, localCorner.y);
        }

        Vector2 localSize = new Vector2(localRight - localLeft, localTop - localBottom);
        Vector2 localCenter = new Vector2(
            (localLeft + localRight) * 0.5f,
            (localBottom + localTop) * 0.5f);
        Vector2 anchorReference = parentRect.rect.min + Vector2.Scale(
            parentRect.rect.size,
            imageRect.anchorMin + Vector2.Scale(
                imageRect.anchorMax - imageRect.anchorMin,
                imageRect.pivot));

        imageRect.anchoredPosition = localCenter - anchorReference;
        imageRect.sizeDelta = localSize - Vector2.Scale(
            parentRect.rect.size,
            imageRect.anchorMax - imageRect.anchorMin);
        boundImage.enabled = true;
    }

    private void HideBoundBox()
    {
        if (boundImage != null)
        {
            boundImage.enabled = false;
        }
    }
}
