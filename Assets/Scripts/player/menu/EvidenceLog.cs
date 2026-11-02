using System;
using UnityEngine;

public class EvidenceLog : MonoBehaviour
{
    private EvidenceData[] allEvidences;
    [SerializeField] private GameObject listParent;
    [SerializeField] private EvidenceCard cardPrefab;

    void Awake()
    {
        gameObject.SetActive(false);
    }

    void OnEnable()
    {
        // Unity looks inside any "Resources" folder for the subfolder "collectibles"
        allEvidences = Resources.LoadAll<EvidenceData>("evidences");

        Debug.Log($"[Resources] Loaded {allEvidences.Length} collectibles!");

        for (int i = listParent.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(listParent.transform.GetChild(i).gameObject);
        }

        foreach (EvidenceData ed in allEvidences)
        {
            Debug.Log($"Found: {ed.name}: state={ed.state}");
            EvidenceCard card = Instantiate(cardPrefab, listParent.transform);
            card.evidenceData = ed;
        }
    }
}
