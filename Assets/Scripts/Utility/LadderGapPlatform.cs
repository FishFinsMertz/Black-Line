using UnityEngine;

public class LadderGapPlatform : MonoBehaviour
{
    private Collider2D platformCollider; // The platform collider to toggle
    private string playerTag = "Player";

    private void Start()
    {
        if (platformCollider == null)
            platformCollider = GetComponent<Collider2D>();
    }

    private void Update()
    {
        // Find the player (or cache it for performance)
        GameObject player = GameObject.FindGameObjectWithTag(playerTag);
        if (player == null) return;

        PlayerController pc = player.GetComponent<PlayerController>();
        if (pc == null) return;

        // Disable the platform when the player is in the climbing state
        bool isClimbing = pc.GetCurrentState() is PlayerClimbingState;
        platformCollider.enabled = !isClimbing;
    }
}