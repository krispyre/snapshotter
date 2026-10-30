using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.InputSystem;
using TMPro;

[DefaultExecutionOrder(-1000)]
public class GameReferences : MonoBehaviour
{
    public static GameReferences Instance { get; private set; }

    [Header("GameEvents")]
    public GameEvent gameEvent;

    [Header("Cameras")]
    [SerializeField] public CinemachineCamera droidCamera;
    [SerializeField] public CinemachineCamera spiderCamera;
    [SerializeField] public CinemachineCamera droidRecordCamera;
    [SerializeField] public CinemachineCamera spiderRecordCamera;

    [Header("Player")]
    [SerializeField] public PlayerInput playerInput;

    [Header("UI Elements")]
    [SerializeField] public GameObject cameraFrame;
    [SerializeField] public GameObject evidencePage;
    public TextMeshProUGUI EvidenceName;
    public TextMeshProUGUI EvidenceDescription;
    public TextMeshProUGUI Systemprompt;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("More than one GameReferences instance is active.", this);
            enabled = false;
            return;
        }

        Instance = this;
        if (gameEvent == null)
        {
            Debug.LogError("GameReferences requires a GameEvent asset.", this);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
