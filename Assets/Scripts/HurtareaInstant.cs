using UnityEngine;

public class HurtareaInstant : MonoBehaviour
{
    [SerializeField] private float hurtAmt = 20f;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        var hb = other.GetComponent<PlayerHitboxMgmt>();
        var points = other.GetComponentInParent<PointSystem>();
        if (points == null) return;
        points.EnterDanger();

        if (hb.iframeTimer <= 0)
        {
            points.DeductPoints(hurtAmt);
            hb.OnHit();
        }

    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        var points = other.GetComponentInParent<PointSystem>();
        if (points != null) points.ExitDanger();
    }
}
