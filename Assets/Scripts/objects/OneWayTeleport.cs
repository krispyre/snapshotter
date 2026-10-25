using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public enum StairWipeDir
{
    FromRight,
    // FromLeft, FromUp, FromDown — add when clips exist
}

public class OneWayTeleport : MonoBehaviour
{
    Transform dest;

    [SerializeField] Animator stairAnimator;
    [SerializeField] StairWipeDir enterDir = StairWipeDir.FromRight;
    [SerializeField] StairWipeDir exitDir = StairWipeDir.FromRight;

    const string EnterFromRightState = "stairTransition_enter";
    const string ExitFromRightState = "stairTransition_exitFromRight";
    const string TeleportedTrigger = "playerTeleported";

    static bool busy;
    static bool animatorInitialized;

    void Start()
    {
        dest = transform.Find("dest");
        Debug.LogAssertion(stairAnimator != null);

        if (stairAnimator != null && !animatorInitialized)
        {
            // Start fully off-screen (end of exit).
            stairAnimator.Play(ExitFromRightState, 0, 1f);
            stairAnimator.Update(0f);
            animatorInitialized = true;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (busy)
            return;

        if (!(other.CompareTag("Player") && other.TryGetComponent(out CharacterController cc)))
            return;

        StartCoroutine(RunTransition(other.transform, cc));
    }

    IEnumerator RunTransition(Transform player, CharacterController cc)
    {
        busy = true;

        PlayerInput playerInput = player.GetComponentInParent<PlayerInput>();
        if (playerInput != null)
            playerInput.actions.FindActionMap("Player").Disable();

        // only right is allowed rn
        stairAnimator.Play(EnterFromRightState, 0, 0f);
        float length = stairAnimator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(length);

        cc.enabled = false;
        player.position = dest.position;
        cc.enabled = true;

        stairAnimator.SetTrigger("playerTeleported");

        length = stairAnimator.GetCurrentAnimatorStateInfo(0).length;
        float normalized = stairAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime;
        float remaining = length * (1f - normalized);
        yield return new WaitForSeconds(remaining);

        if (playerInput != null)
            playerInput.actions.FindActionMap("Player").Enable();

        busy = false;
    }
}
