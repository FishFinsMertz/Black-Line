using UnityEngine;

public class PlayerIdleState : PlayerState
{
    public PlayerIdleState(PlayerController player) : base(player) { }

    public override void Enter()
    {
        player.rb.linearVelocity = Vector2.zero;
        player.bodyAnimator.SetFloat("Mode", 0.0f);
    }

    public override void Update()
    {
        float moveInput = Input.GetAxis("Horizontal");
        if (!Mathf.Approximately(moveInput, 0f))
        {
            player.ChangeState(new PlayerWalkState(player));
        }
    }

    public override void FixedUpdate() { }
    public override void Exit() { }
}