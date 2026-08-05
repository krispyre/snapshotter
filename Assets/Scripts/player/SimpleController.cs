using UnityEngine;
using UnityEngine.InputSystem;
using System.ComponentModel;

public class SimpleController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 1.0f;
    [SerializeField] private float jumpHeight = .4f;
    [SerializeField] private float jumpBufferTime = 0.1f;
    [SerializeField] private float coyoteTime = 0.5f;
    [SerializeField] private float apexGravityMult = 0.35f;
    [SerializeField] private float apexThreshold = 0.5f; // start reducing gravity when yVel within [0~this].

    public float jumpGravity = 7f;
    public float fallGravity = 10f;
    private float jumpSpeed; // only calced when body loaded
    private float xVel = 0f;
    private float yVel = 0f;
    [SerializeField][ReadOnly(true)] private float jumpBuf = 0;
    [SerializeField][ReadOnly(true)] private float coyoteTimer = 0;

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
        jumpSpeed = Mathf.Sqrt(2f * jumpGravity * jumpHeight);

        float dirX = inputActions.Player.DirX.ReadValue<float>();
        float dirY = inputActions.Player.DirY.ReadValue<float>();

        WalkCheck(dirX);
        JumpCheck();
        MoveAndSlide(dirX);
    }

    private void WalkCheck(float dirX)
    {
        ;

    }

    private void JumpCheck()
    {
        bool jumpPressed = inputActions.Player.Jump.WasPressedThisFrame();
        bool jumpHeld = inputActions.Player.Jump.IsPressed();
        bool jumpReleased = inputActions.Player.Jump.WasReleasedThisFrame();

        float curGravity = yVel <= 0 ? fallGravity : jumpGravity;

        if (jumpPressed && jumpBuf <= 0)
        {
            jumpBuf = jumpBufferTime;
        }
        if (!jumpHeld && yVel > 0)
        {
            yVel = 0;
        }// APEX HANG TIME: Reduce gravity when near the top of the jump
        else if (Mathf.Abs(yVel) < apexThreshold)
        {
            curGravity *= apexGravityMult;
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
            yVel -= curGravity * Time.deltaTime;
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
    private void MoveAndSlide(float dirX)
    {
        Vector3 moveDirection = new Vector3(dirX * moveSpeed, yVel, 0f);
        controller.Move(moveDirection * Time.deltaTime);
    }
}