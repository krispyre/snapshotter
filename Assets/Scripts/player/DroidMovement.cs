using UnityEngine;
using UnityEngine.InputSystem;

public class DroidMovement : MonoBehaviour
{
    bool IS_DEBUG = true;
    [SerializeField] private PlayerMvmtParams mvmtParams;
    [SerializeField, ReadOnlyInspector] public PlayerState state = PlayerState.Idle;

    [SerializeField, ReadOnlyInspector] public float xVel = 0f;
    [SerializeField, ReadOnlyInspector] public float yVel = 0f;
    [SerializeField, ReadOnlyInspector] private bool isTouchingWall;

    [SerializeField, ReadOnlyInspector] private bool wasTouchingWall;
    [SerializeField, ReadOnlyInspector] private int wallDirection; // -1 for left, 1 for right
    [SerializeField, ReadOnlyInspector] private bool isRight = true;
    [SerializeField, ReadOnlyInspector] private float curGravity;
    [SerializeField, ReadOnlyInspector] private float curXAccel;
    [SerializeField] private LayerMask wallLayer;
    public LayerMask WallLayer => wallLayer;
    [SerializeField, ReadOnlyInspector] private int wallJumpLockTimer; //frame count

    private CharacterController controller;
    private PlayerInput playerInput;
    private InputAction dirXAction;
    private InputAction dirYAction;
    private InputAction jumpAction;

    private float jumpSpeed;
    [SerializeField, ReadOnlyInspector] private float jumpBuf;
    [SerializeField, ReadOnlyInspector] private float coyoteTimer;

    public float inputDirX;
    public float inputDirY;
    public bool jumpPressed;
    public bool jumpHeld;

    public enum PlayerState { Idle, Walk, Jump, Fall, WallPush }

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();
        CacheActions();
    }

    private void OnEnable() => CacheActions();


    private void CacheActions()
    {
        if (playerInput == null)
            playerInput = GetComponentInParent<PlayerInput>();

        if (playerInput == null || playerInput.actions == null)
            return;

        dirXAction = playerInput.actions.FindAction("DirX");
        dirYAction = playerInput.actions.FindAction("DirY");
        jumpAction = playerInput.actions.FindAction("Jump");
    }

    void Update()
    {
        if (Keyboard.current == null) return;
        if (dirXAction == null || jumpAction == null)
        {
            return;
        }

        // todo put these back to start() after tweaking
        jumpSpeed = Mathf.Sqrt(2f * mvmtParams.jumpGravity * mvmtParams.jumpHeight);

        inputDirX = dirXAction.ReadValue<float>();
        inputDirY = dirYAction.ReadValue<float>();
        if (jumpAction.WasPressedThisFrame()) jumpPressed = true;
        jumpHeld = jumpAction.IsPressed();

        DebugTime(IS_DEBUG);
    }

    void FixedUpdate()
    {
        UpdateSensors(inputDirX, jumpPressed);
        SetState(inputDirX);
        StateExecute(inputDirX, jumpHeld);
        MoveAndSlide();
        jumpPressed = false;
    }

    private void UpdateSensors(float dirX, bool jumpPressed)
    {
        float dist = controller.radius + controller.skinWidth + 0.05f;
        bool wallL = Physics.BoxCast(transform.position, new Vector3(dist, dist, dist), Vector3.left, transform.rotation, dist, wallLayer);
        bool wallR = Physics.BoxCast(transform.position, new Vector3(dist, dist, dist), Vector3.right, transform.rotation, dist, wallLayer);

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
    }

    private void SetState(float dirX)
    {
        if (controller.isGrounded)
        {
            if (jumpBuf > 0)
            {
                Jump();
                return;
            }
            state = (dirX != 0) ? PlayerState.Walk : PlayerState.Idle;
            return;
        }

        if (coyoteTimer > 0 && jumpBuf > 0)
        {
            Jump();
            return;
        }

        state = yVel > 0 ? PlayerState.Jump : PlayerState.Fall;
    }

    private void StateExecute(float dirX, bool jumpHeld)
    {
        switch (state)
        {
            case PlayerState.Idle:
            case PlayerState.Walk:
                curGravity = mvmtParams.jumpGravity;
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

            case PlayerState.WallPush:
                curGravity = 0f;
                yVel = 0f;
                xVel = wallDirection;
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

    private void MoveAndSlide()
    {
        yVel -= curGravity * Time.deltaTime;
        xVel += curXAccel * Time.deltaTime;

        if (controller.isGrounded)
            xVel = Mathf.Clamp(xVel, -mvmtParams.maxWalkSpeed, mvmtParams.maxWalkSpeed);
        else
            xVel = Mathf.Clamp(xVel, -mvmtParams.maxAirSpeed, mvmtParams.maxAirSpeed);

        yVel = Mathf.Max(yVel, -mvmtParams.terminalFallSpeed);
        Vector3 moveDirection = new Vector3(xVel, yVel, 0f);
        controller.Move(moveDirection * Time.deltaTime); //should this be fixeddelta
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