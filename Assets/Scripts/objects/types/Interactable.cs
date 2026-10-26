using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class Interactable : MonoBehaviour
{
    [SerializeField, ReadOnlyInspector] protected bool engaged;
    public string prompt;
    [SerializeField] protected TextMeshProUGUI promptText;
    [SerializeField] private float promptYOffset = 10f;
    protected Collider trigger;
    protected Collider engagedPlayer;
    private MeshRenderer meshRenderer;
    private Camera mainCamera;

    private PlayerInput playerInput;
    private InputAction interactAction;
    private InputAction dirXAction;
    private InputAction dirYAction;

    public bool Engaged => engaged;
    public event System.Action<Collider> PlayerEntered;
    public event System.Action<Collider> PlayerExited;

    // GetComponent<MeshRenderer>().material = Material1;
    protected void Awake()
    {
        trigger = GetComponent<Collider>();
        meshRenderer = GetComponent<MeshRenderer>();
        OnAwake();
    }
    protected void Start()
    {
        playerInput = GameObject.Find("playerGroup").GetComponent<PlayerInput>();
        interactAction = playerInput.actions.FindAction("Interact");
        dirXAction = playerInput.actions.FindAction("DirX");
        dirYAction = playerInput.actions.FindAction("DirY");
        mainCamera = Camera.main;

        if (promptText != null)
        {
            promptText.text = prompt;
            promptText.gameObject.SetActive(false);
        }
        OnStart();
    }

    public bool PressedLeft => PressedDirection(dirXAction, false);
    public bool PressedRight => PressedDirection(dirXAction, true);
    public bool PressedDown => PressedDirection(dirYAction, false);
    public bool PressedUp => PressedDirection(dirYAction, true);

    static bool PressedDirection(InputAction action, bool positive)
    {
        if (action == null || !action.WasPressedThisFrame()) return false;
        float value = action.ReadValue<float>();
        return positive ? value > 0f : value < 0f;
    }

    protected void Update()
    {
        if (interactAction.WasPressedThisFrame() && engaged)
        {
            OnInteract(engagedPlayer);
        }

        if (engaged && promptText != null && trigger != null && mainCamera != null)
        {
            Rect screenRect = trigger.GetScreenRect(mainCamera);
            promptText.rectTransform.position = new Vector3(
                screenRect.center.x,
                screenRect.yMax + promptYOffset,
                0f);
        }

        OnUpdate();
    }

    protected void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Debug.Log("notice me");
            engaged = true;
            engagedPlayer = other;
            if (promptText != null) promptText.gameObject.SetActive(true);
            OnPlayerEnter(other);
            PlayerEntered?.Invoke(other);
        }
    }

    protected void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Debug.Log("unnotice me");
            engaged = false;
            engagedPlayer = null;
            if (promptText != null) promptText.gameObject.SetActive(false);
            OnPlayerExit(other);
            PlayerExited?.Invoke(other);
        }
    }

    protected virtual void OnAwake() { }
    protected virtual void OnStart() { }
    protected virtual void OnUpdate() { }
    protected virtual void OnPlayerEnter(Collider other) { }
    protected virtual void OnPlayerExit(Collider other) { }

    protected virtual void OnInteract(Collider player)
    {
        promptText.gameObject.SetActive(false);
        Debug.Log("interacted");
    }
}
