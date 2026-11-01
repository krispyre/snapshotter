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

        LevelLoader levelLoader = FindAnyObjectByType<LevelLoader>();
        if (levelLoader == null)
        {
            Debug.LogError("VentExit: no LevelLoader in the scene.");
            return;
        }

        SceneHandoff.Set(spawnId);
        // LevelLoader.LoadLevelWithTransition(sceneName, transitionTrigger);
    }
}
