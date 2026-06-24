using UnityEngine;
using System.Collections;
using System.Linq;

public class Valve : MonoBehaviour, ISaveable
{
    [Header("Save ID (unique per component)")]
    [SerializeField] private string saveID; // e.g., "valve_room1"

    [Header("Visual State")]
    [SerializeField] private Animator animator;
    [SerializeField] private string openTrigger = "Open";
    [SerializeField] private string closeTrigger = "Close";
    [SerializeField] private string idleBool = "isIdle";
    [SerializeField] private float animationDuration = 0.5f;

    [Header("State")]
    [SerializeField] private bool startOpen = false;

    [Header("Events")]
    public UnityEngine.Events.UnityEvent onOpen;
    public UnityEngine.Events.UnityEvent onClose;

    private bool isOpen;
    private Coroutine idleCoroutine;

    private void Start()
    {
        SaveManager.Instance?.Register(this);
        isOpen = startOpen;
        ApplyStateInstant(isOpen);
        if (animator != null)
            animator.SetBool(idleBool, true);
    }

    private void OnDestroy()
    {
        SaveManager.Instance?.Unregister(this);
    }

    // Public API
    public void Toggle()
    {
        if (isOpen)
            Close();
        else
            Open();
    }

    public void Open()
    {
        if (isOpen) return;
        isOpen = true;

        if (idleCoroutine != null)
            StopCoroutine(idleCoroutine);

        if (animator != null)
        {
            animator.SetBool(idleBool, false);
            animator.SetTrigger(openTrigger);
        }

        onOpen.Invoke();
        idleCoroutine = StartCoroutine(ReturnToIdle());
    }

    public void Close()
    {
        if (!isOpen) return;
        isOpen = false;

        if (idleCoroutine != null)
            StopCoroutine(idleCoroutine);

        if (animator != null)
        {
            animator.SetBool(idleBool, false);
            animator.SetTrigger(closeTrigger);
        }

        onClose.Invoke();
        idleCoroutine = StartCoroutine(ReturnToIdle());
    }

    private IEnumerator ReturnToIdle()
    {
        yield return new WaitForSeconds(animationDuration + 0.1f);
        if (animator != null)
            animator.SetBool(idleBool, true);
        idleCoroutine = null;
    }

    private void ApplyStateInstant(bool open)
    {
        if (animator != null)
            animator.SetBool(idleBool, true);
        // If you have distinct "Open" and "Closed" states, you could force them:
        // animator.Play(open ? "Open" : "Closed", 0, 0f);
    }

    public bool IsOpen() => isOpen;

    // --- ISaveable ---
    public void Save(GameData data)
    {
        data.componentStates.RemoveAll(c => c.id == saveID);
        data.componentStates.Add(new ComponentState { id = saveID, state = isOpen ? "Open" : "Closed" });
    }

    public void Load(GameData data)
    {
        ComponentState cs = data.componentStates.FirstOrDefault(c => c.id == saveID);
        if (cs != null)
        {
            isOpen = cs.state == "Open";
            // Optional: update visual to match loaded state without animation
            if (animator != null)
                animator.SetBool(idleBool, true);
            // If you need to force a specific frame, you can call animator.Play()
            // animator.Play(isOpen ? "Open" : "Closed", 0, 0f);
        }
    }
}