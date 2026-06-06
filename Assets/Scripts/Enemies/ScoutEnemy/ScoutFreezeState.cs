using UnityEngine;

public class ScoutFreezeState : EnemyState
{
    private ScoutController scout;

    public ScoutFreezeState(ScoutController scout) : base(scout)
    {
        this.scout = scout;
    }

    public override void Enter()
    {
        // Stop movement and play freeze animation
        scout.rb.linearVelocity = Vector2.zero;
        scout.rb.gravityScale = 1f;

        if (scout.animator != null)
            scout.animator.SetTrigger("Freeze");
    }

    public override void Update()
    {
        // Unfreeze if needed (might or might not be needed)
    }

    public override void FixedUpdate() { }

    public override void Exit()
    {
        // Reset any freeze-specific states if needed
    }
}
