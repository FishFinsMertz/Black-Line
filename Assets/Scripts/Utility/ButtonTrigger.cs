using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Linq;

public class ButtonTrigger : MonoBehaviour, ISaveable, IInteractible
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
    [SerializeField] private bool startInteractible = true;

    [Header("Visual Feedback")]
    [SerializeField] private SpriteRenderer outline;

    [Header("Events")]
    public UnityEvent onInteract;

    private bool playerInRange = false;
    private bool isCooldown = false;
    private bool hasBeenUsed = false;
    private bool interactible = true;

    private void Awake()
    {
        if (outline != null)
            outline.enabled = false;
        interactible = startInteractible;
        if (!interactible)
            enabled = false;
    }

    private void Start()
    {
        SaveManager.Instance?.Register(this);
        RefreshInteraction();
        PlayerController.OnPlayerDeath += DisableInteraction;
    }

    private void OnEnable()
    {
        RefreshInteraction();
    }

    private void OnDisable()
    {
        if (outline != null)
            outline.enabled = false;
        playerInRange = false;
    }

    private void OnDestroy()
    {
        SaveManager.Instance?.Unregister(this);
        PlayerController.OnPlayerDeath -= DisableInteraction;
    }

    private void Update()
    {
        if (!interactible) return;
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
        if (!interactible) return;
        if (oneShot && hasBeenUsed) return;

        isCooldown = true;
        StartCoroutine(CooldownRoutine());

        hasBeenUsed = true;
        onInteract.Invoke();

        if (outline != null)
            outline.enabled = false;
    }

    private IEnumerator CooldownRoutine()
    {
        yield return new WaitForSeconds(cooldown);
        isCooldown = false;
        if (interactible && (!oneShot || !hasBeenUsed))
        {
            if (playerInRange && outline != null)
                outline.enabled = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (oneShot && hasBeenUsed) return;

        playerInRange = true;
        if (interactible && !isCooldown && outline != null)
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
        if (interactible && playerInRange && outline != null)
            outline.enabled = true;
    }

    public void RefreshInteraction()
    {
        Collider2D triggerCollider = GetComponent<Collider2D>();
        if (triggerCollider == null) return;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null) return;

        if (triggerCollider.OverlapPoint(playerObj.transform.position))
        {
            playerInRange = true;
            if (interactible && !(oneShot && hasBeenUsed) && !isCooldown && outline != null)
                outline.enabled = true;
        }
        else
        {
            playerInRange = false;
            if (outline != null)
                outline.enabled = false;
        }
    }

    public void EnableInteraction()
    {
        interactible = true;
        enabled = true;
        RefreshInteraction();
    }

    public void DisableInteraction()
    {
        interactible = false;
        if (outline != null)
            outline.enabled = false;
        playerInRange = false;
        enabled = false;
    }

    public bool IsInteractible() => interactible;

    // --- ISaveable ---
    public void Save(GameData data)
    {
        if (string.IsNullOrEmpty(saveID)) return;
        data.componentStates.RemoveAll(c => c.id == saveID);
        data.componentStates.Add(new ComponentState { id = saveID, state = hasBeenUsed ? "Used" : "Unused" });
    }

    public void Load(GameData data)
    {
        if (string.IsNullOrEmpty(saveID)) return;
        ComponentState cs = data.componentStates.FirstOrDefault(c => c.id == saveID);
        if (cs != null)
        {
            hasBeenUsed = cs.state == "Used";
            if (hasBeenUsed && outline != null)
                outline.enabled = false;
        }
    }
}