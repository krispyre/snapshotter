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
        player.GetComponent<PlayerObscuredCheck>().OnObscuredChange += OnPlayerObscuredChange;
    }
    protected override void OnTriggerExit(Collider other)
    {
        base.OnTriggerExit(other);
        player.GetComponent<PlayerObscuredCheck>().OnObscuredChange -= OnPlayerObscuredChange;
        player = null;
    }

    void OnPlayerObscuredChange(bool isObscured)
    {
        if (isObscured)
        {
            stealthPoints.ExitDanger();
        }
        else
        {
            stealthPoints.EnterDanger();
        }
    }

    void FixedUpdate()
    {
        // deduct when NOT obscured lol
        if (playerInside)
        {
            if (!player.GetComponent<PlayerObscuredCheck>().isObscured)
            {
                stealthPoints.DeductStealthPoints(hurtRate * Time.deltaTime);
            }
            else
            {

            }
        }
    }
}
