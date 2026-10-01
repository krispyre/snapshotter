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
    [SerializeField, ReadOnlyInspector] public bool isSafe = true;
    private bool wasSafe = false;
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

    void Update()
    {
        if (thresholds == null) return;
        if (Keyboard.current != null && Keyboard.current.fKey.isPressed)
        {
            Debug.Log("deduct debug");
            DeductPoints(50f);
        }
        else isSafe = true;

        if (isSafe)
        {
            points += pointsRecoverRate * Time.deltaTime;
            points = Mathf.Clamp(points, 0, curMaxPoint);
        }

    }

    public void DeductPoints(float rate)
    /**
        rate is points per second!!
    */
    {
        isSafe = false;
        points -= rate * Time.deltaTime;
    }

    void LateUpdate()
    {

        if (isSafe && !wasSafe) // just returned to safe
        { // set curMaxPoint to nearest point thres
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
        wasSafe = isSafe;
        isSafe = true;
    }

    private void OnDepleted()
    {
        onPointsDepleted.Invoke();
    }
}
