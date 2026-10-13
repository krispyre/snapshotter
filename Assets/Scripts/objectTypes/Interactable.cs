using UnityEngine;
using UnityEngine.InputSystem;

public class Interactable : MonoBehaviour
{
    [SerializeField, ReadOnlyInspector] protected bool engaged;
    public string prompt;
    protected Collider enterTrigger;
    private MeshRenderer meshRenderer;

    [SerializeField] private PlayerInput playerInput;
    private InputAction interactAction;

    // GetComponent<MeshRenderer>().material = Material1;
    protected virtual void Awake()
    {
        enterTrigger = GetComponent<Collider>();
        meshRenderer = GetComponent<MeshRenderer>();

        interactAction = playerInput.actions.FindAction("Interact");
    }

    void Update()
    {
        if (interactAction.WasPressedThisFrame())
        {
            OnInteract();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("notice me");
            engaged = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("unnotice me");
            engaged = false;
        }
    }

    protected virtual void OnInteract()
    {
        Debug.Log(prompt);
    }
}
