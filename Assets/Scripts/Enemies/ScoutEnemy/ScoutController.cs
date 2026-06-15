using UnityEngine;

public class ScoutController : EnemyController
{
    [Header("Movement")]
    public float moveSpeed = 3f;
    public float wanderSmoothness = 2f;

    [Header("Hover")]
    public float hoverRadiusMin = 1.5f;
    public float hoverRadiusMax = 3f;
    public float erraticness = 1f;
    public float hoverSearchRadius = 10f;

    [Header("Retargeting")]
    public float retargetInterval = 2f;

    [Header("Rotation")]
    public float maxTiltAngle = 30f;
    public float tiltSpeed = 5f;

    [Header("Wall Avoidance")]
    public float wallRepulsionRadius = 1.5f;
    public float wallRepulsionForce = 5f;
    public LayerMask wallLayer;

    [Header("Attack")]
    public float attackCooldown = 1.5f;

    [Header("Targeting")]
    public LayerMask targetableLayers;

    // Original values for smoking state
    private float originalMoveSpeed;
    private float originalHoverRadiusMin;
    private float originalHoverRadiusMax;
    private float originalErraticness;

    protected override void Start()
    {
        base.Start();
        originalMoveSpeed = moveSpeed;
        originalHoverRadiusMin = hoverRadiusMin;
        originalHoverRadiusMax = hoverRadiusMax;
        originalErraticness = erraticness;

        ChangeState(new ScoutHoverState(this));
    }

    protected override void OnSmokingStarted()
    {
        //Debug.Log($"{name} (Scout) is overheating");
        moveSpeed *= 1.8f;
        hoverRadiusMin = 0.5f;
        hoverRadiusMax = 1.8f;
        erraticness *= 2.5f;
    }

    protected override void OnSmokingEnded()
    {
        Debug.Log($"{name} (Scout) cooled down – returning to normal.");
        moveSpeed = originalMoveSpeed;
        hoverRadiusMin = originalHoverRadiusMin;
        hoverRadiusMax = originalHoverRadiusMax;
        erraticness = originalErraticness;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, playerDetectionRadius);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, hoverSearchRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, wallRepulsionRadius);

        Gizmos.color = new Color(1f, 0.5f, 0f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, hoverRadiusMin);
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.8f);
        Gizmos.DrawWireSphere(transform.position, hoverRadiusMax);

        Gizmos.color = new Color(0f, 1f, 0f, 0.5f);
        Vector2[] directions = {
            Vector2.up, Vector2.down, Vector2.left, Vector2.right,
            new Vector2(1, 1).normalized, new Vector2(-1, 1).normalized,
            new Vector2(1, -1).normalized, new Vector2(-1, -1).normalized
        };
        foreach (Vector2 dir in directions)
            Gizmos.DrawRay(transform.position, dir * wallRepulsionRadius);

        if (Application.isPlaying)
        {
            ScoutHoverState hoverState = currentState as ScoutHoverState;
            if (hoverState != null)
            {
                Transform target = hoverState.GetHoverTarget();
                if (target != null)
                {
                    Gizmos.color = Color.magenta;
                    Gizmos.DrawLine(transform.position, target.position);
                    Gizmos.DrawWireSphere(target.position, 0.3f);
                }
            }
        }
    }
}