using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Animator))]
public class BlackScreenFlies : MonoBehaviour
{
    private Animator animator;

    void Start()
    {
    animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            animator.SetTrigger("Screentri");
        }
    }
}
