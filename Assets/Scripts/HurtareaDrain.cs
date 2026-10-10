using UnityEngine;

public class Hurtarea : MonoBehaviour
{
    [SerializeField] private float hurtRate = 20f;
    [SerializeField, ReadOnlyInspector] private bool playerInside;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerInside = true;
        PointSystem.Instance.EnterDanger();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerInside = false;
        PointSystem.Instance.ExitDanger();
    }

    void FixedUpdate()
    {
        // iframes are ignored
        if (playerInside)
        {
            PointSystem.Instance.DeductPoints(hurtRate * Time.deltaTime);
        }
    }
}
