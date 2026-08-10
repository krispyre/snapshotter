using UnityEngine;

[CreateAssetMenu(fileName = "PlayerMvmtParams", menuName = "Player/MvmtParams")]
public class PlayerMvmtParams : ScriptableObject
{
    [Header("move")]
    public float walkAccel = 15f;
    public float walkDecel = 30f;// for forwards
    public float airAccel = 20f;//for turn around&stop
    public float airDecel = 30f;
    public float maxWalkSpeed = 1.5f;//run is different
    public float maxAirSpeed = 3;//no wavedashing

    [Header("jump")]
    public float jumpHeight = 0.5f;
    public float jumpGravity = 40f;
    public float fallGravity = 15f;
    public float jumpBufferTime = 0.1f;
    public float coyoteTime = 0.1f;
    public float apexGravityMult = 0.4f;
    public float apexThreshold = 0.5f; // start reducing gravity when yVel within [-this ~ this].
    public float wallSlideGravity = 4f;
    public float wallSlideEnterDampMult = 0.2f;
    public float terminalWallSlideSpeed = 5f;
    public float wallJumpKickSpeed = 5f;
    public float terminalFallSpeed = 10f;
}