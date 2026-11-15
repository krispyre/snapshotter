using System.Collections.Generic;
using UnityEngine;

public class PointsBar : MonoBehaviour
{
    [SerializeField] private PointSystem source;
    [SerializeField] private RectTransform fill;
    [SerializeField] private GameObject thresholdMark;

    private readonly List<GameObject> marks = new List<GameObject>();
    private float[] drawnThresholds;

    void Awake()
    {
        if (source == null)
            source = GetComponentInParent<PointSystem>();
        if (fill == null)
            fill = transform.Find("curPoints") as RectTransform;
    }

    void OnEnable()
    {
        if (source == null) return;
        source.onChanged += Refresh;
        Refresh();
    }

    void OnDisable()
    {
        if (source == null) return;
        source.onChanged -= Refresh;
    }

    private void Refresh()
    {
        if (fill != null && source.initMax > 0f)
        {
            float width = ((RectTransform)transform).rect.width;
            float loss = (source.points - source.initMax) / source.initMax * width;
            Vector2 offsetMax = fill.offsetMax;
            offsetMax.x = loss;
            fill.offsetMax = offsetMax;
        }

        if (source.thresholds == drawnThresholds) return;
        drawnThresholds = source.thresholds;
        RebuildMarks();
    }

    private void RebuildMarks()
    {
        for (int i = 0; i < marks.Count; i++)
        {
            if (marks[i] != null) Destroy(marks[i]);
        }
        marks.Clear();

        if (source.thresholds == null || thresholdMark == null) return;

        float width = ((RectTransform)transform).rect.width;
        foreach (float t in source.thresholds)
        {
            float x = t / source.initMax * width;
            GameObject mark = Instantiate(thresholdMark, transform);
            ((RectTransform)mark.transform).anchoredPosition = new Vector2(x, 0);
            marks.Add(mark);
        }
    }
}
