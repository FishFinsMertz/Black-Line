using UnityEngine;

public class PlayerIdleState : PlayerState
{
    public PlayerIdleState(PlayerController player) : base(player) { }

    public override void Enter()
    {
        Debug.Log("Entered Idle State");
        // Stop all movement when entering idle
        player.rb.linearVelocity = Vector2.zero;
    }

    public override void Update()
    {
        float moveInput = Input.GetAxis("Horizontal");
        if (!Mathf.Approximately(moveInput, 0f))
        {
            player.ChangeState(new PlayerWalkState(player));
        }
    }

    public override void FixedUpdate()
    {
        // No physics changes needed – velocity already zero from Enter
    }

    public override void Exit() { }
}