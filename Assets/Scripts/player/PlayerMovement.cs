using UnityEngine;
using UnityEngine.InputSystem;
using System.ComponentModel;

public partial class PlayerMovement : MonoBehaviour
{
    const bool IS_DEBUG = true;
    [Header("bot")]
    [SerializeField] private PlayerMvmtParams mvmtParams;
    [SerializeField, ReadOnlyInspector] public PlayerState state = PlayerState.Idle;
    [SerializeField, ReadOnlyInspector] public PlayerState prevState; //state before clawing

    [SerializeField, ReadOnlyInspector] private float xVel = 0f;
    [SerializeField, ReadOnlyInspector] private float yVel = 0f;
    [SerializeField, ReadOnlyInspector] private bool isTouchingWall;
    [SerializeField, ReadOnlyInspector] private int wallDirection; // -1 for left, 1 for right
    [SerializeField, ReadOnlyInspector] private bool isRight = true;
    [SerializeField, ReadOnlyInspector] private float curGravity;
    [SerializeField, ReadOnlyInspector] private float curXAccel;
    [SerializeField] private Transform wallCheckL;
    [SerializeField] private Transform wallCheckR;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField, ReadOnlyInspector] private int wallJumpLockTimer; //frame count

    private CharacterController controller;
    private PlayerInput playerInput;
    private InputAction dirXAction;
    private InputAction dirYAction;
    private InputAction jumpAction;
    private InputAction shootAction;
    [SerializeField] Camera mainCamera;

    //movement vars
    private float jumpSpeed;
    [SerializeField, ReadOnlyInspector] private float jumpBuf;
    [SerializeField, ReadOnlyInspector] private float coyoteTimer;

    //input cache
    public float inputDirX;
    public float inputDirY;
    public bool jumpPressed;
    public bool jumpHeld;
    public bool shootPressed;

    public bool wasTouchingWall;

    public enum PlayerState { Idle, Walk, Jump, Fall, WallSlide, WallCling, WallJump, Clawing, ClawFly }

    private void Awake()
    {
        clawFsm = new ClawFSM(this);
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
            shootAction = playerInput.actions.FindAction("ShootToggle");//todo whats the name
        }
    }

    void Start()
    {
        ;
    }

    // update check inputs, fixedupdate calc physics
    void Update()
    {
        if (dirXAction == null || dirYAction == null || jumpAction == null)
        {
            CacheActions();
            if (dirXAction == null || dirYAction == null) return;
        }

        // todo put these back to start() after tweaking
        jumpSpeed = Mathf.Sqrt(2f * mvmtParams.jumpGravity * mvmtParams.jumpHeight);

        inputDirX = dirXAction.ReadValue<float>();
        inputDirY = dirYAction.ReadValue<float>();
        if (jumpAction.WasPressedThisFrame()) jumpPressed = true;
        if (shootAction.WasPressedThisFrame()) shootPressed = true;
        jumpHeld = jumpAction.IsPressed();


        DebugTime(IS_DEBUG);

        if (Keyboard.current.bKey.wasPressedThisFrame)
        {
            clawFsm.SetState(clawFsm.clawReady);
        }

    }

    void FixedUpdate()
    {
        UpdateSensors(inputDirX, jumpPressed);
        SetState(inputDirX);
        clawFsm.Current.FixedUpdate();
        StateExecute(inputDirX, jumpHeld);
        MoveAndSlide();// todo override speed clamps after this for claw physics
        ClawMoveAndSlide();
        jumpPressed = false;
        shootPressed = false;
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
        if (controller.isGrounded) coyoteTimer = mvmtParams.coyoteTime;
        else coyoteTimer = Mathf.Max(0f, coyoteTimer - Time.deltaTime);

        if (jumpPressed) jumpBuf = mvmtParams.jumpBufferTime;
        else jumpBuf = Mathf.Max(0f, jumpBuf - Time.deltaTime);

        UpdateClawPointerPos();
    }

    private void SetState(float dirX)
    {
        if (state == PlayerState.Clawing) return;
        if (state == PlayerState.ClawFly)
        {
            if (Vector3.Distance(transform.position, claw.transform.position) < 0.02)
            {
                ;
            }
        }
        ;


        // non claw actions.
        if (state != PlayerState.Clawing && state != PlayerState.ClawFly)
        {// ground jump
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
    }

    private void StateExecute(float dirX, bool jumpHeld)
    {
        switch (state)
        {
            case PlayerState.Idle:
            case PlayerState.Walk:
                curGravity = mvmtParams.jumpGravity;//ground control, larger friction
                GroundControl(dirX);
                yVel = -1f;

                break;

            case PlayerState.Jump:
                curGravity = mvmtParams.jumpGravity;
                // release jump fall early
                if (!jumpHeld && yVel > 0)
                    yVel *= 0.4f;

                // Reduce gravity when near the top of the jump
                if (Mathf.Abs(yVel) < mvmtParams.apexThreshold)
                    curGravity *= mvmtParams.apexGravityMult;

                AirControl(dirX);
                xVel = Mathf.Clamp(xVel, -mvmtParams.maxAirSpeed, mvmtParams.maxAirSpeed);
                break;

            case PlayerState.Fall:
                curGravity = mvmtParams.fallGravity;
                AirControl(dirX);
                xVel = Mathf.Clamp(xVel, -mvmtParams.maxAirSpeed, mvmtParams.maxAirSpeed);
                break;

            case PlayerState.WallCling:
                curGravity = 0f;
                yVel = 0f;
                xVel = wallDirection;
                break;

            case PlayerState.WallSlide:
                // slow down when enter wall
                xVel = wallDirection;

                if (!wasTouchingWall)
                    yVel = Mathf.Max(-mvmtParams.terminalWallSlideSpeed, yVel * mvmtParams.wallSlideEnterDampMult);
                curGravity = mvmtParams.wallSlideGravity;
                break;

            case PlayerState.WallJump:
                curGravity = mvmtParams.jumpGravity;
                // Transition back to normal aerial control once rising velocity finishes
                if (wallJumpLockTimer > 0)
                {
                    // lock left right yet, only give back control after it ends
                    wallJumpLockTimer--;
                    curXAccel = wallDirection * 1.2f;
                }
                else
                {
                    wallJumpLockTimer = 0;
                    AirControl(dirX);
                }

                if (yVel <= 0 && wallJumpLockTimer <= 0) state = PlayerState.Fall;
                break;
            case PlayerState.Clawing:
                xVel = 0;
                yVel = 0;
                curXAccel = 0;
                curGravity = 0;
                break;
            case PlayerState.ClawFly:
                Vector3 vel = (landingTarget - transform.position).normalized * Vector3.Distance(landingTarget, transform.position) / (clawParams.flyTime * (1 / 60f));
                xVel = vel.x;
                yVel = vel.y;
                curGravity = 0;
                break;
        }
    }

    private void GroundControl(float dirX)
    {
        if (dirX != 0)
        {

            if (Mathf.Sign(dirX) != Mathf.Sign(xVel))
            {
                //Turning Around
                curXAccel = mvmtParams.walkDecel * dirX;
            }
            else
            {   //Going forward
                curXAccel = mvmtParams.walkAccel * dirX;
            }
        }
        else
        {
            if (Mathf.Abs(xVel) < 0.21)//todo a really small threshold
            {
                //Snap to 0
                curXAccel = 0;
                xVel = 0;

            }
            else
            { // Brake
                curXAccel = mvmtParams.walkDecel * -Mathf.Sign(xVel);
            }
        }
    }


    private void AirControl(float dirX)
    {
        // air control logic. shared between jump, walljump, fall
        if (dirX != 0)
        {

            if (Mathf.Sign(dirX) != Mathf.Sign(xVel))
            {
                //Turning Around
                curXAccel = mvmtParams.airDecel * dirX;
            }
            else
            {   //Going forward
                curXAccel = mvmtParams.airAccel * dirX;
            }
        }
        else
        {
            if (Mathf.Abs(xVel) < 0.005)//todo a really small threshold
            {
                //Snap to 0
                curXAccel = 0;
                xVel = 0;

            }
            else
            {// Brake
                curXAccel = mvmtParams.airDecel * -Mathf.Sign(xVel);
            }
        }

    }

    private void Jump()
    {
        state = PlayerState.Jump;
        curGravity = mvmtParams.jumpGravity;
        jumpBuf = 0;
        coyoteTimer = 0;
        yVel = jumpSpeed;
    }
    private void WallJump()
    {
        // init state setup
        state = PlayerState.WallJump;
        curGravity = mvmtParams.jumpGravity;
        wallJumpLockTimer = mvmtParams.wallJumpLock;
        // Debug.Log(wallJumpLockTimer + " " + wallJumpLock);
        jumpBuf = 0;
        coyoteTimer = 0;

        yVel = jumpSpeed; //todo varied too

        // vary jump dist if holding?
        // Kick away from the wall opposite to wallDirection
        xVel += -wallDirection * mvmtParams.wallJumpKickSpeed;
        Debug.Log(xVel + " " + mvmtParams.wallJumpKickSpeed);
    }

    private void MoveAndSlide()
    {
        if (state != PlayerState.Clawing && state != PlayerState.ClawFly)
        {
            yVel -= curGravity * Time.deltaTime;

            xVel += curXAccel * Time.deltaTime;

            if (controller.isGrounded)
            {
                xVel = Mathf.Clamp(xVel, -mvmtParams.maxWalkSpeed, mvmtParams.maxWalkSpeed);
            }
            else
            {
                xVel = Mathf.Clamp(xVel, -GetMaxAirSpeed(), GetMaxAirSpeed());
            }
            if (state == PlayerState.WallSlide) yVel = Mathf.Max(yVel, -mvmtParams.terminalWallSlideSpeed);
            else yVel = Mathf.Max(yVel, -mvmtParams.terminalFallSpeed);
        }
        Vector3 moveDirection = new Vector3(xVel, yVel, 0f);
        controller.Move(moveDirection * Time.deltaTime); //should this be fixeddelta
    }

    private float GetMaxAirSpeed()
    {
        if (state == PlayerState.WallJump) return float.MaxValue;
        return mvmtParams.maxAirSpeed;
    }

    private void DebugTime(bool isDebug)
    {
        if (isDebug)
        {
            if (Keyboard.current != null && Keyboard.current.tKey.wasPressedThisFrame)
            {
                Time.timeScale = (Time.timeScale != 1f) ? 1f : 0.1f;
                Debug.Log($"Time scale set to: {Time.timeScale}");
            }
        }
    }
}