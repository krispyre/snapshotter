using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;


public class evidenceBoundBox : MonoBehaviour
{
    [SerializeField] private GameStates gameStates;
    [SerializeField] private GameObject frameMask;
    [Min(0f)]
    public float speed = 1.6f;
    [Min(0f)]
    public float sizingSpeed = 0.7f;

    private Camera worldCamera;
    private Camera uicamera;
    private RectTransform box;
    private Image boxImage;

    private GameObject evidenceObject;
    private SpriteRenderer evidenceRenderer;
    private RectTransform aimCentreData;
    private Canvas canvas;
    private Vector2 defaultSize;

    private void Awake()
    {
        box = GetComponent<RectTransform>();
        defaultSize = box.sizeDelta;
        boxImage = GetComponent<Image>();
        aimCentreData = frameMask != null ? frameMask.GetComponent<RectTransform>() : null;
        canvas = GetComponentInParent<Canvas>();
        worldCamera = Camera.main;
        uicamera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? canvas.worldCamera
            : null;
    }

    // private void OnEnable()
    // {
    //     gameEvent.OnEvidenceSeen.AddListener(EvidenceBound);
    //     gameEvent.EmptyCamera.AddListener(HideBoundBox);
    //     gameEvent.OnShutCam.AddListener(HideBoundBox);
    // }
    private void Update()
    {
        if (evidenceObject != null && gameStates.CamMode)
        {
            UpdateBoundBox();
        }
    }

    public void EvidenceBound(GameObject seenEvidence)
    {
        
        if (seenEvidence == null)
        {
            HideBoundBox();
            return;
        }

        if (evidenceObject != seenEvidence)
        {
            SetBoxToFrameCentre();
        }

        evidenceObject = seenEvidence;
        evidenceRenderer = seenEvidence.GetComponent<SpriteRenderer>();
        if (evidenceRenderer == null)
        {
            Debug.LogError("Evidence object is missing a SpriteRenderer.", seenEvidence);
            HideBoundBox();
            return;
        }

        if (boxImage != null)
        {
            boxImage.enabled = true;
        }

        UpdateBoundBox();
    }

    private void UpdateBoundBox()
    {
        if (evidenceObject == null || evidenceRenderer == null
            || worldCamera == null || box == null || aimCentreData == null)
        {
            return;
        }

        RectTransform boxParent = box.parent as RectTransform;
        if (boxParent == null)
        {
            Debug.LogError("The evidence bound box must have a RectTransform parent.", this);
            return;
        }

        Vector3 evidenceScreenPosition =
            worldCamera.WorldToScreenPoint(evidenceObject.transform.position);
        if (evidenceScreenPosition.z <= 0f
            || !TryGetAnchoredPosition(boxParent, evidenceScreenPosition, out Vector2 evidenceUIPosition))
        {
            HideBoundBox();
            return;
        }

        float distanceToCentre = Vector2.Distance(evidenceUIPosition, box.anchoredPosition);

        box.anchoredPosition = Vector2.MoveTowards(
            box.anchoredPosition,
            evidenceUIPosition,
            Time.deltaTime * Mathf.Max(0f, distanceToCentre * speed));

        if (TryGetEvidenceUISize(boxParent, out Vector2 targetSize))
        {
            float sizeLerp = 1f - Mathf.Exp(-Mathf.Max(0f, sizingSpeed) * Time.deltaTime);
            Vector2 smoothedSize = Vector2.Lerp(box.rect.size, targetSize, sizeLerp);
            box.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, smoothedSize.x);
            box.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, smoothedSize.y);
        }
    }

    private void SetBoxToFrameCentre()
    {
        if (box == null || aimCentreData == null)
        {
            return;
        }

        box.sizeDelta = defaultSize;
        RectTransform boxParent = box.parent as RectTransform;
        if (boxParent == null)
        {
            Debug.LogError("The evidence bound box must have a RectTransform parent.", this);
            return;
        }

        Vector3 centreWorldPosition = aimCentreData.TransformPoint(aimCentreData.rect.center);
        Vector2 centreScreenPosition = RectTransformUtility.WorldToScreenPoint(uicamera, centreWorldPosition);
        if (TryGetAnchoredPosition(boxParent, centreScreenPosition, out Vector2 centreUIPosition))
        {
            box.anchoredPosition = centreUIPosition;
        }
    }

    private bool TryGetEvidenceUISize(RectTransform parent, out Vector2 size)
    {
        size = Vector2.zero;
        Bounds bounds = evidenceRenderer.bounds;
        Vector3 min = bounds.min;
        Vector3 max = bounds.max;
        Vector2 screenMin = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
        Vector2 screenMax = new Vector2(float.NegativeInfinity, float.NegativeInfinity);

        for (int corner = 0; corner < 8; corner++)
        {
            Vector3 worldCorner = new Vector3(
                (corner & 1) == 0 ? min.x : max.x,
                (corner & 2) == 0 ? min.y : max.y,
                (corner & 4) == 0 ? min.z : max.z);
            Vector3 screenCorner = worldCamera.WorldToScreenPoint(worldCorner);
            if (screenCorner.z <= 0f
                || !TryGetAnchoredPosition(parent, screenCorner, out Vector2 uiCorner))
            {
                return false;
            }

            screenMin = Vector2.Min(screenMin, uiCorner);
            screenMax = Vector2.Max(screenMax, uiCorner);
        }

        size = screenMax - screenMin;
        return size.x > 0f && size.y > 0f;
    }

    private bool TryGetAnchoredPosition(
        RectTransform parent,
        Vector2 screenPosition,
        out Vector2 anchoredPosition)
    {
        anchoredPosition = Vector2.zero;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parent,
            screenPosition,
            uicamera,
            out Vector2 localPosition))
        {
            return false;
        }

        Vector2 anchor = Vector2.Scale(box.anchorMin, Vector2.one - box.pivot)
            + Vector2.Scale(box.anchorMax, box.pivot);
        Vector2 anchorReference = parent.rect.min + Vector2.Scale(parent.rect.size, anchor);
        anchoredPosition = localPosition - anchorReference;
        return true;
    }

    public void HideBoundBox()
    {
        evidenceObject = null;
        evidenceRenderer = null;
        if (boxImage != null)
        {
            boxImage.enabled = false;
        }
    }
}
