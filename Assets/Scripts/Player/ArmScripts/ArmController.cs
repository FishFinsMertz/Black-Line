using UnityEngine;

public abstract class ArmController : MonoBehaviour
{
    [Header("Base Settings")]
    [SerializeField] protected bool armEnabled = true;

    [Header("Run Offset")]
    [SerializeField] protected Vector2 runOffset = Vector2.zero;
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

        // Update current player state for animators
        currentPlayerState = player.GetCurrentState();

        // Apply run offset (handled here, not related to flip)
        Vector3 targetPos = originalLocalPosition;
        if (currentPlayerState is PlayerRunState)
            targetPos += new Vector3(runOffset.x, runOffset.y, 0f);

        transform.localPosition = Vector3.SmoothDamp(transform.localPosition, targetPos, ref velocityRef, runOffsetSmoothTime);
    }

    public virtual void OnEquip() { }
    public virtual void OnUnequip() { }
}