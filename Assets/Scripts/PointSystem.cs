using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using UnityEngine.UI;

public class PointSystem : MonoBehaviour
{
    public UnityEvent onPointsDepleted;
    private float initMaxPoint = 100;
    private float curMaxPoint = 100;
    [SerializeField] private float pointsRecoverRate = 0.5f;

    public float[] thresholds = null; //= { 20f, 40f, 60f, 80f, 100f };
    [SerializeField, ReadOnlyInspector] private float _points;

    public static PointSystem Instance;
    [SerializeField, ReadOnlyInspector] private int danger;
    private MissionSave mission;


    private RectTransform bg;
    private RectTransform fg;
    [SerializeField] private GameObject thresBar;
    public float points
    {
        get => _points;
        private set
        {
            bool dropped = value < _points;
            _points = value;
            if (dropped) LowerMaxPoint();
            UpdateDisplay();

            _points = Mathf.Clamp(_points, 0, curMaxPoint);
            if (_points <= 0)
            {
                OnDepleted();
            }


        }
    }

    private void UpdateDisplay()
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

    public void LoadMission(int missionId)
    {
        mission = SaveSystem.GetMission(missionId);
        curMaxPoint = initMaxPoint;
        _points = initMaxPoint;
        // the drop from full lowers curMaxPoint to match the saved points
        points = mission.points;
    }

    void OnApplicationPause(bool paused)
    {
        if (paused) SavePoints();
    }

    void OnDestroy()
    {
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
            if (t >= curMaxPoint) return;
            if (_points <= t)
            {
                curMaxPoint = t;
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

    private void OnDepleted()
    {
        onPointsDepleted.Invoke();
    }
}
