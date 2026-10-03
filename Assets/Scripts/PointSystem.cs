using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using UnityEngine.UI;

public class PointSystem : MonoBehaviour
{
    public UnityEvent onPointsDepleted;
    private Text scoreText;
    private float initMaxPoint = 100;
    private float curMaxPoint = 100;
    private float pointsRecoverRate = 5;
    [SerializeField]
    public float[] thresholds = null; //= { 20f, 40f, 60f, 80f, 100f };
    [SerializeField, ReadOnlyInspector] private float _points;

    public static PointSystem Instance;
    [SerializeField, ReadOnlyInspector] private int danger;
    public float points
    {
        get => _points;
        private set
        {
            _points = value;

            if (_points > curMaxPoint)
            {
                _points = curMaxPoint;
            }
            else if (_points <= 0)
            {
                _points = 0;
                OnDepleted();
            }

            if (scoreText != null)
            {
                scoreText.text = "points: " + _points.ToString();
            }
        }
    }

    public void SetThresholds(float[] thresholds)
    {
        this.thresholds = thresholds;
    }

    void Awake()
    {
        Instance = this;
        scoreText = GetComponent<Text>();
    }

    void Start()
    {
        curMaxPoint = initMaxPoint;
        points = curMaxPoint;
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
            DeductPoints(50f);
        }

        if (danger <= 0)
        {
            points += pointsRecoverRate * Time.deltaTime;
            points = Mathf.Clamp(points, 0, curMaxPoint);
        }
    }

    public void DeductPoints(float amt)
    /**
        rate is points per second!!
    */
    {
        points -= amt;
    }

    void LateUpdate()
    {
        if (danger >= 0 && thresholds != null)
        {
            for (int i = 0, n = thresholds.Length; i < n; i++)
            {
                if (_points <= thresholds[i])
                {
                    curMaxPoint = thresholds[i];
                    Debug.Log(curMaxPoint);
                    break;
                }
            }
        }
    }

    private void OnDepleted()
    {
        onPointsDepleted.Invoke();
    }
}
