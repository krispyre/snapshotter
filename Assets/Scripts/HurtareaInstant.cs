using UnityEngine;

public class HurtareaInstant : MonoBehaviour
{
    [SerializeField] private float hurtAmt = 20f;
    private bool playerIsInside = false;
    private bool playerWasInside = false;

    private MeshCollider collider;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PointSystem.Instance.DeductPoints(hurtAmt);
            // PlayerHitboxMgmt.OnHit();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerWasInside = playerIsInside;
            playerIsInside = false;
        }
    }

    void Awake()
    {
        collider = GetComponent<MeshCollider>();
    }
}
