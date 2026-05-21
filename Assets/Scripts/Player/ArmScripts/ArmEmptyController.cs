using Unity.Burst.Intrinsics;
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
        if (currentPlayerState != null)
        {
            // Set animator parameters based on player state
            if (currentPlayerState is PlayerIdleState)
            {
                armEmptyAnimator.SetBool("isIdle", true);
                armEmptyAnimator.SetBool("isWalking", false);
            }
            else if (currentPlayerState is PlayerWalkState)
            {
                armEmptyAnimator.SetBool("isIdle", false);
                armEmptyAnimator.SetBool("isWalking", true);
            }
            else
            {
                armEmptyAnimator.SetBool("isIdle", false);
                armEmptyAnimator.SetBool("isWalking", false);
            }
        }
    }
}
