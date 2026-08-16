using UnityEngine;
using System.Collections;

public class PlayerDeathState : PlayerState
{
    public PlayerDeathState(PlayerController player) : base(player) { }

    public override void Enter()
    {
        player.inventory.DisableAllEquipment();
        player.rb.linearVelocity = Vector2.zero;
        player.bodyAnimator.SetTrigger("Die");

        player.StartCoroutine(player.ReloadAfterDeath());
    }
}