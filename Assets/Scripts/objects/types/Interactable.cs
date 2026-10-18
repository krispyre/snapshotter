using UnityEngine;
using UnityEngine.InputSystem;

public class Interactable : MonoBehaviour
{
    [SerializeField, ReadOnlyInspector] protected bool engaged;
    public string prompt;
    protected Collider trigger;
    private MeshRenderer meshRenderer;

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
            OnInteract();
        }
        OnUpdate();
    }

    protected void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Debug.Log("notice me");
            engaged = true;
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
            OnPlayerExit(other);
            PlayerExited?.Invoke(other);
        }
    }

    protected virtual void OnAwake() { }
    protected virtual void OnUpdate() { }
    protected virtual void OnPlayerEnter(Collider other) { }
    protected virtual void OnPlayerExit(Collider other) { }

    protected virtual void OnInteract()
    {
        Debug.Log("interacted");
    }
}
