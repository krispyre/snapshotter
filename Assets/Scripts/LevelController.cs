using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelController : MonoBehaviour
{
    [SerializeField] LevelData levelData;
    public Animator transition;

    void Start()
    {
        PointSystem.Instance.SetThresholds(levelData.pointThresholds);
        PointSystem.Instance.LoadMission(levelData.missionId);
        PointSystem.Instance.onPointsDepleted.AddListener(OnPlayerDeath);
    }

    void OnPlayerDeath()
    {
        Debug.LogWarning("U die");
    }

    public void LoadLevel(string levelName)
    {
        StartCoroutine(LoadLevelCoro(levelName));
    }

    IEnumerator LoadLevelCoro(string levelName)
    {

        transition.SetTrigger("enterElevator");
        yield return new WaitForSeconds(1); // todo hardcode
        SceneManager.LoadScene(levelName);

    }
}
