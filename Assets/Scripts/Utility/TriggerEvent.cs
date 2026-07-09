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

    [Header("Events")]
    public UnityEvent onTriggerEnter;

    private bool hasTriggered = false;

    private void Start()
    {
        if (oneShot && !string.IsNullOrEmpty(saveID))
            SaveManager.Instance?.Register(this);
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

    // --- IInteractible ---
    public void EnableInteraction()
    {
        interactible = true;
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
        // Save both triggered state and interactible state
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
                // Fallback for older save format
                hasTriggered = cs.state == "Triggered";
            }
        }
    }
}