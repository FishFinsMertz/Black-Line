using UnityEngine;

public class ArmEmptyController : ArmController
{
    private Animator armEmptyAnimator;

    protected override void Start()
    {
        base.Start();
        armEmptyAnimator = GetComponent<Animator>();
    }

    protected override void Update()
    {
        base.Update();

        if (player != null)
        {
            PlayerState currentState = player.GetCurrentState();

            if (currentState is PlayerIdleState)
            {
                armEmptyAnimator.SetBool("isIdle", true);
                armEmptyAnimator.SetBool("isWalking", false);
                armEmptyAnimator.SetBool("isRunning", false);
            }
            else if (currentState is PlayerWalkState)
            {
                armEmptyAnimator.SetBool("isIdle", false);
                armEmptyAnimator.SetBool("isWalking", true);
                armEmptyAnimator.SetBool("isRunning", false);
            }
            else if (currentState is PlayerRunState)
            {
                armEmptyAnimator.SetBool("isIdle", false);
                armEmptyAnimator.SetBool("isWalking", false);
                armEmptyAnimator.SetBool("isRunning", true);
            }
            else
            {
                // Fallback for any other state (should not happen normally)
                armEmptyAnimator.SetBool("isIdle", false);
                armEmptyAnimator.SetBool("isWalking", false);
                armEmptyAnimator.SetBool("isRunning", false);
            }
        }
    }
}