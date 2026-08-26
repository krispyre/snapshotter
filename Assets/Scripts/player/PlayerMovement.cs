using UnityEngine;
using UnityEngine.InputSystem;
using System.ComponentModel;

public partial class PlayerMovement : MonoBehaviour
{
    const bool IS_DEBUG = true;
    [Header("bot")]
    [SerializeField] private PlayerMvmtParams mvmtParams;
    [SerializeField, ReadOnlyInspector] private Vector3 pos;

    [SerializeField, ReadOnlyInspector] private Vector3 claw_pos;
    [SerializeField, ReadOnlyInspector] public PlayerState state = PlayerState.Idle;
    [SerializeField, ReadOnlyInspector] public PlayerState prevState; //state before clawing

    [SerializeField, ReadOnlyInspector] private float xVel = 0f;
    [SerializeField, ReadOnlyInspector] private float yVel = 0f;
    [SerializeField, ReadOnlyInspector] private bool isGrounded;
    [SerializeField, ReadOnlyInspector] private bool isTouchingWall;
    [SerializeField, ReadOnlyInspector] private int wallDirection; // -1 for left, 1 for right
    [SerializeField, ReadOnlyInspector] private bool isRight = true;
    [SerializeField, ReadOnlyInspector] private float curGravity;
    [SerializeField, ReadOnlyInspector] private float curXAccel;
    [SerializeField] private Transform wallCheckL;
    [SerializeField] private Transform wallCheckR;
    [SerializeField] private LayerMask wallLayer;
    public LayerMask WallLayer => wallLayer;
    [SerializeField, ReadOnlyInspector] private int wallJumpLockTimer; //frame count

    const float CastSkin = 0.01f;
    const float MinGroundNormalY = 0.5f;
    readonly RaycastHit[] sweepBuf = new RaycastHit[8];

    private Rigidbody body;
    private Collider bodyCollider;
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
        ClawInit();

