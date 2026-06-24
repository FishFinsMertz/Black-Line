using UnityEngine;
using UnityEngine.Events;
using System.Linq;

public class TriggerEvent : MonoBehaviour, ISaveable
{
    [Header("Save ID (optional, only needed if oneShot is true)")]
    [SerializeField] private string saveID;

    [Header("Settings")]
    [SerializeField] private bool oneShot = true;
    [SerializeField] private string requiredTag = "Player";

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
        if (oneShot && hasTriggered) return;
        if (!other.CompareTag(requiredTag)) return;

        onTriggerEnter.Invoke();
        if (oneShot)
            hasTriggered = true;
    }

    // --- ISaveable ---
    public void Save(GameData data)
    {
        if (!oneShot || string.IsNullOrEmpty(saveID)) return;
        data.componentStates.RemoveAll(c => c.id == saveID);
        data.componentStates.Add(new ComponentState { id = saveID, state = hasTriggered ? "Triggered" : "NotTriggered" });
    }

    public void Load(GameData data)
    {
        if (!oneShot || string.IsNullOrEmpty(saveID)) return;
        ComponentState cs = data.componentStates.FirstOrDefault(c => c.id == saveID);
        if (cs != null)
            hasTriggered = cs.state == "Triggered";
    }
}