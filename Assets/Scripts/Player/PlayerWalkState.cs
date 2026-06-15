using UnityEngine;

public class PlayerWalkState : PlayerState
{
    public PlayerWalkState(PlayerController player) : base(player) { }

    public override void Enter() { }

    public override void FixedUpdate()
    {
        float moveInput = Input.GetAxis("Horizontal");
        bool isFacingRight = player.isFacingRight;
        bool movingForward = (moveInput > 0 && isFacingRight) || (moveInput < 0 && !isFacingRight);
        
        if (movingForward && Input.GetKey(player.runKey))
        {
            player.currentSubState = PlayerController.SubState.None;
            player.ChangeState(new PlayerRunState(player));
            return;
        }

        player.currentSubState = PlayerController.SubState.WalkBack;
        
        Vector2 velocity = player.rb.linearVelocity;
        velocity.x = moveInput * player.walkSpeed;
        player.rb.linearVelocity = velocity;
        
        float relativeSpeed = moveInput;
        if (!isFacingRight) relativeSpeed = -relativeSpeed;
        player.bodyAnimator.SetFloat("Mode", relativeSpeed);
        
        if (Mathf.Approximately(moveInput, 0f))
            player.ChangeState(new PlayerIdleState(player));
    }

    public override void Exit()
    {
        player.currentSubState = PlayerController.SubState.None;
    }
}