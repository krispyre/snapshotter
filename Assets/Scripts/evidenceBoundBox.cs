using UnityEngine;
using UnityEngine.UI;

public class evidenceBoundBox : MonoBehaviour
{
    [SerializeField] private GameEvent gameEvent;
    [SerializeField] private GameObject frameMask;
    [Min(0f)]
    public float speed = 0.1f;

    private Camera worldCamera;
    private Camera uicamera;
    private RectTransform box;
    private Image boxImage;

    private GameObject evidenceObject;
    private RectTransform aimCentreData;
    private Canvas canvas;

    private void Awake()
    {
        box = GetComponent<RectTransform>();
        boxImage = GetComponent<Image>();
        aimCentreData = frameMask != null ? frameMask.GetComponent<RectTransform>() : null;
        canvas = GetComponentInParent<Canvas>();
        worldCamera = Camera.main;
        uicamera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? canvas.worldCamera
            : null;
    }

    private void OnEnable()
    {
        gameEvent.OnEvidenceSeen.AddListener(EvidenceBound);
        gameEvent.EmptyCamera.AddListener(HideBoundBox);
        gameEvent.OnShutCam.AddListener(HideBoundBox);
    }

    private void OnDisable()
    {
        gameEvent.OnEvidenceSeen.RemoveListener(EvidenceBound);
        gameEvent.EmptyCamera.RemoveListener(HideBoundBox);
        gameEvent.OnShutCam.RemoveListener(HideBoundBox);
    }

    private void Update()
    {
        if (evidenceObject != null && gameEvent.CamMode)
        {
            UpdateBoundBox();
        }
    }

    private void EvidenceBound(GameObject seenEvidence)
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
        if (boxImage != null)
        {
            boxImage.enabled = true;
        }

        UpdateBoundBox();
    }

    private void UpdateBoundBox()
    {
        if (evidenceObject == null || worldCamera == null || box == null || aimCentreData == null)
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
            Time.deltaTime * (distanceToCentre > 0f ? distanceToCentre * speed : 0f));
    }

    private void SetBoxToFrameCentre()
    {
        if (box == null || aimCentreData == null)
        {
            return;
        }

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

    private void HideBoundBox()
    {
        evidenceObject = null;
        if (boxImage != null)
        {
            boxImage.enabled = false;
        }
    }
}
