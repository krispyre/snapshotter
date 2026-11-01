using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class RoomTransitionMgr : MonoBehaviour
{
    // on the plaeyr,.,sa;dfalksdfjakdsjflksajlfj
    
     public void EnterRoom(string sceneName, string spawnID)
    {
        StartCoroutine(Transition(sceneName, spawnID));
    }

    private IEnumerator Transition(string sceneName, string spawnID)
    {
        yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

    }
}
