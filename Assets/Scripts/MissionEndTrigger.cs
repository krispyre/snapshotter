using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class MissionEndTrigger : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        StartCoroutine(EndLevel());
    }

    IEnumerator EndLevel()
    {
        yield return new WaitForSeconds(1);
        Debug.Log("play fade to black");
        SceneManager.LoadSceneAsync("gameOver_complete");
    }
}
