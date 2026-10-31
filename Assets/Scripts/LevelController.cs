using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelController : MonoBehaviour
{
    [SerializeField] LevelData levelData;
    public Animator transition;
    [SerializeField, ReadOnlyInspector] string arrivalSpawnId;

    void Start()
    {
        SceneHandoff arrival = SceneHandoff.Take();
        arrivalSpawnId = arrival != null ? arrival.spawnId : null;

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
        SceneHandoff.Clear();
        StartCoroutine(LoadLevelCoro(levelName, "enterElevator"));
    }

    public void LoadLevelWithTransition(string levelName, string transitionTrigger)
    {
        StartCoroutine(LoadLevelCoro(levelName, transitionTrigger));
    }

    IEnumerator LoadLevelCoro(string levelName, string transitionTrigger)
    {
        if (!string.IsNullOrEmpty(transitionTrigger))
        {
            if (HasTrigger(transitionTrigger))
                transition.SetTrigger(transitionTrigger);
            else
                Debug.LogWarning($"No transition trigger '{transitionTrigger}'.");
            yield return new WaitForSeconds(1); // todo hardcode
        }

        SceneManager.LoadScene(levelName);
    }

    bool HasTrigger(string triggerName)
    {
        if (transition == null)
            return false;

        foreach (AnimatorControllerParameter parameter in transition.parameters)
        {
            if (parameter.type == AnimatorControllerParameterType.Trigger && parameter.name == triggerName)
                return true;
        }
        return false;
    }
}
