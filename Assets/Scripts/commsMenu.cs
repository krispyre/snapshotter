using UnityEngine;

public class commsMenu : MonoBehaviour
{
    [SerializeField] GameObject[] tabs;
    [SerializeField, ReadOnlyInspector] int activeTabIndex;

    void switchNextTab()
    {
        activeTabIndex = (activeTabIndex + 1) % tabs.Length;
    }

}