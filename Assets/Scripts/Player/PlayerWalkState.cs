using UnityEngine;

public class PlayerWalkState : PlayerState
{
    public PlayerWalkState(PlayerController player) : base(player) { }

    public override void Enter()
    {
        //Debug.Log("Entered Walk State");
    }

    public override void FixedUpdate()
    {
        float moveInput = Input.GetAxis("Horizontal");
        Vector2 velocity = player.rb.linearVelocity;
        velocity.x = moveInput * player.walkSpeed;
        //Flip
        if (moveInput > 0 && player.transform.localScale.x < 0)
        {
            player.Flip();
        }
        else if (moveInput < 0 && player.transform.localScale.x > 0)
        {
            player.Flip();
        }
        player.rb.linearVelocity = velocity;

        // Transition back to idle if velocity is approximately zero
        if (Mathf.Approximately(velocity.x, 0f))
        {
            player.ChangeState(new PlayerIdleState(player));
        }
    }

    public override void Exit()
    {
        // Optional: reset velocity when leaving walk state? Idle will handle it.
    }
}