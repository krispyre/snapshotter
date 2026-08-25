using UnityEngine;

public class ClawFSM : FSM<ClawState>
{
    public ClawReady clawReady;
    public ClawShooting clawShooting;
    public ClawGrabbing clawGrabbing;
    public ClawMiss clawMiss;
    public ClawReturn clawReturn;

    public ClawFSM(PlayerMovement p)
    {
        clawReady = new ClawReady(p);
        clawShooting = new ClawShooting(p);
        clawGrabbing = new ClawGrabbing(p);
        clawMiss = new ClawMiss(p);
        clawReturn = new ClawReturn(p);
    }
}
public class ClawState : IState
{
    protected readonly PlayerMovement p;
    public ClawState(PlayerMovement p) => this.p = p;
    public virtual void Enter() { }
    public virtual void Exit() { }
    public virtual void FixedUpdate() { }
}
public sealed class ClawReady : ClawState
{
    //so copy pastey, any better options
    public ClawReady(PlayerMovement p) : base(p) { }
    public override void Enter() { }

    public override void FixedUpdate()
    {
        if (p.shootPressed)
            p.clawFsm.SetState(p.clawFsm.clawShooting);
    }

    public override void Exit() { }
}

public sealed class ClawShooting : ClawState
{
    const float CastRadius = 0.05f;
    const float CastSkin = 0.02f;

    public ClawShooting(PlayerMovement p) : base(p) { }
    public override void Enter()
    {
        Vector3 origin = p.armOrigin.position;
        Vector3 aim = p.clawPointer.position - origin;
        aim.z = 0f; // stay on the play plane

        if (aim.sqrMagnitude < 0.0001f)
            aim = Vector3.right;
        Vector3 dir = aim.normalized;

        // MeshCollider raycasts ignore backfaces; enable for this query only
        bool prevBackfaces = Physics.queriesHitBackfaces;
        Physics.queriesHitBackfaces = true;
        // start slightly behind so we still catch walls we're already touching
        bool didHit = Physics.SphereCast(
            origin - dir * CastSkin,
            CastRadius,
            dir,
            out RaycastHit hitInfo,
            p.clawParams.armLength + CastSkin,
            p.WallLayer,
            QueryTriggerInteraction.Ignore);
        Physics.queriesHitBackfaces = prevBackfaces;

        if (didHit)
        {
            p.landingTarget = hitInfo.point;
            p.landingTarget.z = origin.z;
            p.missed = false;
        }
        else
        {
            p.landingTarget = origin + dir * p.clawParams.armLength;
            p.missed = true;
        }

        // overrides player state here
        p.prevState = p.state;
        p.clawShootOrigin = origin;

        p.claw.transform.position = origin;
        p.claw.SetActive(true);
        p.state = PlayerMovement.PlayerState.Clawing;
    }

    public override void FixedUpdate()
    {
        Vector3 origin = p.armOrigin.position;
        p.claw.transform.rotation = p.LookAt(origin, p.landingTarget);
        Vector3 vel = p.LinearVel(p.clawShootOrigin, p.landingTarget, p.clawParams.flyTime);
        p.claw_xVel = vel.x;
        p.claw_yVel = vel.y;

        if (Vector3.Distance(p.claw.transform.position, p.landingTarget) < 0.02f)
        {
            Debug.Log("arrive");
            p.claw.transform.position = p.landingTarget;
            p.clawFsm.SetState(p.missed ? p.clawFsm.clawMiss : p.clawFsm.clawGrabbing);
        }
    }
    public override void Exit() { }
}

public sealed class ClawMiss : ClawState
{
    public ClawMiss(PlayerMovement p) : base(p) { }
    int wait;

    public override void Enter()
    {
        wait = p.clawParams.pullDelay;
        p.claw_xVel = 0;
        p.claw_yVel = 0;
    }

    public override void FixedUpdate()
    {
        if (wait > 0) { wait--; }
        else { p.clawFsm.SetState(p.clawFsm.clawReturn); }
    }

    public override void Exit() { }
}

public sealed class ClawGrabbing : ClawState
{
    public ClawGrabbing(PlayerMovement p) : base(p) { }
    public override void Enter()
    {
        p.claw_xVel = 0;
        p.claw_yVel = 0;

        p.state = PlayerMovement.PlayerState.ClawFly;
    }

    public override void FixedUpdate()
    {
        Debug.Log(Vector3.Distance(p.claw.transform.position, p.transform.position));
        p.claw.transform.rotation = p.LookAt(p.armOrigin.position, p.landingTarget);
        if (p.shootPressed)
        {
            p.clawFsm.SetState(p.clawFsm.clawReturn);
        }
        if (Vector3.Distance(p.claw.transform.position, p.transform.position) < 0.2f)
        {
            p.transform.position = p.claw.transform.position;
            p.state = PlayerMovement.PlayerState.WallCling; //todo ceiling Hang
        }
    }

    public override void Exit() { }
}

public sealed class ClawReturn : ClawState
{
    public ClawReturn(PlayerMovement p) : base(p) { }
    public override void Enter() => ApplyFlight();

    public override void FixedUpdate()
    {
        ApplyFlight();
        if (Vector3.Distance(p.claw.transform.position, p.armOrigin.position) < 0.2f)
            p.clawFsm.SetState(p.clawFsm.clawReady);
    }

    public override void Exit()
    {
        p.claw.SetActive(false);
        p.state = p.prevState;
        p.claw_xVel = 0;
        p.claw_yVel = 0;
    }

    //todo horrendous name
    void ApplyFlight()
    {
        Vector3 origin = p.armOrigin.position;
        p.claw.transform.rotation = p.LookAt(origin, p.landingTarget);
        Vector3 vel = p.LinearVel(p.landingTarget, origin, p.clawParams.returnTime);
        p.claw_xVel = vel.x;
        p.claw_yVel = vel.y;
    }
}