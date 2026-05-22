// RiftBounceState.cs
using UnityEngine;

public class RiftBounceState : EnemyState
{
    private RiftController rift;

    private enum Phase { Seeking, Delay, Travelling }
    private Phase phase;

    private float delayTimer;
    private Vector2 startPosition;
    private Vector2 targetLandingPoint;
    private float totalDistance;
    private float travelTimer;
    private float travelDuration;
    private bool hasFlipped;

    private const float FlipAtNormalized = 0.55f;

    public RiftBounceState(RiftController rift) : base(rift)
    {
        this.rift = rift;
    }

    public override void Enter()
    {
        rift.rb.gravityScale = 0f;
        rift.rb.linearVelocity = Vector2.zero;
        phase = Phase.Seeking;
        SeekTarget();
    }

    private void SeekTarget()
    {
        rift.FacePlayer();

        int maxAttempts = 20;
        for (int i = 0; i < maxAttempts; i++)
        {
            if (rift.TryGetBounceTarget(out Vector2 landing, out float dist))
            {
                targetLandingPoint = landing;
                totalDistance = dist;
                phase = Phase.Delay;
                // Randomize delay a bit for more natural behavior
                delayTimer = Random.Range(rift.bounceDelayMin, rift.bounceDelayMax);
                return;
            }
        }

        Debug.LogWarning("[RiftBounceState] No valid bounce target found, returning to idle.");
        enemy.ChangeState(new RiftIdleState(rift));
    }

    private void Launch()
    {
        phase = Phase.Travelling;
        hasFlipped = false;
        travelDuration = totalDistance / rift.bounceSpeed;
        travelTimer = 0f;
        startPosition = rift.transform.position;
    }

    public override void Update()
    {
        if (phase == Phase.Delay)
        {
            delayTimer -= Time.deltaTime;
            if (delayTimer <= 0f)
                Launch();
            return;
        }

        if (phase == Phase.Travelling)
        {
            travelTimer += Time.deltaTime;
            float progress = Mathf.Clamp01(travelTimer / travelDuration);

            // Ease in-out: accelerate first half, decelerate second half
            float easedProgress = EaseInOut(progress);

            // Move by setting position directly from eased progress
            // so speed curve is perfectly smooth regardless of framerate
            rift.transform.position = new Vector3(
                Mathf.Lerp(startPosition.x, targetLandingPoint.x, easedProgress),
                Mathf.Lerp(startPosition.y, targetLandingPoint.y, easedProgress),
                rift.transform.position.z
            );

            // Flip slightly past halfway
            if (!hasFlipped && progress >= FlipAtNormalized)
            {
                hasFlipped = true;
                rift.SetVerticalOrientation(!rift.IsOnCeiling);
            }

            // Arrive
            if (progress >= 1f)
            {
                rift.rb.linearVelocity = Vector2.zero;

                Collider2D col = rift.GetComponent<Collider2D>();
                float extentY = col != null ? col.bounds.extents.y : 0.5f;
                float snapOffsetY = rift.IsOnCeiling ? -extentY : extentY;

                rift.transform.position = new Vector3(
                    targetLandingPoint.x,
                    targetLandingPoint.y + snapOffsetY,
                    rift.transform.position.z
                );

                enemy.ChangeState(new RiftIdleState(rift));
            }
        }
    }

    // Smooth ease in-out: slow start, fast middle, slow end
    private float EaseInOut(float t) => t * t * (3f - 2f * t);

    public void OnHitSurface()
    {
        if (phase != Phase.Travelling) return;
        travelTimer = travelDuration;
    }

    public override void Exit()
    {
        rift.rb.linearVelocity = Vector2.zero;
    }
}