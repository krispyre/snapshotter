using UnityEngine;
using UnityEngine.SceneManagement;

public static class PlayerLoader
{
    private const string PlayerSceneName = "playerScene";
    private const string MainMenuSceneName = "MainMenu"; //todo not a real scene yet. there are other possible playerfree scenes

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Init(){
        SceneManager.sceneLoaded += OnSceneLoaded;
        EnsurePlayerLoaded(SceneManager.GetActiveScene());
    }

// This doesnt check if playerScene is there in the editor, isloaded only fires for this single scene,,,
    private static void EnsurePlayerLoaded(Scene scene){
        if (scene.name == MainMenuSceneName) return;

        if (!SceneManager.GetSceneByName(PlayerSceneName).isLoaded){
            SceneManager.LoadSceneAsync(PlayerSceneName, LoadSceneMode.Additive);
        }
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode){
        // runs for all scenes
        EnsurePlayerLoaded(SceneManager.GetActiveScene());
    }
}
