using UnityEngine;

[CreateAssetMenu(fileName = "EvidenceData", menuName = "Scriptable Objects/EvidenceData")]
public class EvidenceData : ScriptableObject
{
    public string objectName;
    [TextArea(5, 10)]
    public string description;
    [TextArea(5, 10)]
    public string systemResponse;
}
