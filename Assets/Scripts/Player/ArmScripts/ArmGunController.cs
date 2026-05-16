using UnityEngine;

public class ArmGunController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool aimEnabled = true;
    [SerializeField] private float minAngle = -20f;
    [SerializeField] private float maxAngle = 80f;

    [Header("Flip")]
    [SerializeField] private PlayerController player;
    [SerializeField] private float flipThreshold = 0.1f;

    [Header("Fire Point (Testing)")]
    [SerializeField] private Transform firePoint;   // drag the fire point child here

    private void Start()
    {
        if (player == null)
            player = GetComponentInParent<PlayerController>();
    }

    private void Update()
    {
        if (!aimEnabled) return;

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;

        // FLIP LOGIC
        float dx = mousePos.x - player.transform.position.x;
        if (dx > flipThreshold && !player.isFacingRight)
            player.Flip();
        else if (dx < -flipThreshold && player.isFacingRight)
            player.Flip();

        // AIM LOGIC
        Vector2 dir = mousePos - transform.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        if (player.isFacingRight)
        {
            angle = Mathf.DeltaAngle(0f, angle);
            angle = Mathf.Clamp(angle, minAngle, maxAngle);
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }
        else
        {
            angle = Mathf.DeltaAngle(0f, angle - 180f);
            angle = Mathf.Clamp(angle, -maxAngle, -minAngle);
            transform.localRotation = Quaternion.Euler(0f, 0f, -angle);
        }

        // ----- TEST: Draw line from firePoint to mouse on left click -----
        if (Input.GetMouseButtonDown(0) && firePoint != null)
        {
            // Draw a red line that stays visible for 0.5 seconds
            Debug.DrawLine(firePoint.position, mousePos, Color.red, 0.5f);
        }
    }
}