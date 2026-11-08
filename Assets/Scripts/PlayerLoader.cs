using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public static class PlayerLoader
{
    internal const string PlayerSceneName = "playerScene";
    private const string MainMenuSceneName = "MainMenu"; //todo not a real scene yet. there are other possible playerfree scenes

    private static bool playerLoadRequested;

    //todo wtf is this??
#if UNITY_EDITOR
    internal const string OpenBeforePlayKey = "snapshotter.playerScene.openBeforePlay";
    internal const string SnapshotTakenKey = "snapshotter.playerScene.snapshotTaken";
    // AfterSceneLoad only guarantees the active scene. The rest of the hierarchy
    // is activated just after it, so ignore autoload until that finishes.
    private static bool waitingForEditorScenes;
    private static int editorSettleId;
#endif

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Init()
    {
        playerLoadRequested = false;
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;

#if UNITY_EDITOR
        waitingForEditorScenes = false;
        int settleId = ++editorSettleId;
        if (SessionState.GetBool(OpenBeforePlayKey, false))
        {
            waitingForEditorScenes = true;
            EditorApplication.delayCall += () => OnEditorScenesSettled(settleId);
            return;
        }
#endif
        EnsurePlayerLoaded(SceneManager.GetActiveScene());
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == PlayerSceneName)
            playerLoadRequested = false;

        EnsurePlayerLoaded(SceneManager.GetActiveScene());
    }

    private static void EnsurePlayerLoaded(Scene scene)
    {
#if UNITY_EDITOR
        if (waitingForEditorScenes) return;
#endif
        if (!scene.IsValid() || scene.name == MainMenuSceneName || scene.name == PlayerSceneName) return;
        if (playerLoadRequested || IsSceneLoaded(PlayerSceneName)) return;

        playerLoadRequested = true;
        AsyncOperation op = SceneManager.LoadSceneAsync(PlayerSceneName, LoadSceneMode.Additive);
        if (op == null)
            playerLoadRequested = false;
    }

    private static bool IsSceneLoaded(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene loaded = SceneManager.GetSceneAt(i);
            if (loaded.name == sceneName && loaded.isLoaded)
                return true;
        }
        return false;
    }

#if UNITY_EDITOR
    private static void OnEditorScenesSettled(int settleId)
    {
        if (settleId != editorSettleId) return;
        waitingForEditorScenes = false;
        if (!Application.isPlaying) return;
        EnsurePlayerLoaded(SceneManager.GetActiveScene());
    }
#endif
}

#if UNITY_EDITOR
[InitializeOnLoad]
static class PlayerLoaderEditorHook
{
    static PlayerLoaderEditorHook()
    {
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    // ExitingEditMode runs before the play-mode domain reload, while every
    // hierarchy scene is still open. SessionState survives that reload, so
    // AfterSceneLoad can see the list even though SceneManager cannot yet.
    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredEditMode)
        {
            SessionState.SetBool(PlayerLoader.SnapshotTakenKey, false);
            return;
        }

        if (state != PlayModeStateChange.ExitingEditMode) return;
        if (SessionState.GetBool(PlayerLoader.SnapshotTakenKey, false)) return;

        SessionState.SetBool(PlayerLoader.SnapshotTakenKey, true);
        SessionState.SetBool(PlayerLoader.OpenBeforePlayKey, IsPlayerSceneOpen());
    }

    private static bool IsPlayerSceneOpen()
    {
        for (int i = 0; i < EditorSceneManager.sceneCount; i++)
        {
            Scene scene = EditorSceneManager.GetSceneAt(i);
            if (scene.isLoaded && scene.name == PlayerLoader.PlayerSceneName)
                return true;
        }
        return false;
    }
}
#endif
