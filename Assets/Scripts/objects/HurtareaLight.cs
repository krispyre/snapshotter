using UnityEngine;

public class HurtareaLight : HurtareaDrain
{

    [SerializeField] private GameObject player; //cached to do raytracing

    // drains but only if you're in the light
    //only deduct if ray from obj to camera cant reach
    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);
        player = other.gameObject;
    }
    protected override void OnTriggerExit(Collider other)
    {
        base.OnTriggerExit(other);
        player = null;
    }

    void FixedUpdate()
    {
        // deduct when NOT obscured lol
        if (player == null) return;
        if (playerInside && !player.GetComponent<ObscuredCheck>().isObscured)
            stealthPoints.DeductStealthPoints(hurtRate * Time.deltaTime);

    }
}
