using System;
using UnityEngine;

public class RiftBounceState : EnemyState
{
    private RiftController rift;
    private float delayTimer;
    private bool delayed;

    public RiftBounceState(EnemyController enemy) : base(enemy) 
    { 
        rift = (RiftController)enemy;
    }
    public override void Enter()
    {
         // Set up delay before first bounce
        delayTimer = rift.bounceDelay;
        delayed = false;
        rift.rb.linearVelocity = Vector2.zero;
    }

    public override void Update()
    {
        if (!delayed)
        {
            delayTimer -= Time.deltaTime;
            if (delayTimer <= 0f)
            {
                delayed = true;
                LaunchBounce();
            }
            return;
        }

    }
    public override void FixedUpdate()
    {

    }

    public override void Exit()
    {

    }

    private void LaunchBounce()
    {
    }
}
