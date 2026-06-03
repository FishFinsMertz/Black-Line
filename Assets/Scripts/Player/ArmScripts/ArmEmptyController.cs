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

        if (player == null) return;

        PlayerState currentState = player.GetCurrentState();

        // Reset animator speed to normal by default (most states)
        armEmptyAnimator.speed = 1f;

        // Handle climbing state separately (because of pause substate)
        if (currentState is PlayerClimbingState)
        {
            switch (player.currentSubState)
            {
                case PlayerController.SubState.ClimbDown:
                    armEmptyAnimator.SetFloat("Mode", 4f);  // climb down animation
                    armEmptyAnimator.speed = 1f;
                    break;
                case PlayerController.SubState.ClimbPause:
                    // Freeze animation on current frame
                    armEmptyAnimator.speed = 0f;
                    break;
                default: // Climb up (no substate or other)
                    armEmptyAnimator.SetFloat("Mode", 3f);  // climb up animation
                    armEmptyAnimator.speed = 1f;
                    break;
            }
            return;
        }

        // Non‑climbing states
        if (currentState is PlayerIdleState)
        {
            armEmptyAnimator.SetFloat("Mode", 0f);
        }
        else if (currentState is PlayerWalkState && player.currentSubState == PlayerController.SubState.WalkBack)
        {
            armEmptyAnimator.SetFloat("Mode", -1f);  // walk backward
        }
        else if (currentState is PlayerWalkState)
        {
            armEmptyAnimator.SetFloat("Mode", 1f);   // walk forward
        }
        else if (currentState is PlayerRunState)
        {
            armEmptyAnimator.SetFloat("Mode", 2f);   // run forward
        }
        else
        {
            // Fallback for any unhandled states
            armEmptyAnimator.SetFloat("Mode", 0f);
        }
    }
}