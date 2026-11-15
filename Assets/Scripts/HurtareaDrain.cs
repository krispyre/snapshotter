using UnityEngine;

public class HurtareaDrain : MonoBehaviour
{
    [SerializeField] protected float hurtRate = 20f;
    [SerializeField, ReadOnlyInspector] protected bool playerInside;
    protected StealthPoints stealthPoints;

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        stealthPoints = other.GetComponentInParent<StealthPoints>();
        if (stealthPoints == null) return;
        playerInside = true;
        stealthPoints.EnterDanger();
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerInside = false;
        if (stealthPoints != null) stealthPoints.ExitDanger();
    }

    void FixedUpdate()
    {
        // iframes are ignored
        if (playerInside && stealthPoints != null)
            stealthPoints.DeductStealthPoints(hurtRate * Time.deltaTime);
    }
}
