// RiftController.cs
using UnityEngine;

public class RiftController : EnemyController
{
    [Header("Bounce Settings")]
    public float bounceSpeed = 8f;
    public float bounceDelayMin = 1f;
    public float bounceDelayMax = 2f;
    public bool startWithBounce = true;

    [Header("Bounce Raycast Angles (degrees from straight up)")]
    [Tooltip("0 = straight up, 90 = straight horizontal. Keep these above 0 and below 90.")]
    public float bounceAngleMin = 10f; // slight forward lean minimum
    public float bounceAngleMax = 45f; // maximum forward lean

    [Header("Raycast")]
    public LayerMask surfaceLayer;

    public bool IsOnCeiling { get; private set; } = false;

    protected override void Start()
    {
        base.Start();
        if (startWithBounce)
            ChangeState(new RiftBounceState(this));
        else {
            ChangeState(new RiftIdleState(this));
        }
    }

    // Flip vertically, setting absolute orientation
    public void SetVerticalOrientation(bool onCeiling)
    {
        IsOnCeiling = onCeiling;
        Vector3 scale = transform.localScale;
        scale.y = onCeiling ? -Mathf.Abs(scale.y) : Mathf.Abs(scale.y);
        transform.localScale = scale;
    }

    // Cast a ray in the required direction and return hit info
    // On ground: cast upward+forward. On ceiling: cast downward+forward.
    // Returns true if a valid opposite surface is found.
    public bool TryGetBounceTarget(out Vector2 landingPoint, out float travelDistance)
    {
        landingPoint = Vector2.zero;
        travelDistance = 0f;

        // Pick a random angle within range
        float angleDeg = Random.Range(bounceAngleMin, bounceAngleMax);
        float angleRad = angleDeg * Mathf.Deg2Rad;

        // Forward direction (toward player horizontally)
        float forwardSign = isFacingRight ? 1f : -1f;

        // On ground: cast upward. On ceiling: cast downward.
        float vertSign = IsOnCeiling ? -1f : 1f;

        Vector2 rayDir = new Vector2(
            forwardSign * Mathf.Sin(angleRad),
            vertSign * Mathf.Cos(angleRad)
        ).normalized;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, rayDir, Mathf.Infinity, surfaceLayer);

        if (!hit.collider) return false;

        // Validate: on ground we want a ceiling hit (normal pointing down),
        // on ceiling we want a floor hit (normal pointing up)
        float normalY = hit.normal.y;
        bool validSurface = IsOnCeiling ? normalY > 0.7f : normalY < -0.7f;

        if (!validSurface) return false;

        landingPoint = hit.point;
        travelDistance = hit.distance;
        return true;
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        (currentState as RiftBounceState)?.OnHitSurface();
    }
}