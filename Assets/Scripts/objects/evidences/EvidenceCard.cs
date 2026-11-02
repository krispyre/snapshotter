using TMPro;
using UnityEngine;

public class EvidenceCard : MonoBehaviour
{
    public EvidenceData evidenceData;
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
        if (evidenceData == null) return;

        EvidenceData data = evidenceData; //idk icl ts pmo abcdefg
        nameText.text = data.objectName;
        descriptionText.text = data.description;
        systemResponseText.text = data.systemResponse;
        bool showResponse = sendProgress > 0;
        systemResponseText.gameObject.SetActive(showResponse);
        systemResponsePlaceholder.SetActive(!showResponse);
    }
}
