using UnityEngine;
using System.Collections;

public class Valve : MonoBehaviour
{
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
        isOpen = startOpen;
        ApplyStateInstant(isOpen);
        if (animator != null)
            animator.SetBool(idleBool, true);
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

        if (idleCoroutine != null)
            StopCoroutine(idleCoroutine);

        if (animator != null)
        {
            animator.SetBool(idleBool, false);
            animator.SetTrigger(openTrigger);
        }

        //Debug.Log("Valve: Open triggered");
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

        //Debug.Log("Valve: Close triggered");
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
        {
            // Ensure the idle state is correct
            animator.SetBool(idleBool, true);
            // If you have distinct states like "Open" and "Closed", you could force them here:
            // if (open) animator.Play("Open", 0, 0f);
            // else animator.Play("Closed", 0, 0f);
        }
    }

    public bool IsOpen() => isOpen;
}