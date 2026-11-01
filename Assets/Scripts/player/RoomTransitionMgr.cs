using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class RoomTransitionMgr : MonoBehaviour
{
    // on the plaeyr,.,sa;dfalksdfjakdsjflksajlfj
    [SerializeField] DroidLocation droidLocation;
    public string currentRoomName = "";

    void Start()
    {
        EnterRoom("", "", null);
    }

    public void EnterRoom(string sceneName, string spawnID, Collider body)
    {
        StartCoroutine(Transition(sceneName, spawnID, body));
    }

    private IEnumerator Transition(string sceneName, string spawnID, Collider body)
    {
        if (!string.IsNullOrEmpty(currentRoomName))
        {

            yield return SceneManager.UnloadSceneAsync(currentRoomName);
            yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            currentRoomName = sceneName;
        }

        // move the pointer at active to the new scene
        Scene newScene = SceneManager.GetSceneByName(sceneName);

        if (newScene.IsValid())
        {
            SceneManager.SetActiveScene(newScene);
        }

        currentRoomName = SceneManager.GetActiveScene().name;
        SetupRoom(spawnID, body);
    }

    private void SetupRoom(string spawnID, Collider body)
    {
        if (body == null)
            return;

        SpawnPoint[] spawns = FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None);
        if (spawns.Length == 0)
            return;

        SpawnPoint spawnToUse = spawns[0];

        if (!string.IsNullOrEmpty(spawnID))
        {
            foreach (SpawnPoint sp in spawns)
            {
                if (sp.spawnID == spawnID)
                {
                    spawnToUse = sp;
                    break;
                }
            }
        }

        CharacterController cc = body.GetComponentInParent<CharacterController>();
        Transform moved = body.transform;

        bool wasEnabled = cc.enabled;
        if (cc != null)
            cc.enabled = false;

        moved.position = spawnToUse.transform.position;
        cc.enabled = wasEnabled;

        if (moved.name == "spider")
        {
            Transform dummy = transform.Find("droidDummy");

            // check if droid is in current scene )
            bool home = droidLocation.sceneName == currentRoomName;
            dummy.gameObject.SetActive(home);
            if (home)
                dummy.position = droidLocation.lastPosition;
        }
    }
}
