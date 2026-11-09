using UnityEngine;

[RequireComponent(typeof(Evidence))]
public class NPCAction_001 : MonoBehaviour
{   
    public float speed = 1f;
    public float loopInterval = 1f;
    private float timerAccumulator = 0;
    private float moveDirection = 1f;
    [SerializeField] private EvidenceData stateA;
    [SerializeField] private EvidenceData stateB;
    private Evidence evidence;
    private bool showingA;

    void Start()
    {
        evidence = GetComponent<Evidence>();
        SetState(true);
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector3.right * moveDirection * speed * Time.deltaTime, Space.World);

        timerAccumulator += Time.deltaTime;
        if (timerAccumulator >= loopInterval)
        {
            timerAccumulator -= loopInterval;
            SwitchState();
        }
        
    }
    public void SetState(bool newState)
    {
        showingA = newState;
        evidence.evidenceData = showingA? stateA : stateB;
    }


    public void SwitchState()
    {
        moveDirection *= -1f;
        SetState(!showingA);
    }
}
