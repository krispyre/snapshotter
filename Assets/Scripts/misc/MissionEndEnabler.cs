using UnityEngine;

public class MissionEndEnabler : MonoBehaviour
{
    [SerializeField] private GameObject missionEndTrigger;

    void Start()
    {
        missionEndTrigger.SetActive(false);
    }
    public void EnableTrigger()
    {
        Debug.Log("enable end");
        missionEndTrigger.SetActive(true);
    }
}
