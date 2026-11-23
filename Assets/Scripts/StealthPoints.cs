using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class StealthPoints : MonoBehaviour
{
    public float initMax = 100f;
    [SerializeField] private float curMax = 100f;
    [SerializeField] private float recoverRate = 0.5f;
    public float[] thresholds;
    [SerializeField, ReadOnlyInspector] private int danger;
    [SerializeField, ReadOnlyInspector] private float _stealthPoints = 100f;

    [SerializeField] private GameStates gameStates;
    public UnityEvent onStealthPointsDepleted;
    public event Action onChanged;

    public static StealthPoints Instance;

    public float stealthPoints
    {
        get => _stealthPoints;
        private set
        {
            bool dropped = value < _stealthPoints;

            if (dropped) LowerMaxStealthPoints();

            if (gameStates != null)
                gameStates.SetStealth(value);
            onChanged?.Invoke();
            if (_stealthPoints > 0 && value <= 0)
                OnDepleted();

            _stealthPoints = value;
            _stealthPoints = Mathf.Clamp(_stealthPoints, 0, curMax);
        }
    }

    void Awake()
    {
        Instance = this;
    }

    void OnEnable()
    {
        // SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    void OnDisable()
    {
        // SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    public void SetThresholds(float[] thresholds)
    {
        this.thresholds = thresholds;
        onChanged?.Invoke();
    }

    public void LoadMission()
    {
        curMax = initMax;
        _stealthPoints = initMax;
        // the drop from full lowers curMax to match the stored stealth points
        stealthPoints = gameStates.Stealth;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void LowerMaxStealthPoints()
    {
        if (thresholds == null) return;

        // thresholds must be in ascending order
        foreach (float t in thresholds)
        {
            if (t >= curMax) return;
            if (_stealthPoints <= t)
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
            DeductStealthPoints(10f);
        }

        if (danger <= 0)
            stealthPoints += recoverRate * Time.deltaTime;
    }

    public void DeductStealthPoints(float amt)
    {
        stealthPoints -= amt;
    }

    private void OnDepleted()
    {
        onStealthPointsDepleted.Invoke();
        var hitbox = GetComponentInChildren<PlayerHitboxMgmt>();
        if (hitbox != null) hitbox.Disable();
    }
}
