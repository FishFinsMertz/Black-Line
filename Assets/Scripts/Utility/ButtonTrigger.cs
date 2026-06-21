using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class ButtonTrigger : MonoBehaviour
{
    public enum TriggerMode
    {
        Press,  // Trigger once when key is pressed down (default)
        Hold    // Trigger repeatedly while key is held (respects cooldown)
    }

    [Header("Interaction Settings")]
    [SerializeField] private TriggerMode mode = TriggerMode.Press;
    [SerializeField] private string interactionKey = "e";
    [SerializeField] private float cooldown = 0.5f;
    [SerializeField] private bool oneShot = false;

    [Header("Visual Feedback")]
    [SerializeField] private SpriteRenderer outline; // optional

    [Header("Events")]
    public UnityEvent onInteract;

    private bool playerInRange = false;
    private bool isCooldown = false;
    private bool hasBeenUsed = false;

    private void Start()
    {
        if (outline != null)
            outline.enabled = false;
    }

    private void Update()
    {
        if (oneShot && hasBeenUsed) return;
        if (!playerInRange) return;
        if (isCooldown) return;

        bool shouldTrigger = false;

        if (mode == TriggerMode.Press)
        {
            shouldTrigger = Input.GetKeyDown(interactionKey);
        }
        else // Hold
        {
            shouldTrigger = Input.GetKey(interactionKey);
        }

        if (shouldTrigger)
        {
            Interact();
        }
    }

    private void Interact()
    {
        if (oneShot && hasBeenUsed) return;

        isCooldown = true;
        StartCoroutine(CooldownRoutine());

        onInteract.Invoke();
        hasBeenUsed = true;

        if (outline != null)
            outline.enabled = false;
    }

    private IEnumerator CooldownRoutine()
    {
        yield return new WaitForSeconds(cooldown);
        isCooldown = false;
        if (playerInRange && outline != null)
            outline.enabled = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (oneShot && hasBeenUsed) return;

        playerInRange = true;
        if (!isCooldown && outline != null)
            outline.enabled = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = false;
        if (outline != null)
            outline.enabled = false;
    }

    // Public method to reset (for respawning)
    public void ResetTrigger()
    {
        hasBeenUsed = false;
        isCooldown = false;
        playerInRange = false;
        if (outline != null)
            outline.enabled = false;
    }
}