using UnityEngine;
using UnityEngine.InputSystem;
using System.ComponentModel;

public class SimpleController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 1.0f;
    [SerializeField] private float jumpHeight = .4f;
    [SerializeField] private float jumpBufferTime = 0.1f;
    public float gravity = 9.81f;
    private float jumpSpeed; // only calced when body loaded
    private float xVel = 0f;
    private float yVel = 0f;
    [SerializeField][ReadOnly(true)] private float jumpBuf = 0;

    private Vector2 inputVector = Vector2.zero;
    private CharacterController controller;

    void Start()
    {
        Debug.Log("owo");
        controller = GetComponent<CharacterController>();

    }

    void Update()
    {
        // put this back to start() after tweaking
        jumpSpeed = Mathf.Sqrt(2f * gravity * jumpHeight);
        WalkCheck();
        JumpCheck();
        MoveAndSlide();
    }

    private void WalkCheck()
    {
        inputVector.x = 0f;
        if (Keyboard.current != null)
        {

            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) inputVector.x = -1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) inputVector.x = 1f;
        }
    }

    private void JumpCheck()
    {
        //check for input every frame, set buffer if it's not set
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame && jumpBuf == 0)
        {
            jumpBuf = jumpBufferTime;
        }
        if (controller.isGrounded)
        {
            // if jump buffer ongoing, jump
            yVel = -1f;
            if (jumpBuf > 0)
            {
                jumpBuf = 0;
                yVel = jumpSpeed;
            }
        }
        else
        {
            yVel -= gravity * Time.deltaTime;
        }
        // tickdown bufer
        if (jumpBuf > 0) { jumpBuf -= Time.deltaTime; }
    }

    private void MoveAndSlide()
    {
        Vector3 moveDirection = new Vector3(inputVector.x * moveSpeed, yVel, 0f);
        controller.Move(moveDirection * Time.deltaTime);
    }
}