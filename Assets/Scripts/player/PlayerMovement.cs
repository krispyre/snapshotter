using UnityEngine;
using UnityEngine.InputSystem;
using System.ComponentModel;

public class SimpleController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 1.2f;
    [SerializeField] private float jumpHeight = .5f;
    [SerializeField] private float jumpBufferTime = 0.1f;
    [SerializeField] private float coyoteTime = 0.1f;
    [SerializeField] private float apexGravityMult = 0.4f;
    [SerializeField] private float apexThreshold = 0.5f; // start reducing gravity when yVel within [0~this].

    public float jumpGravity = 55F;
    public float fallGravity = 18f;
    private float jumpSpeed; // only calced when body loaded
    private float xVel = 0f;
    private float yVel = 0f;
    [SerializeField][ReadOnly(true)] private float jumpBuf = 0;
    [SerializeField][ReadOnly(true)] private float coyoteTimer = 0;

    private CharacterController controller;
    [SerializeField] private PlayerInput playerInput;

    [SerializeField] private Transform wallCheck;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private bool isWallSliding;

    private InputAction dirXAction;
    private InputAction dirYAction;
    private InputAction jumpAction;


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

    void Update()
    {
        if (dirXAction == null || dirYAction == null)
        {
            CacheActions();
            if (dirXAction == null || dirYAction == null) return;
        }

        // put this back to start() after tweaking
        jumpSpeed = Mathf.Sqrt(2f * jumpGravity * jumpHeight);

        float dirX = dirXAction.ReadValue<float>();
        float dirY = dirYAction.ReadValue<float>();

        WalkCheck(dirX);
        JumpCheck();
        MoveAndSlide(dirX);
        WallSlide(dirX);



    }

    private void WalkCheck(float dirX)
    {
        ;

    }

    private bool isWalled()
    {
        return Physics.OverlapSphere(wallCheck.transform.position, 0.02f, wallLayer).Length > 0;
    }

    private void JumpCheck()
    {
        bool jumpPressed = jumpAction.WasPressedThisFrame();
        bool jumpHeld = jumpAction.IsPressed();

        float curGravity = yVel <= 0 ? fallGravity : jumpGravity;

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
            yVel -= curGravity * Time.deltaTime;
        }

        jumpBuf = Mathf.Max(0f, jumpBuf - Time.deltaTime);
    }
    private void WallSlide(float dirX)
    {
        if (controller.isGrounded && isWalled() && dirX != 0)
        {
            isWallSliding = true;
        }
        else
        {
            isWallSliding = false;
        }

        if (isWalled()) Debug.Log("asfdsaljk");
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