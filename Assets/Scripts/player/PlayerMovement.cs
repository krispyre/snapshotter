using UnityEngine;
using UnityEngine.InputSystem;
using System.ComponentModel;
using System;

public class SimpleController : MonoBehaviour
{
    // params
    [SerializeField] private float moveSpeed = 1.2f;
    [SerializeField] private float jumpHeight = .5f;
    [SerializeField] private float jumpBufferTime = 0.1f;
    [SerializeField] private float coyoteTime = 0.1f;
    [SerializeField] private float apexGravityMult = 0.4f;
    [SerializeField] private float apexThreshold = 0.5f; // start reducing gravity when yVel within [-this ~ this].
    [SerializeField] private float wallSlideGravity = 4f;
    [SerializeField] private float wallSlideEnterDampMult = 0.2f;
    [SerializeField] private float terminalWallSlideSpeed = 5f;

    [SerializeField] private float wallJumpKickSpeed = 5f;
    [SerializeField] private float terminalSpeed = 50f;

    //jump params
    public float jumpGravity = 55F;
    public float fallGravity = 18f;
    private float jumpSpeed; // only calced when body loaded

    // jump assist vars
    [SerializeField][ReadOnlyInspector] private float jumpBuf = 0;
    [SerializeField][ReadOnlyInspector] private float coyoteTimer = 0;

    // movement vars
    private CharacterController controller;
    [SerializeField] private Transform wallCheckL;
    [SerializeField] private Transform wallCheckR;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField][ReadOnlyInspector] private bool isTouchingWall;
    [SerializeField][ReadOnlyInspector] private bool isEnteringWall = true;

    [SerializeField][ReadOnlyInspector] private bool isRight = true;
    [SerializeField][ReadOnlyInspector] private float curGravity;

    enum PlayerState { Walk, Idle, WallJump, Jump, WallSlide, WallCling, Fall };
    [SerializeField][ReadOnlyInspector] private PlayerState state = PlayerState.Idle;

    // input controllers
    [SerializeField] private PlayerInput playerInput;
    private InputAction dirXAction;
    private InputAction dirYAction;
    private InputAction jumpAction;
    [SerializeField][ReadOnlyInspector] private float xVel = 0f;
    [SerializeField][ReadOnlyInspector] private float yVel = 0f;



    void Awake()
    {
        controller = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();
        CacheActions();
    }

    private void OnEnable()
    {
        CacheActions();
    }

    private void CacheActions()
    {
        if (playerInput != null && playerInput.actions != null)
        {
            dirXAction = playerInput.actions.FindAction("DirX");
            dirYAction = playerInput.actions.FindAction("DirY");
            jumpAction = playerInput.actions.FindAction("Jump");

        }
    }

    void Start()
    {
        ;
    }

    void Update()
    {
        if (dirXAction == null || dirYAction == null)
        {
            CacheActions();
            if (dirXAction == null || dirYAction == null) return;
        }

        // todo put these back to start() after tweaking
        jumpSpeed = Mathf.Sqrt(2f * jumpGravity * jumpHeight);
        isTouchingWall = (Physics.OverlapSphere(wallCheckL.transform.position, 0.02f, wallLayer).Length > 0) ||
        (Physics.OverlapSphere(wallCheckR.transform.position, 0.02f, wallLayer).Length > 0);

        float dirX = dirXAction.ReadValue<float>();
        float dirY = dirYAction.ReadValue<float>();

        // todo idle if not moving
        if (xVel == 0 && controller.isGrounded) state = PlayerState.Idle;
        WalkCheck(dirX);
        WallSlide(dirX);
        JumpCheck();
        FallCheck();
        MoveAndSlide(dirX);
        DebugTime(true);

    }
    private void DebugTime(bool isdebug)
    {
        if (isdebug)
        {
            if (Keyboard.current != null && Keyboard.current.tKey.wasPressedThisFrame)
            {
                if (Time.timeScale != 1)
                {
                    Time.timeScale = 1f;
                    Debug.Log("normal time");
                }
                else
                {
                    Time.timeScale = 0.25f;
                    Debug.Log("slow time");

                }
            }
        }
    }

    private void WalkCheck(float dirX)
    {
        if (dirX > 0)
        {
            isRight = true;
        }
        else if (dirX < 0)
        {
            isRight = false;
        }
        if (dirX != 0) { state = PlayerState.Walk; }
        xVel = dirX * moveSpeed;

    }
    private void JumpCheck()
    {
        bool jumpPressed = jumpAction.WasPressedThisFrame();
        bool jumpHeld = jumpAction.IsPressed();


        curGravity = jumpGravity;
        if (jumpPressed && jumpBuf <= 0)
        {
            jumpBuf = jumpBufferTime;
        }
        // drop early if button released
        if (!jumpHeld && yVel > 0)
        {
            yVel *= .4f;
        }
        // Reduce gravity when near the top of the jump
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

        }
        // countdown jumpbuffer
        jumpBuf = Mathf.Max(0f, jumpBuf - Time.deltaTime);
    }
    private void WallSlide(float dirX)
    {
        if (!controller.isGrounded && isTouchingWall && yVel < 0)
        {
            state = PlayerState.WallSlide;
            if (isEnteringWall)
            {
                // slow down when enter wall, 
                yVel = Mathf.Max(-terminalWallSlideSpeed, yVel * wallSlideEnterDampMult);
                isEnteringWall = false;
            }
            curGravity = wallSlideGravity;
            WallCling(dirX);
            WallJump(dirX);
        }
        else
        {
            isEnteringWall = true;
        }

    }
    private void ExecuteJump()
    {
        state = PlayerState.Jump;
        curGravity = jumpGravity;
        jumpBuf = 0f;
        // prevent double jump and consume it
        coyoteTimer = 0f;
        yVel = jumpSpeed;
    }
    private void FallCheck()
    {
        if (!controller.isGrounded && yVel <= 0)
        {
            state = PlayerState.Fall;
            curGravity = fallGravity;
        }
    }
    private void WallJump(float dirX)
    {
        bool jumpPressed = jumpAction.WasPressedThisFrame();
        bool jumpHeld = jumpAction.IsPressed();
        // vary jump dist if holding?
        if ((state == PlayerState.WallSlide || state == PlayerState.WallCling) && jumpPressed)
        {
            state = PlayerState.WallJump;
            xVel += wallJumpKickSpeed * dirX;
        }
    }

    private void WallCling(float dirX)
    {
        if (!controller.isGrounded && isTouchingWall && dirX != 0)
        {
            state = PlayerState.WallCling;
            curGravity = 0f;
            yVel = 0f;
        }
    }

    private void MoveAndSlide(float dirX)
    {
        yVel -= curGravity * Time.deltaTime;
        if (state == PlayerState.WallSlide) yVel = Mathf.Max(yVel, -terminalWallSlideSpeed);
        else yVel = Mathf.Max(yVel, -terminalSpeed);

        Vector3 moveDirection = new Vector3(xVel, yVel, 0f);
        controller.Move(moveDirection * Time.deltaTime);
    }
}