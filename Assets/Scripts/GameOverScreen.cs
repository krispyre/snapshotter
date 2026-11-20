using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class GameOverScreen : MonoBehaviour
{
    [SerializeField] private GameStates gameStates;
    [SerializeField] private TMP_Text scoreText;
    private PlayerInput player;
    void Start()
    {
        player = FindFirstObjectByType<PlayerInput>();
        player.actions.FindActionMap("Player").Disable();
        scoreText.SetText("Score: " + gameStates.Score);
    }
}
