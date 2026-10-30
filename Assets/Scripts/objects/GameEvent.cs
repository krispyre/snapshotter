using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class EvidenceDataEvent : UnityEvent<EvidenceData>
{
}

[CreateAssetMenu(fileName = "GameEvent", menuName = "Scriptable Objects/GameEvent")]
public class GameEvent : ScriptableObject
{
    public UnityEvent OnOpenCam = new UnityEvent();
    public UnityEvent OnShutCam = new UnityEvent();
    public UnityEvent SwitchToSpider = new UnityEvent();
    public UnityEvent SwitchToDroid = new UnityEvent();
    public EvidenceDataEvent OnEvidenceSeen = new EvidenceDataEvent();
    public EvidenceDataEvent OnEvidenceTaken = new EvidenceDataEvent();
    public EvidenceDataEvent OnEvidenceSent = new EvidenceDataEvent();


    private bool openCam;
    public bool CamMode => openCam;
    private bool spiderMode;
    public bool PlayerModeIsSpider => spiderMode;

    public void Changemode(bool isSpider)
    {
        if(isSpider == spiderMode) return;
        spiderMode = isSpider;
        ToggleCamera(false); // Close the camera when switching modes

        if (isSpider)
        {
            SwitchToSpider?.Invoke();
        }
        else
        {
            SwitchToDroid?.Invoke();
        }
    }

    public void ToggleCamera(bool isOpenCam)
    {
        if (openCam == isOpenCam) return;
        openCam = isOpenCam;

        if (isOpenCam)
        {
            OnOpenCam?.Invoke();
        }
        else
        {
            OnShutCam?.Invoke();
        }
    }
    public void SeeEvidence(EvidenceData evidence)
    {
        OnEvidenceSeen?.Invoke(evidence);
        evidence.SetState(1);

    }

    public void TakeEvidence(EvidenceData evidence)
    {
        OnEvidenceTaken?.Invoke(evidence);
        evidence.SetState(2);
    }
    public void SendEvidence(EvidenceData evidence)
    {
        OnEvidenceSent?.Invoke(evidence);
        evidence.SetState(3);
    }
    
}