        body = GetComponent<Rigidbody>();
        bodyCollider = GetComponent<Collider>();
        if (bodyCollider == null || bodyCollider.isTrigger)
        {
            foreach (var col in GetComponentsInChildren<Collider>())
            {
                if (!col.isTrigger)
                {
                    bodyCollider = col;
                    break;
                }
            }
        }
        ConfigureBody();
        playerInput = GetComponent<PlayerInput>();
        CacheActions();
    }

    private void OnEnable() => CacheActions();

    private void OnDisable()
    {
        dirXAction = null;
        dirYAction = null;
        jumpAction = null;
        shootAction = null;
    }

    private void OnDestroy()
    {
        playerInput = null;
        dirXAction = null;
        dirYAction = null;
        jumpAction = null;
        shootAction = null;
    }

    private void CacheActions()
    {
        if (playerInput == null)
            playerInput = GetComponent<PlayerInput>();

        // Unity destroyed objects are "fake null"; bail before touching them
        if (playerInput == null || playerInput.actions == null)
            return;

        dirXAction = playerInput.actions.FindAction("DirX");
        dirYAction = playerInput.actions.FindAction("DirY");
        jumpAction = playerInput.actions.FindAction("Jump");
        shootAction = playerInput.actions.FindAction("ShootToggle");//todo whats the name
    }

    void Start()
    {
    }

    // update check inputs, fixedupdate calc physics
    void Update()
    {
        if (dirXAction == null || dirYAction == null || jumpAction == null || shootAction == null)
        {
            CacheActions();
            if (dirXAction == null || dirYAction == null || jumpAction == null || shootAction == null)
                return;
        }

        // todo put these back to start() after tweaking
        jumpSpeed = Mathf.Sqrt(2f * mvmtParams.jumpGravity * mvmtParams.jumpHeight);

        inputDirX = dirXAction.ReadValue<float>();
        inputDirY = dirYAction.ReadValue<float>();
        if (jumpAction.WasPressedThisFrame()) jumpPressed = true;
        if (shootAction.WasPressedThisFrame()) shootPressed = true;
        jumpHeld = jumpAction.IsPressed();


        DebugTime(IS_DEBUG);
        ResetClaw();

        if (Keyboard.current != null && Keyboard.current.bKey.wasPressedThisFrame)
        {
            clawFsm.SetState(clawFsm.clawReady);
        }

    }

    void FixedUpdate()
    {
        UpdateSensors(inputDirX, jumpPressed);
        SetState(inputDirX);
        if (clawFsm != null && clawFsm.Current != null)
        {
            clawFsm.Current.FixedUpdate();
            clawState = clawFsm.Current?.GetType().Name;
        }
        StateExecute(inputDirX, jumpHeld);
        MoveAndSlide();// todo override speed clamps after this for claw physics
        ClawMoveAndSlide();
        jumpPressed = false;
        shootPressed = false;
        pos = BodyPosition;
        claw_pos = claw.transform.position;
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
        if (isGrounded) coyoteTimer = mvmtParams.coyoteTime;
        else coyoteTimer = Mathf.Max(0f, coyoteTimer - Time.fixedDeltaTime);

        if (jumpPressed) jumpBuf = mvmtParams.jumpBufferTime;
        else jumpBuf = Mathf.Max(0f, jumpBuf - Time.fixedDeltaTime);

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
            if (isGrounded)
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
                Vector3 vel = LinearVel(clawShootOrigin, landingTarget, clawParams.flyTime);
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

    public Vector3 BodyPosition => body != null ? body.position : transform.position;

    public void SetBodyPosition(Vector3 worldPos)
    {
        if (body == null) return;
        worldPos.z = body.position.z;
        body.position = worldPos;
    }

    private void ConfigureBody()
    {
        if (body == null) return;
        body.isKinematic = true;
        body.useGravity = false;
        body.interpolation = RigidbodyInterpolation.Interpolate;
        body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        body.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionZ;
    }

    private bool IsOwnCollider(Collider other)
    {
        return other != null && (other == bodyCollider || other.transform.IsChildOf(transform));
    }

    private void MoveAndSlide()
    {
        float dt = Time.fixedDeltaTime;
        if (state != PlayerState.Clawing && state != PlayerState.ClawFly)
        {
            yVel -= curGravity * dt;
            xVel += curXAccel * dt;

            if (isGrounded)
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

        bool groundedThisMove = false;
        Vector3 nextPos = BodyPosition;
        nextPos = SweepMove(nextPos, new Vector3(xVel * dt, 0f, 0f), ref groundedThisMove);
        nextPos = SweepMove(nextPos, new Vector3(0f, yVel * dt, 0f), ref groundedThisMove);
        isGrounded = groundedThisMove;
        if (body != null)
            body.MovePosition(nextPos);
    }

    private Vector3 SweepMove(Vector3 origin, Vector3 delta, ref bool hitGround)
    {
        if (delta.sqrMagnitude < 1e-12f) return origin;

        GetWorldCapsule(origin, out Vector3 p1, out Vector3 p2, out float radius);
        float mag = delta.magnitude;
        Vector3 dir = delta / mag;
        int hits = Physics.CapsuleCastNonAlloc(
            p1, p2, radius, dir, sweepBuf, mag + CastSkin, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);

        float best = mag;
        int bestIndex = -1;
        for (int i = 0; i < hits; i++)
        {
            if (IsOwnCollider(sweepBuf[i].collider)) continue;
            if (sweepBuf[i].distance < best)
            {
                best = sweepBuf[i].distance;
                bestIndex = i;
            }
        }
        if (bestIndex < 0)
            return origin + delta;

        if (sweepBuf[bestIndex].normal.y >= MinGroundNormalY)
            hitGround = true;
        return origin + dir * Mathf.Max(0f, best - CastSkin);
    }

    private void GetWorldCapsule(Vector3 worldPos, out Vector3 p1, out Vector3 p2, out float radius)
    {
        if (bodyCollider is CapsuleCollider cap)
        {
            Vector3 scale = transform.lossyScale;
            float radial = Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.z));
            radius = Mathf.Max(0.01f, cap.radius * radial - CastSkin);
            float height = cap.height * Mathf.Abs(scale.y);
            Vector3 center = worldPos + Vector3.Scale(cap.center, scale);
            float half = Mathf.Max(0f, height * 0.5f - radius);
            p1 = center + Vector3.up * half;
            p2 = center - Vector3.up * half;
            return;
        }

        if (bodyCollider is SphereCollider sphere)
        {
            float radial = Mathf.Max(Mathf.Abs(transform.lossyScale.x), Mathf.Abs(transform.lossyScale.z));
            radius = Mathf.Max(0.01f, sphere.radius * radial - CastSkin);
            p1 = p2 = worldPos + Vector3.Scale(sphere.center, transform.lossyScale);
            return;
        }

        if (bodyCollider is BoxCollider box)
        {
            Vector3 extents = Vector3.Scale(box.size * 0.5f, transform.lossyScale);
            radius = Mathf.Max(0.01f, Mathf.Min(extents.x, extents.z) - CastSkin);
            Vector3 center = worldPos + Vector3.Scale(box.center, transform.lossyScale);
            float half = Mathf.Max(0f, extents.y - radius);
            p1 = center + Vector3.up * half;
            p2 = center - Vector3.up * half;
            return;
        }

        radius = 0.09f;
        p1 = p2 = worldPos;
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