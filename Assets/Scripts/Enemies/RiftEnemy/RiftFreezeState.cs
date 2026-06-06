using UnityEngine;

public class RiftFreezeState : EnemyState
{
    private EnemyController enemy;

    public RiftFreezeState(EnemyController enemy) : base(enemy)
    {
        this.enemy = enemy;
    }

    public override void Enter()
    {

        if (enemy.animator != null)
            enemy.animator.SetTrigger("Freeze");
    }

    public override void Update()
    {
        // Unfreeze if needed (might or might not be needed)
    }

    public override void FixedUpdate() { }

    public override void Exit()
    {

    }
}
