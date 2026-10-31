using UnityEngine;

public class VentExit : Interactable
{
    [SerializeField] string sceneName = "ninthFloor_roof";
    [SerializeField] string transitionTrigger;
    [SerializeField] string spawnId;

    protected override void OnAwake()
    {
        prompt = "Exit";
    }

    protected override void OnInteract(Collider player)
    {
        if (promptText != null)
            promptText.gameObject.SetActive(false);

        LevelController levelController = FindAnyObjectByType<LevelController>();
        if (levelController == null)
        {
            Debug.LogError("VentExit: no LevelController in the scene.");
            return;
        }

        SceneHandoff.Set(spawnId);
        levelController.LoadLevelWithTransition(sceneName, transitionTrigger);
    }
}
