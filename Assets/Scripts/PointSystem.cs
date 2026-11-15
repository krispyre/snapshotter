using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PointSystem : MonoBehaviour
{
    public float initMax = 100f;
    [SerializeField] private float curMax = 100f;
    [SerializeField] private float recoverRate = 0.5f;
    public float[] thresholds;
    [SerializeField, ReadOnlyInspector] private int danger;
    [SerializeField, ReadOnlyInspector] private float _points = 100f;

    [SerializeField] private GameStates gameStates;
    public UnityEvent onPointsDepleted;
    public event Action onChanged;

    public static PointSystem Instance;
    private MissionSave mission;

    public float points
    {
        get => _points;
        private set
        {
            bool dropped = value < _points;
            _points = value;
            if (dropped) LowerMaxPoint();

            _points = Mathf.Clamp(_points, 0, curMax);
            onChanged?.Invoke();
            if (_points <= 0)
                OnDepleted();
        }
    }

    void Awake()
    {
        Instance = this;
        GameStateSync();
    }

    void OnEnable()
    {
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    void OnDisable()
    {
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    public void SetThresholds(float[] thresholds)
    {
        this.thresholds = thresholds;
        onChanged?.Invoke();
    }

    public void LoadMission(int missionId)
    {
        mission = SaveSystem.GetMission(missionId);
        curMax = initMax;
        _points = initMax;
        // the drop from full lowers curMax to match the saved points
        points = mission.points;
    }

    void OnApplicationPause(bool paused)
    {
        if (paused) SavePoints();
    }

    void OnApplicationQuit()
    {
        SavePoints();
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
        SavePoints();
    }

    private void SavePoints()
    {
        if (mission == null) return;
        mission.points = _points;
        SaveSystem.Save();
    }

    private void LowerMaxPoint()
    {
        if (thresholds == null) return;

        // thresholds must be in ascending order
        foreach (float t in thresholds)
        {
            if (t >= curMax) return;
            if (_points <= t)
            {
                curMax = t;
                return;
            }
        }
    }

    public void EnterDanger()
    {
        danger++;
    }

    public void ExitDanger()
    {
        if (danger > 0) danger--;
    }

    void Update()
    {
        if (thresholds == null) return;
        if (Keyboard.current != null && Keyboard.current.fKey.isPressed)
        {
            Debug.Log("deduct debug");
            DeductPoints(10f);
        }

        if (danger <= 0)
            points += recoverRate * Time.deltaTime;
    }

    public void DeductPoints(float amt)
    {
        points -= amt;
        GameStateSync();

    }

    private void OnDepleted()
    {
        onPointsDepleted.Invoke();
    }

    void GameStateSync()
    {
        gameStates.SetStealth(points);
    }
}
