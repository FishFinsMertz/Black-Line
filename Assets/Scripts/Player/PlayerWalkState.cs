using UnityEngine;

public class PlayerWalkState : PlayerState
{
    public PlayerWalkState(PlayerController player) : base(player) { }

    public override void Enter() { }

    public override void FixedUpdate()
    {
        float moveInput = Input.GetAxis("Horizontal");
        Vector2 velocity = player.rb.linearVelocity;
        velocity.x = moveInput * player.walkSpeed;
        player.rb.linearVelocity = velocity;

        // No flip logic here – the gun controller (ArmGunController) handles flipping based on mouse position.

        // Calculate relative speed for blend tree:
        bool isFacingRight = player.transform.localScale.x > 0;
        float relativeSpeed = (moveInput > 0 && isFacingRight || moveInput < 0 && !isFacingRight)
            ? Mathf.Abs(moveInput)
            : -Mathf.Abs(moveInput);
        player.bodyAnimator.SetFloat("SpeedX", relativeSpeed);

        // Transition to idle when no input
        if (Mathf.Approximately(moveInput, 0f))
        {
            player.ChangeState(new PlayerIdleState(player));
        }
    }

    public override void Exit() { }
}