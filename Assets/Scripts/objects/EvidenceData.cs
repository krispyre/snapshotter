using UnityEngine;

[CreateAssetMenu(fileName = "EvidenceData", menuName = "Scriptable Objects/EvidenceData")]
public class EvidenceData : ScriptableObject
{
    public string objectName;
    [TextArea(5, 10)]
    public string description;
    [TextArea(5, 10)]
    public string systemResponse;

    public int state; //0 = not taken, 1 = seen, 2 = taken, 3 = sent;
    public void SetState(int newState)
    {
        state = newState;
    }

    private void OnEnable()
    {
        state = 0; // todo this is a debug reset
    }
}
