using UnityEngine;
using UnityEngine.InputSystem;
using System.ComponentModel;

public class SimpleController : MonoBehaviour
{
    const bool IS_DEBUG=true;
    [Header("move params")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float terminalSpeed = 50f;

    [Header("jump params")]
    [SerializeField] private float jumpHeight = 0.5f;
    public float jumpGravity = 55f;
    public float fallGravity = 18f;
    [SerializeField] private float jumpBufferTime = 0.1f;
    [SerializeField] private float coyoteTime = 0.1f;
    [SerializeField] private float apexGravityMult = 0.4f;
    [SerializeField] private float apexThreshold = 0.5f; // start reducing gravity when yVel within [-this ~ this].
    [SerializeField] private float wallSlideGravity = 4f;
    [SerializeField] private float wallSlideEnterDampMult = 0.2f;
    [SerializeField] private float terminalWallSlideSpeed = 5f;
    [SerializeField] private float wallJumpKickSpeed = 5f;
    
    [SerializeField] private float terminalFallSpeed = 50f;
    [SerializeField] private Transform wallCheckL;
    [SerializeField] private Transform wallCheckR;
    [SerializeField] private LayerMask wallLayer;

    [Header("debug")]
    [SerializeField, ReadOnlyInspector] private PlayerState state = PlayerState.Idle;
    [SerializeField, ReadOnlyInspector] private float xVel = 0f;
    [SerializeField, ReadOnlyInspector] private float yVel = 0f;
    [SerializeField, ReadOnlyInspector] private bool isTouchingWall;
    [SerializeField, ReadOnlyInspector] private int wallDirection; // -1 for left, 1 for right
    [SerializeField, ReadOnlyInspector] private bool isRight = true;
    [SerializeField, ReadOnlyInspector] private float curGravity;

    private CharacterController controller;
    private PlayerInput playerInput;
    private InputAction dirXAction;
    private InputAction dirYAction;
    private InputAction jumpAction;

    //movement vars
    private float jumpSpeed;
    private float jumpBuf;
    private float coyoteTimer;
    
    private bool wasTouchingWall;

    private enum PlayerState { Idle, Walk, Jump, Fall, WallSlide, WallCling, WallJump }

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();
        CacheActions();
    }

    private void OnEnable() => CacheActions();

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
        if (dirXAction == null || dirYAction == null || jumpAction == null)
        {
            CacheActions();
            if (dirXAction == null || dirYAction == null) return;
        }

        // todo put these back to start() after tweaking
        jumpSpeed = Mathf.Sqrt(2f * jumpGravity * jumpHeight);

        float dirX = dirXAction.ReadValue<float>();
        float dirY = dirYAction.ReadValue<float>();
        bool jumpPressed = jumpAction.WasPressedThisFrame();
        bool jumpHeld = jumpAction.IsPressed();

        UpdateSensors(dirX, jumpPressed);
        SetState(dirX);
        StateExecute(dirX, jumpHeld);
        MoveAndSlide();
        DebugTime(IS_DEBUG);

    }
    private void UpdateSensors(float dirX, bool jumpPressed)
    {
        bool wallL = Physics.OverlapSphere(wallCheckL.position, 0.02f, wallLayer).Length > 0;
        bool wallR = Physics.OverlapSphere(wallCheckR.position, 0.02f, wallLayer).Length > 0;
        
        wasTouchingWall = isTouchingWall;
        isTouchingWall = wallL || wallR;
        wallDirection = wallR ? 1 : (wallL ? -1 : 0);

        if (dirX > 0) isRight = true;
        else if (dirX < 0) isRight = false;

        // Timers
        if (controller.isGrounded) coyoteTimer = coyoteTime;
        else coyoteTimer = Mathf.Max(0f, coyoteTimer - Time.deltaTime);

        if (jumpPressed) jumpBuf = jumpBufferTime;
        else jumpBuf = Mathf.Max(0f, jumpBuf - Time.deltaTime);
    }

    private void SetState(float dirX)
    {
        // ground jump
        if (controller.isGrounded)
        {
            if (jumpBuf > 0)
            {
                Jump();
                return;
            }
            state = (dirX != 0) ? PlayerState.Walk : PlayerState.Idle;//todo add pushwall
            return;
        }

        // assisted jump
        if (coyoteTimer > 0 && jumpBuf > 0)
        {
            Jump();
            return;
        }

        // wall slide/cling
        if (isTouchingWall && yVel <= 0)
        {
            if (jumpBuf > 0)
            {
                WallJump();
                return;
            }
            // todo note this should only happen to airborne
            bool pushingIntoWall = (dirX < 0 && wallDirection == -1) || (dirX > 0 && wallDirection == 1);
            state = pushingIntoWall ? PlayerState.WallCling : PlayerState.WallSlide;
            return;
        }

        if (yVel > 0 && state != PlayerState.WallJump)
        {
            state = PlayerState.Jump;
        }
        else if (state != PlayerState.WallJump)
        {
            state = PlayerState.Fall;
        }
    }

    private void StateExecute(float dirX, bool jumpHeld)
    {
        xVel = dirX * moveSpeed;

        switch (state)
        {
            case PlayerState.Idle:
            case PlayerState.Walk:
                yVel = -2f; // Keeps character firmly grounded
                curGravity = jumpGravity;
                break;

            case PlayerState.Jump:
                curGravity = jumpGravity;
                // release jump fall early
                if (!jumpHeld && yVel > 0)
                    yVel *= 0.4f;

                // Reduce gravity when near the top of the jump
                if (Mathf.Abs(yVel) < apexThreshold)
                    curGravity *= apexGravityMult;
                break;

            case PlayerState.Fall:
                curGravity = fallGravity;
                break;

            case PlayerState.WallCling:
                curGravity = 0f;
                yVel = 0f;
                break;

            case PlayerState.WallSlide:
                // slow down when enter wall
                if (!wasTouchingWall)
                    yVel = Mathf.Max(-terminalWallSlideSpeed, yVel * wallSlideEnterDampMult);

                curGravity = wallSlideGravity;
                break;

            case PlayerState.WallJump:
                curGravity = jumpGravity;
                // Transition back to normal aerial control once rising velocity finishes
                if (yVel <= 0) state = PlayerState.Fall;
                break;
        }
    }

    private void Jump()
    {
        state = PlayerState.Jump;
        curGravity = jumpGravity;
        jumpBuf = 0f;
        coyoteTimer = 0f;
        yVel = jumpSpeed;
    }
    private void WallJump()
    {
        state = PlayerState.WallJump;
        curGravity = jumpGravity;
        jumpBuf = 0f;
        coyoteTimer = 0f;

        yVel = jumpSpeed; //todo varied too
        // vary jump dist if holding?
        // Kick away from the wall opposite to wallDirection
        xVel = -wallDirection * wallJumpKickSpeed; 
    }

    private void MoveAndSlide()
    {
        yVel -= curGravity * Time.deltaTime;
        if (state == PlayerState.WallSlide) yVel = Mathf.Max(yVel, -terminalWallSlideSpeed);
        else yVel = Mathf.Max(yVel, -terminalFallSpeed);

        Vector3 moveDirection = new Vector3(xVel, yVel, 0f);
        controller.Move(moveDirection * Time.deltaTime);
    }

    private void DebugTime(bool isDebug)
    {
        if (isDebug){if (Keyboard.current != null && Keyboard.current.tKey.wasPressedThisFrame)
        {
            Time.timeScale = (Time.timeScale != 1f) ? 1f : 0.25f;
            Debug.Log($"Time scale set to: {Time.timeScale}");
        }}
    }
}