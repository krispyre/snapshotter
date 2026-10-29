using TMPro;
using UnityEngine;

public class EvidenceCard : MonoBehaviour
{
    public Evidence evidence;
    public int sendProgress;

    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI descriptionText;
    [SerializeField] TextMeshProUGUI systemResponseText;
    [SerializeField] GameObject systemResponsePlaceholder;

    void Awake()
    {
        if (nameText == null)
            nameText = transform.Find("name").GetComponent<TextMeshProUGUI>();
        if (descriptionText == null)
            descriptionText = transform.Find("info/selfDesc").GetComponent<TextMeshProUGUI>();
        if (systemResponseText == null)
            systemResponseText = transform.Find("info/sysResSlot/sysRes").GetComponent<TextMeshProUGUI>();
        if (systemResponsePlaceholder == null)
            systemResponsePlaceholder = transform.Find("info/sysResSlot/sysRes_placeholder").gameObject;
    }

    void Start()
    {
        Apply();
    }

    public void Apply()
    {
        if (evidence == null || evidence.evidenceData == null) return;

        EvidenceData data = evidence.evidenceData;
        nameText.text = data.objectName;
        descriptionText.text = data.description;
        systemResponseText.text = data.systemResponse;
        bool showResponse = sendProgress > 0;
        systemResponseText.gameObject.SetActive(showResponse);
        systemResponsePlaceholder.SetActive(!showResponse);
    }
}
