using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[System.Serializable]
public class SwitchModeEvent : UnityEvent<bool> { }

public class PlayerModeController : MonoBehaviour
{
    public SwitchModeEvent switchMode = new SwitchModeEvent();

    [SerializeField] private GameStates gameStates;
    private PlayerInput playerInput;
    private InputAction dirYAction;
    private InputAction interactAction;
    private bool isDroid = true;

    private OpenCam openCam;// contains the opencam controller script, calls the changestate() function when player mode changes
    private GameObject droid;
    private GameObject spider;
    private GameObject dummy;
    [SerializeField] public float holdDuration = 2;
    [SerializeField, ReadOnlyInspector] private float holdTimer = 0;
    [SerializeField] cameraFrame cameraFrame; // contains the camera frame script, calls the changeState() function when player mode changes


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
        if (gameStates == null)
        {
            Debug.LogError("PlayerModeController requires a GameStates asset.", this);
            enabled = false;
            return;
        }

        CacheActions();
        openCam = GetComponent<OpenCam>();
        droid = transform.Find("droid").gameObject;
        spider = transform.Find("spider").gameObject;
        dummy = transform.Find("droidDummy").gameObject;
    }

    void Start()
    {
        isDroid = gameStates.IsDroid;
        droid.SetActive(isDroid);
        dummy.SetActive(!isDroid);
        spider.SetActive(!isDroid);

        if (openCam != null)
            openCam.SetCameraOpen(gameStates.CamMode);
    }

    private void OnEnable() => CacheActions();
    void Update()
    {
        if (interactAction.IsPressed() && dirYAction.ReadValue<float>() > 0 && droid.GetComponent<DroidMovement>().state == DroidMovement.PlayerState.Idle && !gameStates.CamMode) //todo change to held tgt
        {
            holdTimer += Time.deltaTime;
        }
        else if (holdTimer >= 0)
        {
            holdTimer -= 3 * Time.deltaTime;
        }
        else
        {
            holdTimer = 0;
        }

        if (isDroid && holdTimer >= holdDuration)
        {
            holdTimer = 0;
            SwitchToSpider();
        }
    }

    public void SwitchToSpider()
    {
        //deactivate droid, spawn dummy droid, then enable spider at droid
        SwitchMode(false);
        RoomTransitionMgr rooms = GetComponent<RoomTransitionMgr>();
        gameStates.SetDroidLocation(rooms != null ? rooms.currentRoomName : "", droid.transform.position);
        droid.SetActive(false);
        dummy.transform.position = droid.transform.position;
        dummy.SetActive(true);
        spider.transform.position = droid.transform.position;
        spider.SetActive(true);
    }
    public void ReturnToDroid()
    {
        //deactivate spider, despawn dummy droid, then enable droid
        SwitchMode(true);
        droid.SetActive(true);
        dummy.SetActive(false);
        spider.SetActive(false);
    }

    public void SwitchMode(bool isDroid)
    {
        this.isDroid = isDroid;
        if (!gameStates.SetIsDroid(isDroid))
            return;

        switchMode.Invoke(isDroid);
    }
}
