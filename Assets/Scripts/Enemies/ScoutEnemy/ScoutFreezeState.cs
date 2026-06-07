using UnityEngine;

public class ScoutFreezeState : EnemyState
{
    private ScoutController scout;
    private float freezeEndTime;
    private float unfreezeStartTime;
    private bool isUnfreezing;

    public ScoutFreezeState(ScoutController scout) : base(scout)
    {
        this.scout = scout;
    }

    public override void Enter()
    {
        // Stop movement
        scout.rb.linearVelocity = Vector2.zero;
        scout.rb.gravityScale = 1f;

        // Calculate when freeze should end (using frozenTime from EnemyController)
        freezeEndTime = Time.time + scout.frozenTime;

        // Play freeze animation
        if (scout.animator != null)
            scout.animator.SetTrigger("Freeze");

        isUnfreezing = false;
    }

    public override void Update()
    {
        if (!isUnfreezing)
        {
            // Wait until freeze duration is over
            if (Time.time >= freezeEndTime)
            {
                isUnfreezing = true;
                if (scout.animator != null)
                    scout.animator.SetTrigger("Unfreeze");
                unfreezeStartTime = Time.time;
            }
        }
        else
        {
            // Wait for unfreeze animation to complete (approx 1 second)
            if (Time.time >= unfreezeStartTime + 1f)
            {
                // Return to hover state
                scout.ChangeState(new ScoutHoverState(scout));
            }
        }
    }

    public override void FixedUpdate() { }
    public override void Exit() { }
}