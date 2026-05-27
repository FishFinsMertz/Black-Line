using UnityEngine;

public class PlayerRunState : PlayerState
{
    public PlayerRunState(PlayerController player) : base(player) { }

    public override void Enter()
    {
        Debug.Log("Entered Run State");
    }

    public override void FixedUpdate()
    {
        float moveInput = Input.GetAxis("Horizontal");
        bool isFacingRight = player.isFacingRight;
        bool movingForward = (moveInput > 0 && isFacingRight) || (moveInput < 0 && !isFacingRight);
        float targetSpeed = movingForward && Input.GetKey(player.runKey) ? player.runSpeed : player.walkSpeed;

        // Apply acceleration/deceleration towards target horizontal speed
        float currentVelX = player.rb.linearVelocity.x;
        float targetVelX = moveInput * targetSpeed;
        float accel = (Mathf.Abs(targetVelX) > Mathf.Abs(currentVelX)) ? player.runAcceleration : player.runDeceleration;
        float newVelX = Mathf.MoveTowards(currentVelX, targetVelX, accel * Time.fixedDeltaTime);
        player.rb.linearVelocity = new Vector2(newVelX, player.rb.linearVelocity.y);

        // Condition to leave run state:
        // 1. Not moving forward (e.g., moving backwards)
        // 2. Run key released while moving forward (then transition to walk)
        // 3. No input (idle)
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

        // Set animator speed (2.0 for run, 0.5 for walk, etc.)
        float relativeSpeed = moveInput;
        if (!isFacingRight) relativeSpeed = -relativeSpeed;
        float animSpeed = Mathf.Abs(relativeSpeed) > 0 ? Mathf.Sign(relativeSpeed) * 2.0f : 0f;
        player.bodyAnimator.SetFloat("SpeedX", animSpeed);
    }

    public override void Exit() { }
}