using UnityEngine;

public class EvidenceLog : MonoBehaviour
{
    private EvidenceData[] allCollectibles;

    void Start()
    {
        // Unity looks inside any "Resources" folder for the subfolder "collectibles"
        allCollectibles = Resources.LoadAll<EvidenceData>("evidences");

        Debug.Log($"[Resources] Loaded {allCollectibles.Length} collectibles!");

        foreach (var item in allCollectibles)
        {
            Debug.Log($"Found: {item.name}");
        }
    }
}
