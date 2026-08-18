using UnityEngine;

public class ClawRange : MonoBehaviour
{
    [SerializeField] private ClawParams data;

    private void OnValidate()
    {
        ApplyScale();
    }
    void Start()
    {
        ApplyScale();
    }

    private void Update()
    {
        // Optional: Keep updated if changed via code at runtime
        if (Application.isPlaying)
        {
            ApplyScale();
        }
    }

    private void ApplyScale()
    {
        if (data != null)
        {
            transform.localScale = Vector3.one * data.armLength * 2;
        }
    }
}