using UnityEngine;
using UnityEngine.Events;

public class AccessTerminal : MonoBehaviour
{
    [SerializeField] private string requiredAccessItemID;
    [SerializeField] private UnityEvent onAccessGranted;
    [SerializeField] private UnityEvent onAccessDenied;

    [Header("Notifications")]
    [SerializeField] private string notificationGranted;
    [SerializeField] private string notificationDenied;

    private bool hasGranted = false;

    public void CheckAccess()
    {
        if (hasGranted) return;

        GameData data = SaveManager.Instance?.GetCurrentData();
        if (data != null && data.collectedAccessItems.Contains(requiredAccessItemID))
        {
            hasGranted = true;
            if (NotificationManager.Instance != null)
                NotificationManager.Instance.NotifyBottom(notificationGranted);

            onAccessGranted.Invoke();
        }
        else
        {
            if (NotificationManager.Instance != null)
                NotificationManager.Instance.NotifyBottom(notificationDenied);

            onAccessDenied.Invoke();
        }
    }
}