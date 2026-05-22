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
        rift.rb.linearVelocity = Vector2.zero;
        rift.rb.gravityScale = 0f; // pinned to surface, no gravity needed
        idleTimer = 0.2f;
    }

    public override void Update()
    {
        idleTimer -= Time.deltaTime;
        if (idleTimer > 0f) return;

        if (!enemy.IsPlayerInDetectionRange()) return;

        enemy.ChangeState(new RiftBounceState(rift));
    }
}