using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    [SerializeField] LevelData levelData;
    [SerializeField, ReadOnlyInspector] string arrivalSpawnId;

    void Start()
    {
        StealthPoints.Instance.SetThresholds(levelData.stealthPointsThresholds);
        StealthPoints.Instance.LoadMission();
        StealthPoints.Instance.onStealthPointsDepleted.AddListener(OnPlayerDeath);
    }

    void OnPlayerDeath()
    {
        Debug.LogWarning("U die");
        // SceneManager.LoadSceneAsync("gameOver_fail", LoadSceneMode.Additive);

    }

    // public void LoadLevelWithTransition(string levelName, string transitionTrigger)
    // {
    //     StartCoroutine(LoadLevelCoro(levelName, transitionTrigger));
    // }

    // IEnumerator LoadLevelCoro(string levelName, string transitionTrigger)
    // {
    //     if (!string.IsNullOrEmpty(transitionTrigger))
    //     {
    //         if (HasTrigger(transitionTrigger))
    //             transition.SetTrigger(transitionTrigger);
    //         else
    //             Debug.LogWarning($"No transition trigger '{transitionTrigger}'.");
    //         yield return new WaitForSeconds(1); // todo hardcode
    //     }

    //     SceneManager.LoadSceneAsync(levelName);
    // }



    // bool HasTrigger(string triggerName)
    // {
    //     if (transition == null)
    //         return false;

    //     foreach (AnimatorControllerParameter parameter in transition.parameters)
    //     {
    //         if (parameter.type == AnimatorControllerParameterType.Trigger && parameter.name == triggerName)
    //             return true;
    //     }
    //     return false;
    // }



}
