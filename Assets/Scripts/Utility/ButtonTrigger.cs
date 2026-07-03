using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Linq;

public class ButtonTrigger : MonoBehaviour, ISaveable
{
    [Header("Save ID (unique per component)")]
    [SerializeField] private string saveID;

    public enum TriggerMode
    {
        Press,
        Hold
    }

    [Header("Interaction Settings")]
    [SerializeField] private TriggerMode mode = TriggerMode.Press;
    [SerializeField] private string interactionKey = "e";
    [SerializeField] private float cooldown = 0.5f;
    [SerializeField] private bool oneShot = false;

    [Header("Visual Feedback")]
    [SerializeField] private SpriteRenderer outline;

    [Header("Events")]
    public UnityEvent onInteract;

    private bool playerInRange = false;
    private bool isCooldown = false;
    private bool hasBeenUsed = false;

    private void Start()
    {
        SaveManager.Instance?.Register(this);
        if (outline != null)
            outline.enabled = false;
    }

    private void OnDestroy()
    {
        SaveManager.Instance?.Unregister(this);
    }

    private void Update()
    {
        if (oneShot && hasBeenUsed) return;
        if (!playerInRange) return;
        if (isCooldown) return;

        bool shouldTrigger = false;
        if (mode == TriggerMode.Press)
            shouldTrigger = Input.GetKeyDown(interactionKey);
        else
            shouldTrigger = Input.GetKey(interactionKey);

        if (shouldTrigger)
            Interact();
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

    public void ResetTrigger()
    {
        hasBeenUsed = false;
        isCooldown = false;
        playerInRange = false;
        if (outline != null)
            outline.enabled = false;
    }

    // --- ISaveable ---
    public void Save(GameData data)
    {
        if (!oneShot || string.IsNullOrEmpty(saveID)) return;

        data.componentStates.RemoveAll(c => c.id == saveID);
        data.componentStates.Add(new ComponentState { id = saveID, state = hasBeenUsed ? "Used" : "Unused" });
    }

    public void Load(GameData data)
    {
        if (!oneShot || string.IsNullOrEmpty(saveID)) return;

        ComponentState cs = data.componentStates.FirstOrDefault(c => c.id == saveID);
        if (cs != null)
        {
            hasBeenUsed = cs.state == "Used";
            if (hasBeenUsed && outline != null)
                outline.enabled = false;
        }
    }
}