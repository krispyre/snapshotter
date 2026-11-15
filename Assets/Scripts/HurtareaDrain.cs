using UnityEngine;

public class Hurtarea : MonoBehaviour
{
    [SerializeField] private float hurtRate = 20f;
    [SerializeField, ReadOnlyInspector] private bool playerInside;
    private PointSystem points;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        points = other.GetComponentInParent<PointSystem>();
        if (points == null) return;
        playerInside = true;
        points.EnterDanger();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerInside = false;
        if (points != null) points.ExitDanger();
    }

    void FixedUpdate()
    {
        // iframes are ignored
        if (playerInside && points != null)
            points.DeductPoints(hurtRate * Time.deltaTime);
    }
}
