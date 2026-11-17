using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class MissionEndTrigger : MonoBehaviour
{
    public void EndLevel()
    {
        StartCoroutine(EndLevelCoro());
    }

    IEnumerator EndLevelCoro()
    {
        yield return new WaitForSeconds(1);
        Debug.Log("play fade to black");
        SceneManager.LoadSceneAsync("gameOver_complete");
    }
}
