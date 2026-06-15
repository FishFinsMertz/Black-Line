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
        scout.rb.linearVelocity = Vector2.zero;
        scout.rb.gravityScale = 1f;
        freezeEndTime = Time.time + scout.frozenTime;

        if (scout.animator != null)
            scout.animator.SetTrigger("Freeze");

        isUnfreezing = false;
    }

    public override void Update()
    {
        if (!isUnfreezing)
        {
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
            if (Time.time >= unfreezeStartTime + 1f)
            {
                scout.ChangeState(new ScoutHoverState(scout));
            }
        }
    }

    public override void FixedUpdate() { }
    public override void Exit() { }
}