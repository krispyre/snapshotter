using UnityEngine;

public class LayerSwitcher : MonoBehaviour
{
    [SerializeField] GameObject[] enable;
    [SerializeField] GameObject[] disable;

    public void Apply()
    {
        foreach (GameObject go in enable)
            if (go != null) go.SetActive(true);
        foreach (GameObject go in disable)
            if (go != null) go.SetActive(false);
    }
}