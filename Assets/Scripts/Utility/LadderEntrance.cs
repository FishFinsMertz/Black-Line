using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class LadderEntrance : MonoBehaviour, IInteractible
{
    [SerializeField] public bool isTop = false;

    private PlayerController playerInRange = null;
    private bool isInteractible = true;

    private void OnEnable()
    {
        PlayerController.OnPlayerDeath += DisableInteraction;
    }

    private void OnDisable()
    {
        PlayerController.OnPlayerDeath -= DisableInteraction;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;
        if (!isInteractible) return;
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

    public void EnterLadder()
    {
        if (!isInteractible) return;
        if (playerInRange == null) return;
        if (playerInRange.GetPlayerCurrentState() is PlayerClimbingState) return; // already climbing

        PlayerClimbingState newState = new PlayerClimbingState(playerInRange, isTop);
        newState.OnEntranceTouched(isTop, true);
        playerInRange.ChangeState(newState);
    }

    public void EnableInteraction()
    {
        isInteractible = true;
    }

    public void DisableInteraction()
    {
        isInteractible = false;
        playerInRange = null;
    }
}