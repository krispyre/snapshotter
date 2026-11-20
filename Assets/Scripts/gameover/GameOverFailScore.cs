using TMPro;
using UnityEngine;

public class GameOverFailScore : MonoBehaviour
{
    [SerializeField] private GameStates gameStates;
    [SerializeField] private TMP_Text tmp;
    void Start()
    {
        tmp.SetText("Score: " + gameStates.Score);
    }
}
