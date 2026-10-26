using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerModeController : MonoBehaviour
{
    // only for switching modes
    private PlayerInput playerInput;
    private InputAction dirYAction;
    private InputAction interactAction;
    [SerializeField] private bool isDroid = true;
    private GameObject droid;
    private GameObject spider;
    private GameObject dummy;
    [SerializeField] public float holdDuration = 2;
    [SerializeField, ReadOnlyInspector] private float holdTimer = 0;
    [SerializeField] cameraFrame cameraFrame; // contains the camera frame script, calls the changeState() function when player mode changes
    private OpenCam openCam;// contains the opencam controller script, calls the changestate() function when player mode changes

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
        openCam.changeState(!isDroid);
        if (cameraFrame != null)
            cameraFrame.changeMode(!isDroid);
    }

    private void OnEnable() => CacheActions();
    void Update()
    {
        if (interactAction.IsPressed() && dirYAction.ReadValue<float>() > 0 && droid.GetComponent<DroidMovement>().state == DroidMovement.PlayerState.Idle && !openCam.isOpenCam) //todo change to held tgt
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
        isDroid = false;
        droid.SetActive(false);
        dummy.transform.position = droid.transform.position;
        dummy.SetActive(true);
        spider.transform.position = droid.transform.position;
        spider.SetActive(true);
        openCam.changeState(true);
        cameraFrame.changeMode(true);
    }
    public void ReturnToDroid()
    {
        //deactivate spider, despawn dummy droid, then enable droid
        isDroid = true;
        droid.SetActive(true);
        dummy.SetActive(false);
        spider.SetActive(false);
        openCam.changeState(false);
        cameraFrame.changeMode(false);


    }
}
