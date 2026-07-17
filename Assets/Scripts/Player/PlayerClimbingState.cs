using UnityEngine;

public class PlayerClimbingState : PlayerState
{
    private float defaultGravityScale;
    private Inventory.EquipmentType previousEquipment;

    private bool atTop = false;
    private bool atBottom = false;
    private bool startedFromTop;

    public PlayerClimbingState(PlayerController player, bool fromTop) : base(player)
    {
        startedFromTop = fromTop;
    }

    public void OnEntranceTouched(bool isTop, bool entered)
    {
        if (isTop) atTop = entered;
        else atBottom = entered;
    }

    public override void Enter()
    {
        previousEquipment = player.inventory.GetCurrentEquipment();
        player.inventory.EquipType(Inventory.EquipmentType.None, true);
        defaultGravityScale = player.rb.gravityScale;
        player.rb.gravityScale = 0f;
        player.rb.linearVelocity = Vector2.zero;
        player.bodyAnimator.speed = 1f;

        if (player.audioSource != null && player.climbSound != null)
        {
            player.audioSource.clip = player.climbSound;
            player.audioSource.loop = true;
        }

        if (startedFromTop)
        {
            player.currentSubState = PlayerController.SubState.ClimbDown;
            player.bodyAnimator.SetFloat("Mode", 4f);
            player.rb.linearVelocity = new Vector2(0f, -player.climbSpeed);
            if (player.audioSource != null && !player.audioSource.isPlaying)
                player.audioSource.Play();
        }
        else
        {
            player.currentSubState = PlayerController.SubState.None;
            player.bodyAnimator.SetFloat("Mode", 3f);
            player.rb.linearVelocity = new Vector2(0f, player.climbSpeed);
            if (player.audioSource != null && !player.audioSource.isPlaying)
                player.audioSource.Play();
        }
    }

    public override void Update()
    {
        if ((atTop || atBottom) && Input.GetAxisRaw("Horizontal") != 0f)
        {
            player.ChangeState(new PlayerIdleState(player));
        }
    }

    public override void FixedUpdate()
    {
        float verticalInput = Input.GetAxisRaw("Vertical");

        bool blockedUp   = atTop    && verticalInput > 0f;
        bool blockedDown = atBottom && verticalInput < 0f;
        bool blocked = blockedUp || blockedDown;

        bool moving = !Mathf.Approximately(verticalInput, 0f) && !blocked;

        if (moving)
        {
            player.rb.linearVelocity = new Vector2(0f, verticalInput * player.climbSpeed);
            player.bodyAnimator.speed = 1f;

            if (verticalInput > 0f)
            {
                player.currentSubState = PlayerController.SubState.None;
                player.bodyAnimator.SetFloat("Mode", 3f);
            }
            else
            {
                player.currentSubState = PlayerController.SubState.ClimbDown;
                player.bodyAnimator.SetFloat("Mode", 4f);
            }

            if (player.audioSource != null && !player.audioSource.isPlaying)
                player.audioSource.Play();
        }
        else
        {
            player.rb.linearVelocity = Vector2.zero;
            player.currentSubState = PlayerController.SubState.ClimbPause;
            player.bodyAnimator.speed = 0f;

            if (player.audioSource != null && player.audioSource.isPlaying)
                player.audioSource.Stop();
        }
    }

    public override void Exit()
    {
        player.bodyAnimator.speed = 1f;
        player.bodyAnimator.SetFloat("Mode", 0f);
        player.rb.gravityScale = defaultGravityScale;
        player.currentSubState = PlayerController.SubState.None;
        player.inventory.EquipType(previousEquipment, true);
        if (player.audioSource != null && player.audioSource.isPlaying)
        {
            player.audioSource.Stop();
        }
    }
}