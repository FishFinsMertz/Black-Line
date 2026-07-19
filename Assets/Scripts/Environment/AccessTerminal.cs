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

    [Header("Audio")]
    [SerializeField] private AudioClip accessGrantedAudio;

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

            if (accessGrantedAudio != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayOneShot(accessGrantedAudio, transform.position, volumeScale: 0.2f);
            }
        }
        else
        {
            if (NotificationManager.Instance != null)
                NotificationManager.Instance.NotifyBottom(notificationDenied);

            onAccessDenied.Invoke();
        }
    }
}