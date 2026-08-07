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
    [SerializeField] private float wallSlideGravity = 1f;
    [SerializeField] private float terminalWallSlideSpeed = 10f;
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
    [SerializeField] private Transform wallCheck;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField][ReadOnlyInspector] private bool isWallSliding = false;
    [SerializeField][ReadOnlyInspector] private bool isEnteringWall = true;

    [SerializeField][ReadOnlyInspector] private bool isRight = true;
    [SerializeField][ReadOnlyInspector] private float curGravity;

    // input controllers
    [SerializeField] private PlayerInput playerInput;
    private InputAction dirXAction;
    private InputAction dirYAction;
    private InputAction jumpAction;

    public int owo = 0;

    private float xVel = 0f;
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
        // Time.timeScale = 0.5f;

        float dirX = dirXAction.ReadValue<float>();
        float dirY = dirYAction.ReadValue<float>();

        WalkCheck(dirX);
        WallSlide(dirX);
        JumpCheck();
        MoveAndSlide(dirX);

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

    }

    private bool isWalled()
    {
        return Physics.OverlapSphere(wallCheck.transform.position, 0.02f, wallLayer).Length > 0;
    }

    private void JumpCheck()
    {
        bool jumpPressed = jumpAction.WasPressedThisFrame();
        bool jumpHeld = jumpAction.IsPressed();


        if (!isWallSliding) curGravity = yVel <= 0 ? fallGravity : jumpGravity;

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
        if (!controller.isGrounded && isWalled() && dirX != 0)
        {
            isWallSliding = true;
            if (isEnteringWall)
            {
                // slow down when enter wall, 
                yVel = Mathf.Max(-terminalWallSlideSpeed, yVel / 10);
                isEnteringWall = false;
            }

            if (yVel > 0)
            {
                owo++;
                Debug.Log("nope" + owo);
                curGravity = fallGravity * 3f;
            }
            else
                curGravity = wallSlideGravity;
        }
        else
        {
            isWallSliding = false;
            isEnteringWall = true;
        }

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
        if (isWallSliding) yVel = Mathf.Max(yVel, -terminalWallSlideSpeed);
        else yVel = Mathf.Max(yVel, -terminalSpeed);
        Vector3 moveDirection = new Vector3(dirX * moveSpeed, yVel, 0f);
        controller.Move(moveDirection * Time.deltaTime);
    }
}