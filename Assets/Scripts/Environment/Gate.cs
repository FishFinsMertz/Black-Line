using UnityEngine;
using System.Collections;

public enum GateState
{
    Open,
    Closed
}
public class Gate : MonoBehaviour, IInteractible
{
    [SerializeField] private float openDuration = 1.5f;
    [SerializeField] private float closeDuration = 0f;
    [SerializeField] private GateState gateState = GateState.Open;
    private Animator animator;
    private BoxCollider2D gateCollider;
    private bool interactible = true;

    private void Start()
    {
        gateCollider = GetComponent<BoxCollider2D>();
        animator = GetComponent<Animator>();
    }

    private IEnumerator ColliderCoroutine(float delay, bool enable)
    {
        interactible = false;
        yield return new WaitForSeconds(delay);
        gateCollider.enabled = enable;
        interactible = true;
    }

    // Public API
    public void Open()
    {
        if (!interactible) return;
        gateState = GateState.Open;
        animator.SetTrigger("Open");
        StartCoroutine(ColliderCoroutine(openDuration, false));
    }

    public void Close()
    {
        if (!interactible) return;
        gateState = GateState.Closed;
        animator.SetTrigger("Close");
        StartCoroutine(ColliderCoroutine(closeDuration, true));
    }

    public void Toggle()
    {
        if (!interactible) return;
        if (gateState == GateState.Open)
            Close();
        else
            Open();
    }

    public void EnableInteraction()
    {
        interactible = true;
    }

    public void DisableInteraction()
    {
        interactible = false;
    }
}
