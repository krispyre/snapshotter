using UnityEngine;

public class MissionEndEnabler : MonoBehaviour
{
    [SerializeField] private GameObject missionEndTrigger;

    void Awake()
    {
        if (PlayerPrefs.GetInt("MissionEndEnabled") < 1)
        {
            Debug.Log("disable end");
            missionEndTrigger.SetActive(false);
        }
    }
    public void EnableTrigger()
    {
        Debug.Log("enable end");
        PlayerPrefs.SetInt("MissionEndEnabled", 1); // extremely hacky but easy to setup. this will do
        missionEndTrigger.SetActive(true);
    }
}
