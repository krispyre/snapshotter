using UnityEngine;
using UnityEngine.InputSystem;

public class Interactable : MonoBehaviour
{
    [SerializeField, ReadOnlyInspector] protected bool engaged;
    public string prompt;
    protected Collider trigger;
    private MeshRenderer meshRenderer;

    [SerializeField] private PlayerInput playerInput;
    private InputAction interactAction;

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
        }
    }

    protected void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Debug.Log("unnotice me");
            engaged = false;
            OnPlayerExit(other);
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
