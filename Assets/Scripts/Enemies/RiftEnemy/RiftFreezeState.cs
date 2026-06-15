using UnityEngine;

public class RiftFreezeState : EnemyState
{
    private RiftController rift;
    private float freezeEndTime;
    private bool isUnfreezing;
    private float unfreezeStartTime;

    public RiftFreezeState(RiftController rift) : base(rift)
    {
        this.rift = rift;
    }

    public override void Enter()
    {
        // Stop movement
        rift.rb.linearVelocity = Vector2.zero;
        rift.rb.gravityScale = 1f;

        freezeEndTime = Time.time + rift.frozenTime;

        if (rift.animator != null)
            rift.animator.SetTrigger("Freeze");

        isUnfreezing = false;
    }

    public override void Update()
    {
        if (!isUnfreezing)
        {
            if (Time.time >= freezeEndTime)
            {
                isUnfreezing = true;
                if (rift.animator != null)
                    rift.animator.SetTrigger("Unfreeze");
                unfreezeStartTime = Time.time;
            }
        }
        else
        {
            // Wait for unfreeze animation
            if (Time.time >= unfreezeStartTime + 1f)
            {
                rift.ChangeState(new RiftIdleState(rift));
            }
        }
    }

    public override void FixedUpdate() { }
    public override void Exit() { }
}