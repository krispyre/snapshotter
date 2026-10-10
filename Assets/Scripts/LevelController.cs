using UnityEngine;

public class LevelController : MonoBehaviour
{
    [SerializeField] LevelData levelData;

    void Start()
    {
        PointSystem.Instance.SetThresholds(levelData.pointThresholds);
        PointSystem.Instance.onPointsDepleted.AddListener(OnPlayerDeath);
    }

    void OnPlayerDeath()
    {

        Debug.LogWarning("U die");
    }
}
