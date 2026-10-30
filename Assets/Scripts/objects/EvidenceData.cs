using UnityEngine;

[CreateAssetMenu(fileName = "EvidenceData", menuName = "Scriptable Objects/EvidenceData")]
public class EvidenceData : ScriptableObject
{
    public string objectName;
    public string description;
    public string systemResponse;
    
    public void SetState(int newState)
    {
        state = newState;
    }
    [SerializeField]
    public int state= 0 ; //0 = not taken, 1 = seen, 2 = taken, 3 = sent;

    private void OnEnable()
    {
        state = 0;
    }
}
