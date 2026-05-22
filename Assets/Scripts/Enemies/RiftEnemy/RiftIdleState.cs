using UnityEngine;

public class RiftIdleState : EnemyState
{
    public RiftIdleState(EnemyController enemy) : base(enemy) { }

    public override void Enter()
    {
    }

    public override void Update()
    {
        if (enemy.IsPlayerInDetectionRange())
        {
            enemy.ChangeState(new RiftBounceState(enemy));
        }

    }

    public override void FixedUpdate() { }
    public override void Exit() { }
}
