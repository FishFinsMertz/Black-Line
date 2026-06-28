using UnityEngine;

public class LadderGapPlatform : MonoBehaviour, IInteractible
{
    private Collider2D platformCollider;
    private string playerTag = "Player";
    private bool isInteractionEnabled = true;

    private void Awake()
    {
        if (platformCollider == null)
            platformCollider = GetComponent<Collider2D>();
    }

    private void Update()
    {
        if (!isInteractionEnabled) return;

        GameObject player = GameObject.FindGameObjectWithTag(playerTag);
        if (player == null) return;

        PlayerController pc = player.GetComponent<PlayerController>();
        if (pc == null) return;

        bool isClimbing = pc.GetCurrentState() is PlayerClimbingState;
        platformCollider.enabled = !isClimbing;
    }

    // --- IInteractible implementation ---
    public void EnableInteraction()
    {
        isInteractionEnabled = true;
        if (platformCollider != null)
            platformCollider.enabled = false;
    }

    public void DisableInteraction()
    {
        isInteractionEnabled = false;
        if (platformCollider != null)
            platformCollider.enabled = true;
    }
}