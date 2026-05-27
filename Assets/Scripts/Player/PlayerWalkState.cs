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
        
        // Transition to Run if moving forward AND run key held
        if (movingForward && Input.GetKey(player.runKey))
        {
            player.ChangeState(new PlayerRunState(player));
            return;
        }
        
        // rest of walk logic
        Vector2 velocity = player.rb.linearVelocity;
        velocity.x = moveInput * player.walkSpeed;
        player.rb.linearVelocity = velocity;
        
        // Set animator speed (normalized walk speed)
        float relativeSpeed = moveInput;
        if (!isFacingRight) relativeSpeed = -relativeSpeed;
        player.bodyAnimator.SetFloat("SpeedX", relativeSpeed);
        
        if (Mathf.Approximately(moveInput, 0f))
            player.ChangeState(new PlayerIdleState(player));
    }

    public override void Exit() { }
}