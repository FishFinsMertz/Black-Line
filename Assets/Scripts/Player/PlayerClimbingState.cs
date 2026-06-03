using UnityEngine;

public class PlayerClimbingState : PlayerState
{
    private float defaultGravityScale;

    public PlayerClimbingState(PlayerController player) : base(player) { }

    public override void Enter()
    {
        Debug.Log("Entered Climbing State");
        defaultGravityScale = player.rb.gravityScale;
        player.rb.gravityScale = 0f;
        player.rb.linearVelocity = Vector2.zero;

        // Ensure animator speed is normal when starting to climb
        player.bodyAnimator.speed = 1f;
        player.bodyAnimator.SetFloat("Mode", 3f); // default to climb up pose
    }

    public override void FixedUpdate()
    {
        float verticalInput = Input.GetAxisRaw("Vertical");
        player.rb.linearVelocity = new Vector2(0f, verticalInput * player.climbSpeed);

        if (Mathf.Approximately(verticalInput, 0f))
        {
            // No input – freeze the animation
            player.bodyAnimator.speed = 0f;
        }
        else
        {
            // Climbing – resume normal speed and set direction
            player.bodyAnimator.speed = 1f;
            if (verticalInput > 0f)
                player.bodyAnimator.SetFloat("Mode", 3f); // climb up
            else
                player.bodyAnimator.SetFloat("Mode", 4f); // climb down
        }
    }

    public override void Exit()
    {
        Debug.Log("Exited Climbing State");
        // Resume normal animator speed and reset parameter
        player.bodyAnimator.speed = 1f;
        player.bodyAnimator.SetFloat("Mode", 0f);
        player.rb.gravityScale = defaultGravityScale;
    }
}