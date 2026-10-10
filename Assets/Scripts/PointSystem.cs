using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using UnityEngine.UI;

public class PointSystem : MonoBehaviour
{
    public UnityEvent onPointsDepleted;
    private float initMaxPoint = 100;
    private float curMaxPoint = 100;
    private float pointsRecoverRate = 5;
    [SerializeField]
    public float[] thresholds = null; //= { 20f, 40f, 60f, 80f, 100f };
    [SerializeField, ReadOnlyInspector] private float _points;

    public static PointSystem Instance;
    [SerializeField, ReadOnlyInspector] private int danger;

    private RectTransform bg;
    private RectTransform fg;
    [SerializeField] private GameObject thresBar;
    public float points
    {
        get => _points;
        private set
        {
            _points = value;
            ApplyLoss();
            if (_points > curMaxPoint)
            {
                _points = curMaxPoint;
            }
            else if (_points <= 0)
            {
                _points = 0;
                OnDepleted();
            }


        }
    }

    private void ApplyLoss()
    {
        if (fg == null || initMaxPoint <= 0f) return;

        float width = ((RectTransform)transform).rect.width;
        float loss = (points - initMaxPoint) / initMaxPoint * width; // why is it the other way around/???
        Vector2 newOffsetMax = fg.offsetMax;
        newOffsetMax.x = loss;
        fg.offsetMax = newOffsetMax;
    }

    public void SetThresholds(float[] thresholds)
    {
        this.thresholds = thresholds;

        foreach (float t in thresholds)
        {
            float x = t / initMaxPoint * ((RectTransform)transform).rect.width;
            GameObject tObj = Instantiate(thresBar, transform);
            RectTransform tRect = (RectTransform)tObj.transform;

            tRect.anchoredPosition = new Vector2(x, 0);
        }
    }

    void Awake()
    {
        Instance = this;
        bg = GetComponent<RectTransform>();
        fg = transform.Find("curPoints").GetComponent<RectTransform>();
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
