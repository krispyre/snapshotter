using UnityEngine;

public class HurtareaInstant : MonoBehaviour
{
    [SerializeField] private float hurtAmt = 20f;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        var hb = other.GetComponent<PlayerHitboxMgmt>();
        var stealthPoints = other.GetComponentInParent<StealthPoints>();
        if (stealthPoints == null) return;
        stealthPoints.EnterDanger();

        if (hb.iframeTimer <= 0)
        {
            stealthPoints.DeductStealthPoints(hurtAmt);
            hb.OnHit();
        }

    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        var stealthPoints = other.GetComponentInParent<StealthPoints>();
        if (stealthPoints != null) stealthPoints.ExitDanger();
    }
}
