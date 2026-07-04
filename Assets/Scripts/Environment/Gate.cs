using UnityEngine;
using System.Collections;
using System.Linq;

public enum GateState
{
    Open,
    Closed
}

public class Gate : MonoBehaviour, IInteractible, ISaveable
{
    [Header("Save ID")]
    [SerializeField] private string saveID;

    [Header("Gate Settings")]
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

        if (!string.IsNullOrEmpty(saveID))
            SaveManager.Instance?.Register(this);
    }

    private void OnDestroy()
    {
        if (!string.IsNullOrEmpty(saveID))
            SaveManager.Instance?.Unregister(this);
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

    // --- ISaveable ---
    public void Save(GameData data)
    {
        if (string.IsNullOrEmpty(saveID)) return;
        data.componentStates.RemoveAll(c => c.id == saveID);
        data.componentStates.Add(new ComponentState { id = saveID, state = gateState.ToString() });
    }

    public void Load(GameData data)
    {
        if (string.IsNullOrEmpty(saveID)) return;
        ComponentState cs = data.componentStates.FirstOrDefault(c => c.id == saveID);
        if (cs != null)
        {
            if (cs.state == "Open")
            {
                gateState = GateState.Open;
                gateCollider.enabled = false;
                animator.Play("Open", 0, 0f);
            }
            else
            {
                gateState = GateState.Closed;
                gateCollider.enabled = true;
                animator.Play("Closed", 0, 0f);
            }
        }
    }
}