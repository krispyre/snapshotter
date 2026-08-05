using UnityEngine;
using UnityEngine.InputSystem;
using System.ComponentModel;

public class SimpleController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 1.0f;
    [SerializeField] private float jumpHeight = .4f;
    [SerializeField] private float jumpBufferTime = 0.1f;
    [SerializeField] private float coyoteTime = 0.5f;
    public float gravity = 9.81f;
    private float jumpSpeed; // only calced when body loaded
    private float xVel = 0f;
    private float yVel = 0f;
    [SerializeField][ReadOnly(true)] private float jumpBuf = 0;
    [SerializeField][ReadOnly(true)] private float coyoteTimer = 0;

    private Vector2 inputVector = Vector2.zero;
    private CharacterController controller;
    [SerializeField] private PlayerControl inputActions;

    private void OnEnable()
    {
        inputActions.Enable();
    }

    private void OnDisable()
    {
        inputActions.Disable();
    }

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        inputActions = new PlayerControl();
    }

    void Update()
    {
        // put this back to start() after tweaking
        jumpSpeed = Mathf.Sqrt(2f * gravity * jumpHeight);

        float dirX = inputActions.Player.DirX.ReadValue<float>();
        float dirY = inputActions.Player.DirY.ReadValue<float>();
        bool jump = inputActions.Player.Jump.WasPressedThisFrame();
        WalkCheck(dirX);
        JumpCheck(jump);
        MoveAndSlide();
    }

    private void WalkCheck(float dirX)
    {
        inputVector.x = 0f;
        if (Keyboard.current != null)
        {

            if (dirX < 0) inputVector.x = -1f;
            if (dirX > 0) inputVector.x = 1f;
        }
    }

    private void JumpCheck(bool jumpBtn)
    {
        if (jumpBtn && jumpBuf <= 0)
        {
            jumpBuf = jumpBufferTime;
        }

        if (controller.isGrounded)
        {
            coyoteTimer = coyoteTime;
            yVel = -1f; // press player on the ground

            if (jumpBuf > 0)
            {
                ExecuteJump();
            }
        }
        else
        {
            if (coyoteTimer > 0 && jumpBuf > 0)
            {
                ExecuteJump();
            }

            coyoteTimer = Mathf.Max(0f, coyoteTimer - Time.deltaTime);
            yVel -= gravity * Time.deltaTime;
        }

        jumpBuf = Mathf.Max(0f, jumpBuf - Time.deltaTime);
    }

    private void ExecuteJump()
    {
        jumpBuf = 0f;
        // prevent double jump and consume it
        coyoteTimer = 0f;
        yVel = jumpSpeed;
    }
    private void MoveAndSlide()
    {
        Vector3 moveDirection = new Vector3(inputVector.x * moveSpeed, yVel, 0f);
        controller.Move(moveDirection * Time.deltaTime);
    }
}