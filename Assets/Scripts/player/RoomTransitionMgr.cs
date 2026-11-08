using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class RoomTransitionMgr : MonoBehaviour
{
    // on the plaeyr,.,sa;dfalksdfjakdsjflksajlfj
    [SerializeField] GameStates gameStates;
    public string currentRoomName = "";

    void Start()
    {
        EnterRoom("", "", null);
    }

    public void EnterRoom(string sceneName, string spawnID, Collider body, RoomTransition roomTransition = null)
    {
        StartCoroutine(Transition(sceneName, spawnID, body, roomTransition));
    }

    private IEnumerator Transition(string sceneName, string spawnID, Collider body, RoomTransition roomTransition)
    {
        GameObject enterPrefab = null;
        GameObject exitPrefab = null;
        string enterState = null;
        string exitState = null;
        if (roomTransition != null)
        {
            enterPrefab = roomTransition.transitonEnterPrefab;
            exitPrefab = roomTransition.transitonExitPrefab;
            enterState = roomTransition.transitonEnterVersion;
            exitState = roomTransition.transitonExitVersion;
        }

        if (roomTransition != null && enterPrefab == null)
            Debug.LogError($"No enter prefab on {roomTransition.name}", roomTransition);

        GameObject enterInstance = null;
        if (enterPrefab != null)
        {
            enterInstance = SpawnTransition(enterPrefab, enterState);
            yield return WaitForAnim(enterInstance);
        }

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

        if (roomTransition != null && exitPrefab == null)
            Debug.LogError($"No exit prefab on {roomTransition.name}", roomTransition);

        GameObject exitInstance = null;
        if (exitPrefab != null)
            exitInstance = SpawnTransition(exitPrefab, exitState);

        if (enterInstance != null)
            Destroy(enterInstance);

        if (exitInstance != null)
        {
            yield return WaitForAnim(exitInstance);
            Destroy(exitInstance);
        }
    }

    GameObject SpawnTransition(GameObject prefab, string state)
    {
        if (prefab == null)
            return null;

        GameObject instance = Instantiate(prefab);
        SceneManager.MoveGameObjectToScene(instance, gameObject.scene);

        if (!instance.TryGetComponent(out Animator anim))
        {
            Debug.LogError($"SpawnTransition: no Animator on {instance.name}", instance);
            return instance;
        }

        if (string.IsNullOrEmpty(state))
        {
            Debug.LogError($"SpawnTransition: no state name for {prefab.name}", instance);
            return instance;
        }

        anim.Play(state, 0, 0f);
        anim.Update(0f);
        return instance;
    }

    static IEnumerator WaitForAnim(GameObject instance)
    {
        if (instance == null)
        {
            Debug.LogError("WaitForAnim: instance is null");
            yield break;
        }

        if (!instance.TryGetComponent(out Animator anim))
        {
            Debug.LogError($"WaitForAnim: no Animator on {instance.name}", instance);
            yield break;
        }

        yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length);
    }

    private void SetupRoom(string spawnID, Collider body)
    {
        if (body == null)
            return;

        RoomTransitionDest[] spawns = FindObjectsByType<RoomTransitionDest>(FindObjectsSortMode.None);
        if (spawns.Length == 0)
            return;

        RoomTransitionDest spawnToUse = spawns[0];

        if (!string.IsNullOrEmpty(spawnID))
        {
            foreach (RoomTransitionDest sp in spawns)
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

        if (spawnToUse.TryGetComponent<LayerSwitcher>(out LayerSwitcher ls))
        {
            ls.Apply();
        }
    }
}
