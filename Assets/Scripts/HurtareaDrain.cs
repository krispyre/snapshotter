using UnityEngine;

public class HurtareaDrain : MonoBehaviour
{
    [SerializeField] protected float hurtRate = 20f;
    [SerializeField, ReadOnlyInspector] protected bool playerInside;
    protected PointSystem points;

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        points = other.GetComponentInParent<PointSystem>();
        if (points == null) return;
        playerInside = true;
        points.EnterDanger();
    }

    protected virtual void OnTriggerExit(Collider other)
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
