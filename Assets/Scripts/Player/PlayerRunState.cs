using UnityEngine;

public class PlayerRunState : PlayerState
{
    public PlayerRunState(PlayerController player) : base(player) { }

    public override void Enter()
    {
        player.PlayLoopSound(player.runSound);
    }

    public override void FixedUpdate()
    {
        float moveInput = Input.GetAxis("Horizontal");
        bool isFacingRight = player.isFacingRight;
        bool movingForward = (moveInput > 0 && isFacingRight) || (moveInput < 0 && !isFacingRight);
        float targetSpeed = movingForward && Input.GetKey(player.runKey) ? player.runSpeed : player.walkSpeed;

        float currentVelX = player.rb.linearVelocity.x;
        float targetVelX = moveInput * targetSpeed;
        float accel = (Mathf.Abs(targetVelX) > Mathf.Abs(currentVelX)) ? player.runAcceleration : player.runDeceleration;
        float newVelX = Mathf.MoveTowards(currentVelX, targetVelX, accel * Time.fixedDeltaTime);
        player.rb.linearVelocity = new Vector2(newVelX, player.rb.linearVelocity.y);

        if (!movingForward)
        {
            player.ChangeState(new PlayerWalkState(player));
            return;
        }
        if (!Input.GetKey(player.runKey))
        {
            player.ChangeState(new PlayerWalkState(player));
            return;
        }
        if (Mathf.Approximately(moveInput, 0f))
        {
            player.ChangeState(new PlayerIdleState(player));
            return;
        }

        float relativeSpeed = moveInput;
        if (!isFacingRight) relativeSpeed = -relativeSpeed;
        float animSpeed = Mathf.Abs(relativeSpeed) > 0 ? Mathf.Sign(relativeSpeed) * 2.0f : 0f;
        player.bodyAnimator.SetFloat("Mode", animSpeed);
    }

    public override void Exit()
    {
        player.StopLoopSound();
    }
}