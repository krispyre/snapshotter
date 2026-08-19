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
    public ClawShooting(PlayerMovement p) : base(p) { }
    public override void Enter()
    {
        Vector3 aim = p.clawPointer.position - p.transform.position;
        Ray ray = new Ray(p.transform.position, aim);

        Debug.Log("shoot");
        Vector3 aimDirection = p.clawPointer.position - p.transform.position;
        Ray clawRay = new Ray(p.transform.position, aimDirection);

        if (Physics.Raycast(clawRay, out RaycastHit hitInfo, p.clawParams.armLength))
        {
            p.landingTarget = hitInfo.point;
            p.claw.transform.position = p.landingTarget;
            p.missed = false;
        }
        else
        {
            p.landingTarget = p.transform.position + p.clawParams.armLength * aimDirection;
            Debug.LogAssertion("claw miss");
            p.missed = true;
        }

        // overrides player state here
        p.prevState = p.state;

        p.claw.transform.position = p.transform.position;
        p.claw.SetActive(true);
        p.state = PlayerMovement.PlayerState.Clawing;
    }

    public override void FixedUpdate()
    {
        p.claw.transform.rotation = p.LookAt(p.transform.position, p.landingTarget);
        Vector3 vel = p.LinearVel(p.transform.position, p.landingTarget, p.clawParams.flyTime);
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
    }

    public override void FixedUpdate()
    {
        p.claw.transform.rotation = p.LookAt(p.transform.position, p.landingTarget);
        if (p.shootPressed)
            p.clawFsm.SetState(p.clawFsm.clawReturn);
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
        if (Vector3.Distance(p.claw.transform.position, p.transform.position) < 0.02f)
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
        p.claw.transform.rotation = p.LookAt(p.transform.position, p.landingTarget);
        Vector3 vel = p.LinearVel(p.landingTarget, p.transform.position, p.clawParams.returnTime);
        p.claw_xVel = vel.x;
        p.claw_yVel = vel.y;
    }
}