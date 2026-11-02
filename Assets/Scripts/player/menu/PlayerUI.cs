using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private Canvas commLogCanvas;

    private InputActionMap playerMap;
    private InputActionMap uiMap;
    private InputAction commsAction;
    private InputAction closeCommsAction;
    private bool isOpen;

    private void Awake()
    {
        if (commLogCanvas == null)
            commLogCanvas = GetComponent<Canvas>();

        if (playerInput == null || playerInput.actions == null)
        {
            Debug.LogError("PlayerUI requires a PlayerInput.", this);
            enabled = false;
            return;
        }

        playerMap = playerInput.actions.FindActionMap("Player");
        uiMap = playerInput.actions.FindActionMap("UI");
        commsAction = playerInput.actions.FindAction("Comms");
        closeCommsAction = playerInput.actions.FindAction("CloseComms");
    }

    private void Start()
    {
        SetOpen(false);
    }

    private void Update()
    {
        // TODO: check the condition under which the player is allowed to toggle the UI
        // todo distinguish double tap n single tap?
        if (!isOpen && commsAction != null && commsAction.WasPressedThisFrame())
            SetOpen(true);
        else if (isOpen && closeCommsAction != null && closeCommsAction.WasPressedThisFrame())
            SetOpen(false);
    }

    private void SetOpen(bool open)
    {
        isOpen = open;

        if (commLogCanvas != null)
            commLogCanvas.gameObject.SetActive(open);

        if (open)
        {
            playerMap?.Disable();
            uiMap?.Enable();
        }
        else
        {
            uiMap?.Disable();
            playerMap?.Enable();
        }
    }
}
