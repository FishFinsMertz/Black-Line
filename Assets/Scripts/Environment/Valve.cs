using UnityEngine;
using System.Collections;
using System.Linq;

public class Valve : MonoBehaviour, ISaveable
{
    public enum ValveMode { Toggle, Rotate }

    [SerializeField] private string saveID;
    [SerializeField] private ValveMode mode = ValveMode.Toggle;
    [SerializeField] private Animator animator;
    [SerializeField] private string openTrigger = "Open";
    [SerializeField] private string closeTrigger = "Close";
    [SerializeField] private string idleBool = "isIdle";
    [SerializeField] private float animationDuration = 0.5f;
    [SerializeField] private bool startOpen = false;

    [Header("Audio")]
    [SerializeField] private AudioClip turnSound;

    public UnityEngine.Events.UnityEvent onOpen;
    public UnityEngine.Events.UnityEvent onClose;
    public UnityEngine.Events.UnityEvent onRotate;

    private bool isOpen;
    private Coroutine idleCoroutine;

    private void Start()
    {
        if (mode == ValveMode.Toggle)
        {
            SaveManager.Instance?.Register(this);
            isOpen = startOpen;
            ApplyStateInstant(isOpen);
        }
        if (animator != null)
            animator.SetBool(idleBool, true);
    }

    private void OnDestroy()
    {
        if (mode == ValveMode.Toggle)
            SaveManager.Instance?.Unregister(this);
    }

    public void Interact()
    {
        if (mode == ValveMode.Toggle)
            Toggle();
        else
            RotateStep();
    }

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
        if (idleCoroutine != null) StopCoroutine(idleCoroutine);
        if (animator != null)
        {
            animator.SetBool(idleBool, false);
            animator.SetTrigger(openTrigger);
            if (turnSound != null && AudioManager.Instance != null)
                AudioManager.Instance.PlayOneShot(turnSound, transform.position, volumeScale: 0.4f);
        }
        onOpen.Invoke();
        idleCoroutine = StartCoroutine(ReturnToIdle());
    }

    public void Close()
    {
        if (!isOpen) return;
        isOpen = false;
        if (idleCoroutine != null) StopCoroutine(idleCoroutine);
        if (animator != null)
        {
            animator.SetBool(idleBool, false);
            animator.SetTrigger(closeTrigger);
            if (turnSound != null && AudioManager.Instance != null)
                AudioManager.Instance.PlayOneShot(turnSound, transform.position, volumeScale: 0.4f);
        }
        onClose.Invoke();
        idleCoroutine = StartCoroutine(ReturnToIdle());
    }

    public void RotateStep()
    {
        if (mode != ValveMode.Rotate) return;
        if (idleCoroutine != null) StopCoroutine(idleCoroutine);
        if (animator != null)
        {
            animator.SetBool(idleBool, false);
            animator.SetTrigger(openTrigger);
            if (turnSound != null && AudioManager.Instance != null)
                AudioManager.Instance.PlayOneShot(turnSound, transform.position, volumeScale: 0.4f, pitchVariation: 0.1f);
        }
        onRotate.Invoke();
        idleCoroutine = StartCoroutine(ReturnToIdle());
    }

    private IEnumerator ReturnToIdle()
    {
        yield return new WaitForSeconds(animationDuration + 0.1f);
        if (animator != null) animator.SetBool(idleBool, true);
        idleCoroutine = null;
    }

    private void ApplyStateInstant(bool open)
    {
        if (animator != null) animator.SetBool(idleBool, true);
    }

    public bool IsOpen() => isOpen;

    // --- ISaveable implementation ---

    public void Save(GameData data)
    {
        if (mode != ValveMode.Toggle || string.IsNullOrEmpty(saveID)) return;
        data.componentStates.RemoveAll(c => c.id == saveID);
        data.componentStates.Add(new ComponentState { id = saveID, state = isOpen ? "Open" : "Closed" });
    }

    public void Load(GameData data)
    {
        if (mode != ValveMode.Toggle || string.IsNullOrEmpty(saveID)) return;
        ComponentState cs = data.componentStates.FirstOrDefault(c => c.id == saveID);
        if (cs != null)
        {
            isOpen = cs.state == "Open";
            if (animator != null) animator.SetBool(idleBool, true);
        }
    }
}