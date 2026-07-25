using UnityEngine;
using UnityEngine.Events;
using System.Linq;

public class TriggerEvent : MonoBehaviour, ISaveable, IInteractible
{
    [Header("Save ID (optional, only needed if oneShot is true)")]
    [SerializeField] private string saveID;

    [Header("Settings")]
    [SerializeField] private bool oneShot = true;
    [SerializeField] private string requiredTag = "Player";
    [SerializeField] private bool interactible = true;
    [SerializeField] private bool checkPlayerInsideOnEnable = true;

    [Header("Events")]
    public UnityEvent onTriggerEnter;
    public UnityEvent onTriggerExit;

    private bool hasTriggered = false;
    private Collider2D triggerCollider;

    private void Start()
    {
        if (oneShot && !string.IsNullOrEmpty(saveID))
            SaveManager.Instance?.Register(this);

        triggerCollider = GetComponent<Collider2D>();
    }

    private void OnEnable()
    {
        if (checkPlayerInsideOnEnable && (!oneShot || (oneShot && !hasTriggered)))
        {
            CheckPlayerInside();
        }
    }

    private void OnDestroy()
    {
        if (oneShot && !string.IsNullOrEmpty(saveID))
            SaveManager.Instance?.Unregister(this);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!interactible) return;
        if (oneShot && hasTriggered) return;
        if (!other.CompareTag(requiredTag)) return;

        onTriggerEnter.Invoke();
        if (oneShot)
            hasTriggered = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!interactible) return;
        if (oneShot && hasTriggered) return;
        if (!other.CompareTag(requiredTag)) return;

        Debug.Log("Left");

        onTriggerExit.Invoke();
    }

    private void CheckPlayerInside()
    {
        if (triggerCollider == null)
        {
            triggerCollider = GetComponent<Collider2D>();
            if (triggerCollider == null) return;
        }

        if (!triggerCollider.enabled || !gameObject.activeInHierarchy)
        {
            return;
        }

        GameObject player = GameObject.FindGameObjectWithTag(requiredTag);
        if (player == null) return;

        Collider2D playerCollider = player.GetComponent<Collider2D>();
        if (playerCollider == null) return;

        // Method 1: Using Overlap (preferred)
        ContactFilter2D filter = new ContactFilter2D();
        filter.useTriggers = true;
        filter.SetLayerMask(1 << player.layer);

        Collider2D[] results = new Collider2D[1];
        int count = triggerCollider.Overlap(filter, results);

        bool playerInside = count > 0 && results[0].CompareTag(requiredTag);

        if (!playerInside)
        {
            Vector2 playerCenter = playerCollider.bounds.center;
            Vector2 closestPoint = triggerCollider.ClosestPoint(playerCenter);
            float distance = Vector2.Distance(closestPoint, playerCenter);
            if (distance < 0.01f)
            {
                playerInside = true;
            }
        }

        if (playerInside)
        {
            onTriggerEnter.Invoke();
            if (oneShot)
                hasTriggered = true;
        }
    }

    // --- IInteractible ---
    public void EnableInteraction()
    {
        interactible = true;
        CheckPlayerInside();
    }

    public void DisableInteraction()
    {
        interactible = false;
    }

    // --- ISaveable ---
    public void Save(GameData data)
    {
        if (!oneShot || string.IsNullOrEmpty(saveID)) return;
        data.componentStates.RemoveAll(c => c.id == saveID);
        string state = $"{hasTriggered}:{interactible}";
        data.componentStates.Add(new ComponentState { id = saveID, state = state });
    }

    public void Load(GameData data)
    {
        if (!oneShot || string.IsNullOrEmpty(saveID)) return;
        ComponentState cs = data.componentStates.FirstOrDefault(c => c.id == saveID);
        if (cs != null)
        {
            string[] parts = cs.state.Split(':');
            if (parts.Length == 2)
            {
                hasTriggered = parts[0] == "True";
                interactible = parts[1] == "True";
            }
            else
            {
                hasTriggered = cs.state == "Triggered";
            }
        }
    }
}