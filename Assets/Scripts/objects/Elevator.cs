using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Elevator : MonoBehaviour
{
    [SerializeField] private float waitTimeMax = 2; // wait till open or wait till close
    [SerializeField, ReadOnlyInspector] private float waitTimer;
    [SerializeField] private bool waitTimerTimeout = false;
    [SerializeField] private bool doorOpen = false;
    [SerializeField] private Animator panelUi_anim;
    [SerializeField] private Interactable interactable;
    private PlayerInput lockedPlayerInput;
    private Collider riding;
    private MeshRenderer meshRenderer;


    void Awake()
    {
        interactable = GetComponentInParent<Interactable>();
        meshRenderer = GetComponent<MeshRenderer>();
        SetDoorVisual(doorOpen);
    }

    void Start()
    {
        panelUi_anim.SetBool("IsOpen", false);
        panelUi_anim.Play("elevatorPanel_exit", 0, 1f); //hide the panel first, then show using animns
        panelUi_anim.Update(0f);
    }

    void SetDoorVisual(bool open)
    {
        if (meshRenderer != null)
            meshRenderer.enabled = open;
    }

    // wait 0.5s to open the door
    public void ButtonCall()
    {
        if (waitTimer <= 0 && !doorOpen)
        {
            Debug.Log("press button");
            waitTimer = 1f; //Random.Range(0, waitTimeMax * 10f) / 0.1f; //todo add this back
        }
    }

    public void OpenPanel(Collider player)
    {
        if (doorOpen)
        {
            Debug.Log("open elevator ui");
            riding = player;
            lockedPlayerInput = player.GetComponentInParent<PlayerInput>();
            if (lockedPlayerInput != null)
                lockedPlayerInput.actions.FindActionMap("Player").Disable();
            panelUi_anim.SetBool("IsOpen", true);
        }
    }

    // Floor button OnClick → Ride, string arg is the destination scene name.
    // Lands on the ElevatorButton child named "spawn" (SpawnPoint id "elevator").
    public void Ride(string sceneName)
    {
        if (riding == null || string.IsNullOrEmpty(sceneName))
            return;

        RoomTransitionMgr manager = riding.GetComponentInParent<RoomTransitionMgr>();
        Collider body = riding;
        ClosePanelUi();

        if (manager)
            manager.EnterRoom(sceneName, "elevator", body);
    }

    void Update()
    {
        if (waitTimer > 0)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0)
            {
                waitTimer = 0;
                waitTimerTimeout = true;
            }
        }


        if (waitTimerTimeout)
        {
            waitTimerTimeout = false;

            if (doorOpen)
            {
                // close the door
                Debug.Log("close door");
                doorOpen = false;
                SetDoorVisual(false);
            }
            else
            {
                //open the door
                Debug.Log("open door");

                doorOpen = true;
                SetDoorVisual(true);

                // begin timer decrement
                waitTimer = 5;
            }
        }

        if (interactable != null && interactable.Engaged && doorOpen)
        {
            waitTimer = 5f;
            // Debug.Log("occupied, do not decrement");
        }
    }

    // Wire Exit Button OnClick → this Elevator (prefab-local), no scene player ref needed
    public void ClosePanelUi()
    {
        panelUi_anim.SetBool("IsOpen", false);
        if (interactable != null)
            interactable.ShowPrompt();

        riding = null;

        if (lockedPlayerInput != null)
        {
            lockedPlayerInput.actions.FindActionMap("Player").Enable();
            lockedPlayerInput = null;
        }
    }
}
