using UnityEngine;
using UnityEngine.Events;

public class AccessTerminal : MonoBehaviour
{
    [SerializeField] private string requiredAccessItemID;
    [SerializeField] private UnityEvent onAccessGranted;
    [SerializeField] private UnityEvent onAccessDenied;
    private bool hasGranted = false;

    // Called by ButtonTrigger.onInteract
    public void CheckAccess()
    {
        if (hasGranted) return;

        GameData data = SaveManager.Instance?.GetCurrentData();
        if (data != null && data.collectedAccessItems.Contains(requiredAccessItemID))
        {
            hasGranted = true;
            onAccessGranted.Invoke();
        }
        else
        {
            Debug.Log($"Access denied. Required: {requiredAccessItemID}");
            onAccessDenied.Invoke();
        }
    }
}