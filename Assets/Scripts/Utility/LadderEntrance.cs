using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class LadderEntrance : MonoBehaviour
{
    [SerializeField] public bool isTop = false;

    private PlayerController playerInRange = null;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;
        PlayerController playerController = collision.GetComponent<PlayerController>();
        if (playerController == null) return;

        playerInRange = playerController;

        // If already climbing, update the entrance flags
        if (playerController.GetPlayerCurrentState() is PlayerClimbingState climbState)
            climbState.OnEntranceTouched(isTop, true);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;
        PlayerController playerController = collision.GetComponent<PlayerController>();
        if (playerController == null) return;

        if (playerController == playerInRange)
            playerInRange = null;

        if (playerController.GetPlayerCurrentState() is PlayerClimbingState climbState)
            climbState.OnEntranceTouched(isTop, false);
    }

    // Public method (for ButtonTrigger)
    public void EnterLadder()
    {
        if (playerInRange == null) return;
        if (playerInRange.GetPlayerCurrentState() is PlayerClimbingState) return; // already climbing

        PlayerClimbingState newState = new PlayerClimbingState(playerInRange, isTop);
        newState.OnEntranceTouched(isTop, true);
        playerInRange.ChangeState(newState);
    }
}