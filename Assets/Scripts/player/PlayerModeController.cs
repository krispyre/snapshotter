using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerModeController : MonoBehaviour
{
    // only for switching modes
    private PlayerInput playerInput;
    private InputAction dirYAction;
    private InputAction interactAction;
    [SerializeField, ReadOnlyInspector] private bool isDroid = true;
    private void CacheActions()
    {
        if (playerInput == null)
            playerInput = GetComponent<PlayerInput>();

        if (playerInput == null || playerInput.actions == null)
            return;
        dirYAction = playerInput.actions.FindAction("DirY");
        interactAction = playerInput.actions.FindAction("Interact");
    }

    void Awake()
    {
        CacheActions();
    }
    private void OnEnable() => CacheActions();
    void Update()
    {
        if (interactAction.IsPressed() && dirYAction.ReadValue<float>() > 0) //todo change to held tgt
        {
            if (isDroid)
            {
                SwitchToSpider();
            }
        }
    }

    void SwitchToSpider()
    {
        //deactivate droid, spawn dummy droid, then enable spider


    }
    void ReturnToDroid()
    {
        //deactivate droid, spawn dummy droid, then enable spider

    }
}
