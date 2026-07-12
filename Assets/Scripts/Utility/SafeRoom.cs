using UnityEngine;

public class SafeRoom : MonoBehaviour
{
    [Header("Safe Room Settings")]
    [SerializeField] private bool isActive = true;

    private Collider2D triggerCollider;
    private GeneralThermalRegulator thermal;

    private void Start()
    {
        triggerCollider = GetComponent<Collider2D>();
        thermal = FindFirstObjectByType<GeneralThermalRegulator>();

        if (thermal == null)
        {
            Debug.LogWarning("SafeRoom: No GeneralThermalRegulator found.");
            return;
        }

        // Check if the player is already inside this safe room on scene load
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
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isActive) return;
        if (!other.CompareTag("Player")) return;
        if (thermal != null)
            thermal.SetInSafeRoom(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!isActive) return;
        if (!other.CompareTag("Player")) return;
        if (thermal != null)
            thermal.SetInSafeRoom(false);
    }
}