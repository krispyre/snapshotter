using UnityEngine;
using UnityEngine.InputSystem;

public class cameraFrame : MonoBehaviour
{
    public float XMax = 198.0f;
    public float YMax = 108.0f;

    public float ratioOnScreen = 0.3f;
    public float Xratio = 16f;
    public float Yratio = 9f;

    private RectTransform rectTransform;
    [SerializeField] private PlayerInput playerInput;
    private InputAction camToggleAction;
    private bool isOpenCam;
    private Vector2 screenSize;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        camToggleAction = playerInput.actions.FindAction("CamToggle");
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

    // Update is called once per frame
    void Update()
    {
        if (camToggleAction.WasPressedThisFrame())
        {
             print(isOpenCam);
            isOpenCam = !isOpenCam;
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
        rectTransform.anchoredPosition = new Vector2(normalizedMouse.x * XMax, normalizedMouse.y * YMax);

     }
}
