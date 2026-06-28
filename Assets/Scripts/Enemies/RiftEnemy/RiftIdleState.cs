// RiftIdleState.cs
using UnityEngine;

public class RiftIdleState : EnemyState
{
    private RiftController rift;
    private float idleTimer;

    public RiftIdleState(RiftController rift) : base(rift)
    {
        this.rift = rift;
    }

    public override void Enter()
    {
        rift.animator.SetBool("isIdle", true);
        rift.rb.linearVelocity = Vector2.zero;
        rift.rb.gravityScale = 0f;
        idleTimer = 0.2f;
    }

    public override void Update()
    {
        idleTimer -= Time.deltaTime;
        if (idleTimer > 0f) return;

        if (rift.IsFrozen) {
            rift.ChangeState(new RiftFreezeState(rift));
            return;
        }

        if (!enemy.IsPlayerInDetectionRange()) return;

        enemy.ChangeState(new RiftBounceState(rift));
    }

    public override void Exit()
    {
        rift.animator.SetBool("isIdle", false);
    }
}