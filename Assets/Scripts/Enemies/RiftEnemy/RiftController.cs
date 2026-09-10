using UnityEngine;

public class RiftController : EnemyController
{
    [Header("Bounce Settings")]
    public float bounceSpeed = 8f;
    public float bounceDelayMin = 1f;
    public float bounceDelayMax = 2f;
    public bool startWithBounce = true;

    [Header("Bounce Raycast Angles")]
    public float bounceAngleMin = 10f;
    public float bounceAngleMax = 45f;

    [Header("Drop Attack (from ceiling)")]
    [Range(0f, 1f)]
    public float chanceToDropOnPlayer = 0.3f;
    public float dropDistance = 7f;

    [Header("Raycast")]
    public LayerMask surfaceLayer;

    [Header("Audio")]
    public AudioClip bounceSound;

    public bool IsOnCeiling { get; private set; } = false;

    // Original values for smoking state
    private float originalBounceSpeed;
    private float originalBounceDelayMin;
    private float originalBounceDelayMax;

    protected override void Start()
    {
        base.Start();
        // Store original values
        originalBounceSpeed = bounceSpeed;
        originalBounceDelayMin = bounceDelayMin;
        originalBounceDelayMax = bounceDelayMax;

        if (startWithBounce)
        {
            SetVerticalOrientation(true);
            ChangeState(new RiftBounceState(this));
        }
        else
        {
            SetVerticalOrientation(false);
            ChangeState(new RiftIdleState(this));
        }
    }

    protected override void OnSmokingStarted()
    {
        //Debug.Log($"{name} (Rift) is overheating");
        bounceSpeed *= 1.6f;
        bounceDelayMin = 0.2f;
        bounceDelayMax = 0.6f;
    }

    protected override void OnSmokingEnded()
    {
        //Debug.Log($"{name} (Rift) cooled down – bounce pattern normalised.");
        bounceSpeed = originalBounceSpeed;
        bounceDelayMin = originalBounceDelayMin;
        bounceDelayMax = originalBounceDelayMax;
    }

    public void SetVerticalOrientation(bool onCeiling)
    {
        IsOnCeiling = onCeiling;
        Vector3 scale = transform.localScale;
        scale.y = onCeiling ? -Mathf.Abs(scale.y) : Mathf.Abs(scale.y);
        transform.localScale = scale;
    }

    public bool TryGetGroundUnderPlayer(out Vector2 groundPoint)
    {
        groundPoint = Vector2.zero;
        RaycastHit2D hit = Physics2D.Raycast(player.transform.position, Vector2.down, 20f, surfaceLayer);
        if (hit.collider)
        {
            groundPoint = hit.point;
            return true;
        }
        return false;
    }

    public bool TryGetBounceTarget(out Vector2 landingPoint, out float travelDistance)
    {
        landingPoint = Vector2.zero;
        travelDistance = 0f;

        if (IsOnCeiling && Random.value < chanceToDropOnPlayer)
        {
            float horizDist = Mathf.Abs(player.transform.position.x - transform.position.x);
            if (horizDist <= dropDistance && TryGetGroundUnderPlayer(out Vector2 playerGround))
            {
                landingPoint = playerGround;
                travelDistance = Vector2.Distance(transform.position, landingPoint);
                return true;
            }
        }

        float angleDeg = Random.Range(bounceAngleMin, bounceAngleMax);
        float angleRad = angleDeg * Mathf.Deg2Rad;
        float forwardSign = isFacingRight ? 1f : -1f;
        float vertSign = IsOnCeiling ? -1f : 1f;

        Vector2 rayDir = new Vector2(
            forwardSign * Mathf.Sin(angleRad),
            vertSign * Mathf.Cos(angleRad)
        ).normalized;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, rayDir, Mathf.Infinity, surfaceLayer);
        if (!hit.collider) return false;

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

    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;

        Gizmos.color = new Color(1f, 0.5f, 0, 0.3f);
        Gizmos.DrawWireSphere(transform.position, dropDistance);

        float forwardSign = isFacingRight ? 1f : -1f;
        float vertSign = IsOnCeiling ? -1f : 1f;

        Vector2 GetDir(float angleDeg)
        {
            float rad = angleDeg * Mathf.Deg2Rad;
            return new Vector2(forwardSign * Mathf.Sin(rad), vertSign * Mathf.Cos(rad)).normalized;
        }

        Vector2 origin = transform.position;
        Vector2 dirMin = GetDir(bounceAngleMin);
        Vector2 dirMax = GetDir(bounceAngleMax);

        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(origin, dirMin * 5f);
        Gizmos.DrawRay(origin, dirMax * 5f);

        Vector2 prev = dirMin;
        for (float a = bounceAngleMin; a <= bounceAngleMax; a += 2f)
        {
            Vector2 current = GetDir(a);
            Gizmos.DrawLine(origin + prev * 5f, origin + current * 5f);
            prev = current;
        }
    }
}