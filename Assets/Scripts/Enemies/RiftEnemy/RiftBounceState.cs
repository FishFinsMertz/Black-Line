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
    private bool bounceTriggered;
    private bool hasHit = false;

    private const float FlipAtNormalized = 0.55f;
    private const float BOUNCE_ANIM_LENGTH = 1f;

    public RiftBounceState(RiftController rift) : base(rift)
    {
        this.rift = rift;
    }

    public override void Enter()
    {
        rift.rb.gravityScale = 0f;
        rift.rb.linearVelocity = Vector2.zero;
        phase = Phase.Seeking;
        bounceTriggered = false;
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
                delayTimer = Random.Range(rift.bounceDelayMin, rift.bounceDelayMax);
                return;
            }
        }

        //Debug.LogWarning("[RiftBounceState] No valid bounce target found, returning to idle.");
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
            if (!bounceTriggered && delayTimer <= BOUNCE_ANIM_LENGTH)
            {
                rift.animator.SetTrigger("Bounce");
                if (AudioManager.Instance != null && rift.bounceSound != null)
                {
                    AudioManager.Instance.PlayOneShot(rift.bounceSound, rift.transform.position, volumeScale: 0.35f, pitchVariation: 0.1f);
                }
                bounceTriggered = true;
            }

            delayTimer -= Time.deltaTime;
            if (delayTimer <= 0f)
            {
                Launch();
            }
            return;
        }

        if (phase == Phase.Travelling)
        {
            travelTimer += Time.deltaTime;
            float progress = Mathf.Clamp01(travelTimer / travelDuration);
            float easedProgress = EaseInOut(progress);

            rift.transform.position = new Vector3(
                Mathf.Lerp(startPosition.x, targetLandingPoint.x, easedProgress),
                Mathf.Lerp(startPosition.y, targetLandingPoint.y, easedProgress),
                rift.transform.position.z
            );

            if (!hasFlipped && progress >= FlipAtNormalized)
            {
                hasFlipped = true;
                rift.animator.SetTrigger("Land");
                rift.SetVerticalOrientation(!rift.IsOnCeiling);
            }

            if (progress >= 1f)
            {
                // If hit player
                if (Physics2D.OverlapCircle(rift.transform.position, rift.attackRadius, LayerMask.GetMask("Player")) && !hasHit)
                {
                    hasHit = true;
                    PlayerController pc = rift.player.GetComponent<PlayerController>();
                    if (pc != null)
                        pc.TakeDamage(rift.damage);
                    rift.thermalObject.ChangeCurrentTemperature(rift.temperatureSteal);
                }

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

    private float EaseInOut(float t) => t * t * (3f - 2f * t);

    public void OnHitSurface()
    {
        if (phase != Phase.Travelling) return;
        travelTimer = travelDuration;
    }

    public override void Exit()
    {
        rift.rb.linearVelocity = Vector2.zero;
        hasHit = false;
    }
}