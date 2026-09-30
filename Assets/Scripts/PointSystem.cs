using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

public class PointSystem : MonoBehaviour
{
    [SerializeField] private Text scoreText;
    private float initMaxPoint = 100;
    [SerializeField] private float[] thresholds = { 20f, 40f, 60f, 80f };
    [SerializeField, ReadOnlyInspector] private float _points;

    public static PointSystem Instance;
    public float points
    {
        get => _points;
        private set
        {
            _points = value;

            if (_points > initMaxPoint)
            {
                _points = initMaxPoint;
            }
            else if (_points <= 0)
            {
                _points = 0;
                OnDeath();
            }

            if (scoreText != null)
            {
                scoreText.text = "points: " + _points.ToString();
                Debug.Log(scoreText.text);
            }
        }
    }

    void Awake()
    {
        Instance = this;
        scoreText = GetComponent<Text>();
    }

    void Start()
    {
        points = initMaxPoint;
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.fKey.isPressed)
        {
            DeductPoints(10f);
        }
    }

    public void DeductPoints(float rate)
    /**
        rate is points per second!!
    */
    {
        points -= rate * Time.deltaTime;
    }

    private void OnDeath()
    {
        Debug.LogWarning("U die");
    }
}
