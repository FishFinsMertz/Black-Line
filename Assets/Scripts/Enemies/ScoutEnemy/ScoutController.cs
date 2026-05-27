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

    protected override void Start()
    {
        base.Start();
        ChangeState(new ScoutHoverState(this));
    }

    private void OnDrawGizmosSelected()
    {
        // Detection radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, playerDetectionRadius);

        // Hover search radius
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, hoverSearchRadius);

        // Attack radius
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);

        // Wall repulsion radius
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, wallRepulsionRadius);

        // Hover radius min/max ring
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.4f); // orange translucent
        Gizmos.DrawWireSphere(transform.position, hoverRadiusMin);
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.8f);
        Gizmos.DrawWireSphere(transform.position, hoverRadiusMax);

        // Wall repulsion rays (8 directions)
        Gizmos.color = new Color(0f, 1f, 0f, 0.5f);
        Vector2[] directions = {
            Vector2.up, Vector2.down, Vector2.left, Vector2.right,
            new Vector2(1, 1).normalized, new Vector2(-1, 1).normalized,
            new Vector2(1, -1).normalized, new Vector2(-1, -1).normalized
        };
        foreach (Vector2 dir in directions)
            Gizmos.DrawRay(transform.position, dir * wallRepulsionRadius);

        // Draw line to current hover target if in play mode
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