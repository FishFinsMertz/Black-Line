using UnityEngine;

public abstract class ArmController : MonoBehaviour
{
    [Header("Base Settings")]
    [SerializeField] protected bool armEnabled = true;
    [SerializeField] protected float flipThreshold = 0.1f;

    [Header("Run Offset")]
    [SerializeField] protected Vector2 runOffset = Vector2.zero; // X and Y offset in local space
    [SerializeField] protected float runOffsetSmoothTime = 0.1f;

    protected PlayerController player;
    protected PlayerState currentPlayerState;
    protected Camera cam;
    protected Vector3 originalLocalPosition;
    protected Vector3 velocityRef;

    protected virtual void Start()
    {
        player = GetComponentInParent<PlayerController>();
        cam = Camera.main;
        originalLocalPosition = transform.localPosition;
    }

    protected virtual void Update()
    {
        if (!armEnabled) return;
        HandleFlip();
        currentPlayerState = player.GetCurrentState();

        // Calculate target position: original + run offset (if running)
        Vector3 targetPos = originalLocalPosition;
        if (currentPlayerState is PlayerRunState)
        {
            targetPos += new Vector3(runOffset.x, runOffset.y, 0f);
        }

        // Smoothly move towards target position
        transform.localPosition = Vector3.SmoothDamp(transform.localPosition, targetPos, ref velocityRef, runOffsetSmoothTime);
    }

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

    public virtual void OnEquip() { }
    public virtual void OnUnequip() { }
}