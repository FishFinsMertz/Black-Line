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
    }

    public override void Update() { }

    public override void FixedUpdate()
    {
        float verticalInput = Input.GetAxisRaw("Vertical");

        player.rb.gravityScale = 0f;
        player.rb.linearVelocity = new Vector2(0f, verticalInput * player.climbSpeed);
    }

    public override void Exit()
    {
        Debug.Log("Exited Climbing State");
        // Restore gravity when leaving climb state
        player.rb.gravityScale = defaultGravityScale;
    }
}