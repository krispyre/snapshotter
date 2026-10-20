using UnityEngine;

public class OneWayTeleport : MonoBehaviour
{
    Transform dest;
    void Start()
    {
        dest = transform.Find("dest");
    }

    void OnTriggerEnter(Collider other)
    {
        // need the player itself to touch, not interact
        if (other.CompareTag("Player") && other.TryGetComponent<CharacterController>(out CharacterController cc))
        {
            cc.enabled = false;
            other.transform.position = dest.transform.position;
            cc.enabled = true;
        }
    }
}
