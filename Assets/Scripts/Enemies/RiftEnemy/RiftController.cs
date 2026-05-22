using UnityEngine;

public class RiftController : EnemyController
{
    protected override void Start()
    {
        base.Start();
        ChangeState(new RiftIdleState(this));
    }
}
