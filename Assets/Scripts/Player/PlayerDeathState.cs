using UnityEngine;
using System.Collections;

public class PlayerDeathState : PlayerState
{
    public PlayerDeathState(PlayerController player) : base(player) { }

    public override void Enter()
    {
        player.inventory.ForceDisableAllEquipment();
        player.rb.linearVelocity = Vector2.zero;
        player.bodyAnimator.SetTrigger("Die");

        player.InvokeDeath();
        player.StartCoroutine(player.ReloadAfterDeath());
    }
}