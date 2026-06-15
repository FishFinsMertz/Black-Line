using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class LadderEntrance : MonoBehaviour
{
    [SerializeField] public bool isTop = false;

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;
        PlayerController playerController = collision.GetComponent<PlayerController>();
        if (playerController == null) return;

        bool isClimbing = playerController.GetPlayerCurrentState() is PlayerClimbingState;

        if (!isClimbing)
        {
            float verticalInput = Input.GetAxisRaw("Vertical");
            float horizontalInput = Input.GetAxisRaw("Horizontal");
            bool validMount = isTop ? verticalInput < 0f : verticalInput > 0f;
            if (validMount && horizontalInput == 0f)
            {
                PlayerClimbingState newState = new PlayerClimbingState(playerController, isTop);
                newState.OnEntranceTouched(isTop, true);
                playerController.ChangeState(newState);
            }
        }
        // Exit condition is handled inside PlayerClimbingState.Update
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;
        PlayerController playerController = collision.GetComponent<PlayerController>();
        if (playerController == null) return;

        if (playerController.GetPlayerCurrentState() is PlayerClimbingState climbState)
            climbState.OnEntranceTouched(isTop, true);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;
        PlayerController playerController = collision.GetComponent<PlayerController>();
        if (playerController == null) return;

        if (playerController.GetPlayerCurrentState() is PlayerClimbingState climbState)
            climbState.OnEntranceTouched(isTop, false);
    }
}