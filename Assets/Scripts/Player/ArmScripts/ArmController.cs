using UnityEngine;

public abstract class ArmController : MonoBehaviour
{
    [Header("Base Settings")]
    [SerializeField] protected bool armEnabled = true;
    [SerializeField] protected float flipThreshold = 0.1f;

    protected PlayerController player;
    protected Camera cam;

    protected virtual void Start()
    {
        player = GetComponentInParent<PlayerController>();
        cam = Camera.main;
    }

    protected virtual void Update()
    {
        if (!armEnabled) return;
        HandleFlip();
    }

    // Flip the player based on mouse position relative to player's pivot
    protected virtual void HandleFlip()
    {
        if (player == null || cam == null) return;

        Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;

        float dx = mousePos.x - player.transform.position.x;
        if (dx > flipThreshold && !player.isFacingRight)
            player.Flip();
        else if (dx < -flipThreshold && player.isFacingRight)
            player.Flip();
    }

    // Called by Inventory when this arm is equipped
    public virtual void OnEquip() { }

    // Called by Inventory when this arm is unequipped
    public virtual void OnUnequip() { }
}