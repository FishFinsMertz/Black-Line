using UnityEngine;

public class LadderGapPlatform : MonoBehaviour
{
    private Collider2D platformCollider;
    private string playerTag = "Player";

    private void Start()
    {
        if (platformCollider == null)
            platformCollider = GetComponent<Collider2D>();
    }

    private void Update()
    {
        GameObject player = GameObject.FindGameObjectWithTag(playerTag);
        if (player == null) return;

        PlayerController pc = player.GetComponent<PlayerController>();
        if (pc == null) return;

        bool isClimbing = pc.GetCurrentState() is PlayerClimbingState;
        platformCollider.enabled = !isClimbing;
    }
}