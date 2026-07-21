using UnityEngine;

public class SafeRoom : MonoBehaviour
{
    [Header("Safe Room Settings")]
    [SerializeField] private bool isActive = true;
    [SerializeField] private string enterMessage = "Ambient Temperature Stabilizing";
    [SerializeField] private int flashCycles = 6;

    private Collider2D triggerCollider;
    private GeneralThermalRegulator thermal;
    private bool hasNotifiedOnLoad = false;

    private void Start()
    {
        triggerCollider = GetComponent<Collider2D>();
        thermal = FindFirstObjectByType<GeneralThermalRegulator>();

        if (thermal == null)
        {
            return;
        }

        CheckPlayerInside();
    }

    private void CheckPlayerInside()
    {
        if (!isActive || triggerCollider == null) return;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        if (triggerCollider.OverlapPoint(player.transform.position))
        {
            thermal.SetInSafeRoom(true);
            if (!hasNotifiedOnLoad)
            {
                NotificationManager.Instance?.NotifyTop(enterMessage, true, flashCycles);
                hasNotifiedOnLoad = true;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isActive) return;
        if (!other.CompareTag("Player")) return;
        if (thermal != null)
        {
            thermal.SetInSafeRoom(true);
            NotificationManager.Instance?.NotifyTop(enterMessage, true, flashCycles);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!isActive) return;
        if (!other.CompareTag("Player")) return;
        if (thermal != null)
            thermal.SetInSafeRoom(false);
        hasNotifiedOnLoad = false;
    }
}