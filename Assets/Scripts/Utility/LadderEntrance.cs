using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class LadderEntrance : MonoBehaviour
{
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        PlayerController playerController = collision.GetComponent<PlayerController>();
        if (playerController == null) return;

        bool isClimbing = playerController.GetPlayerCurrentState() is PlayerClimbingState;

        if (!isClimbing)
        {
            // Only enter if pressing vertical and NOT pressing horizontal
            float verticalInput = Input.GetAxisRaw("Vertical");
            float horizontalInput = Input.GetAxisRaw("Horizontal");
            if (verticalInput != 0f && horizontalInput == 0f)
                playerController.ChangeState(new PlayerClimbingState(playerController));
        }
        else
        {
            // Only exit if ONLY horizontal is pressed with no vertical
            float verticalInput = Input.GetAxisRaw("Vertical");
            float horizontalInput = Input.GetAxisRaw("Horizontal");
            if (horizontalInput != 0f && verticalInput == 0f)
                playerController.ChangeState(new PlayerIdleState(playerController));
        }
    }
}