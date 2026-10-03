using UnityEngine;

public class HurtareaInstant : MonoBehaviour
{
    [SerializeField] private float hurtAmt = 20f;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        var hb = other.GetComponent<PlayerHitboxMgmt>();
        PointSystem.Instance.EnterDanger();

        if (hb.iframeTimer <= 0)
        {
            PointSystem.Instance.DeductPoints(hurtAmt);
            hb.OnHit();
        }

    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        PointSystem.Instance.ExitDanger();
    }
}
