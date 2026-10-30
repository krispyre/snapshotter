using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerModeController : MonoBehaviour
{
    // only for switching modes
    private GameEvent gameEvent;
    private GameReferences gameReferences;
    private PlayerInput playerInput;
    private InputAction dirYAction;
    private InputAction interactAction;
    [SerializeField] private bool isDroid = true;
    private GameObject droid;
    private GameObject spider;
    private GameObject dummy;
    [SerializeField] public float holdDuration = 2;
    [SerializeField, ReadOnlyInspector] private float holdTimer = 0;
    private OpenCam openCam;

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
        gameReferences = GameReferences.Instance;
        gameEvent = gameReferences.gameEvent;
        openCam = GetComponent<OpenCam>();
        CacheActions();
        droid = transform.Find("droid").gameObject;
        spider = transform.Find("spider").gameObject;
        dummy = transform.Find("droidDummy").gameObject;


        droid.SetActive(isDroid);
        dummy.SetActive(!isDroid);
        spider.SetActive(!isDroid);
    }

    void Start()
    {
        // Sync camera priorities with the starting mode (droid by default).
        gameEvent.Changemode(!isDroid);
    }

    private void OnEnable() => CacheActions();
    void Update()
    {
        if (interactAction.IsPressed() && dirYAction.ReadValue<float>() > 0 && droid.GetComponent<DroidMovement>().state == DroidMovement.PlayerState.Idle && !gameEvent.CamMode) //todo change to held tgt
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
        gameEvent.Changemode(true);
        isDroid = false;
        droid.SetActive(false);
        dummy.transform.position = droid.transform.position;
        dummy.SetActive(true);
        spider.transform.position = droid.transform.position;
        spider.SetActive(true);
    }
    public void ReturnToDroid()
    {
        //deactivate spider, despawn dummy droid, then enable droid
        gameEvent.Changemode(false);
        isDroid = true;
        droid.SetActive(true);
        dummy.SetActive(false);
        spider.SetActive(false);
    }
}
