using UnityEngine;
using UnityEngine.Events;

public class TriggerEvent : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool oneShot = true;
    [SerializeField] private string requiredTag = "Player";

    [Header("Events")]
    public UnityEvent onTriggerEnter;

    private bool hasTriggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (oneShot && hasTriggered) return;
        if (!other.CompareTag(requiredTag)) return;

        onTriggerEnter.Invoke();
        hasTriggered = true;
    }
}