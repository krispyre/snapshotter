using UnityEngine;
using UnityEngine.UI;

public class commsMenu : MonoBehaviour
{
    [System.Serializable]
    public struct Tab
    {
        public Button button;
        public GameObject page;
    }

    [SerializeField] Tab[] tabs;
    [SerializeField, ReadOnlyInspector] int activeTabIndex;

    void Awake()
    {
        if (tabs == null)
            return;

        for (int i = 0; i < tabs.Length; i++)
        {
            int index = i;
            if (tabs[i].button != null)
                tabs[i].button.onClick.AddListener(() => ShowTab(index));
        }
    }

    void OnEnable()
    {
        ShowTab(activeTabIndex);
    }

    void switchNextTab()
    {
        if (tabs == null || tabs.Length == 0)
            return;

        ShowTab((activeTabIndex + 1) % tabs.Length);
    }

    public void ShowTab(int index)
    {
        if (tabs == null || tabs.Length == 0)
            return;

        activeTabIndex = Mathf.Clamp(index, 0, tabs.Length - 1);
        for (int i = 0; i < tabs.Length; i++)
        {
            if (tabs[i].page != null)
                tabs[i].page.SetActive(i == activeTabIndex);
        }
    }
}
