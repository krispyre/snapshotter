using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.Events;

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

    [SerializeField, ReadOnlyInspector] private PlayerInput playerInput;
    private InputAction interactAction;
    private InputAction dirXAction;
    private InputAction dirYAction;

    public bool Engaged => engaged;
    [SerializeField] protected UnityEvent onAwake = new UnityEvent();
    [SerializeField] protected UnityEvent onStart = new UnityEvent();
    [SerializeField] protected UnityEvent onUpdate = new UnityEvent();
    [SerializeField] protected UnityEvent<Collider> onPlayerEnter = new UnityEvent<Collider>();
    [SerializeField] protected UnityEvent<Collider> onPlayerExit = new UnityEvent<Collider>();
    [SerializeField] protected UnityEvent<Collider> onInteract = new UnityEvent<Collider>();

    // GetComponent<MeshRenderer>().material = Material1;
    protected void Awake()
    {
        trigger = GetComponent<Collider>();
        meshRenderer = GetComponent<MeshRenderer>();
        OnAwake();
        onAwake.Invoke();
    }
    protected void Start()
    {
        // both the group parent and spider share the Player tag idk how to fix it yet just check whichever has the comp
        foreach (GameObject playerObject in GameObject.FindGameObjectsWithTag("Player"))
        {
            playerInput = playerObject.GetComponentInParent<PlayerInput>();
            if (playerInput != null)
                break;
        }

        if (playerInput != null && playerInput.actions != null)
        {
            interactAction = playerInput.actions.FindAction("Interact");
            dirXAction = playerInput.actions.FindAction("DirX");
            dirYAction = playerInput.actions.FindAction("DirY");
        }
        else
        {
            Debug.LogError("Interactable could not find a PlayerInput on the Player.", this);
        }

        mainCamera = Camera.main;

        if (promptText != null)
        {
            promptText.text = prompt;
            promptText.gameObject.SetActive(false);
        }
        OnStart();
        onStart.Invoke();
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
            onInteract.Invoke(engagedPlayer);
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
        onUpdate.Invoke();
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
            onPlayerEnter.Invoke(other);
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
            onPlayerExit.Invoke(other);
        }
    }

    protected virtual void OnAwake() { }
    protected virtual void OnStart() { }
    protected virtual void OnUpdate() { }
    protected virtual void OnPlayerEnter(Collider other) { }
    protected virtual void OnPlayerExit(Collider other) { }

    protected virtual void OnInteract(Collider player)
    {
        if (promptText != null)
            promptText.gameObject.SetActive(false);
    }

    public void ShowPrompt()
    {
        //todo prompttext is moved to player hud
        if (promptText != null)
            promptText.gameObject.SetActive(true);
    }
}
