using UnityEngine;

public class ScoutHoverState : EnemyState
{
    private ScoutController scout;

    private Transform hoverTarget;
    private Vector2 orbitOffset;
    private float retargetTimer;
    private float attackCooldownTimer;
    private Vector2 smoothedVelocity;
    private float spinTimer;

    public ScoutHoverState(ScoutController scout) : base(scout)
    {
        this.scout = scout;
    }

    public override void Enter()
    {
        retargetTimer = 0f;
        attackCooldownTimer = 0f;
        smoothedVelocity = Vector2.zero;
        spinTimer = 0f;
        scout.animator.SetBool("isIdle", true);
        FindHoverTarget();
        GenerateOrbitOffset();

        if (scout.hoverSound != null && scout.audioSource != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.ConfigureLoop(
                scout.audioSource,
                scout.hoverSound,
                volumeScale: 0.3f,
                fadeInDuration: 0f,
                spatialBlend: 1f,
                minDistance: 1f,
                maxDistance: 10f
            );
        }
    }

    public override void Update()
    {
        if (scout.IsFrozen())
        {
            scout.ChangeState(new ScoutFreezeState(scout));
        }
    }

    public override void FixedUpdate()
    {
        retargetTimer -= Time.deltaTime;
        if (retargetTimer <= 0f)
        {
            FindHoverTarget();
            GenerateOrbitOffset();
            retargetTimer = scout.retargetInterval;
        }

        if (hoverTarget == null) return;

        if (attackCooldownTimer > 0f)
            attackCooldownTimer -= Time.deltaTime;

        Vector2 wallRepulsion = GetWallRepulsion();

        MoveTowardOrbitPoint(wallRepulsion);
        RotateTowardMovement();
        TryAttackPlayer();
    }

    public override void Exit()
    {
        scout.transform.rotation = Quaternion.identity;
        scout.rb.linearVelocity = Vector2.zero;
        scout.animator.SetBool("isIdle", false);

        if (scout.audioSource != null && scout.audioSource.isPlaying)
        {
            AudioManager.Instance?.FadeOut(scout.audioSource, 0f);
        }
    }

    private void FindHoverTarget()
    {
        ThermalObject[] allThermals = Object.FindObjectsByType<ThermalObject>(FindObjectsSortMode.None);

        ThermalObject best = null;
        float bestTemp = float.MinValue;

        foreach (ThermalObject t in allThermals)
        {
            if ((scout.targetableLayers & (1 << t.gameObject.layer)) == 0) continue;

            float dist = Vector2.Distance(scout.transform.position, t.transform.position);
            if (dist > scout.hoverSearchRadius) continue;

            Vector2 direction = t.transform.position - scout.transform.position;
            RaycastHit2D hit = Physics2D.Raycast(scout.transform.position, direction, dist, scout.obstacleMask);
            if (hit.collider != null) continue;

            float temp = t.GetCurrentTemperature();
            if (temp > bestTemp)
            {
                bestTemp = temp;
                best = t;
            }
        }

        hoverTarget = best != null ? best.transform : null;
    }

    private void GenerateOrbitOffset()
    {
        if (hoverTarget == null) return;

        float angle = Random.Range(0f, Mathf.PI * 2f);
        float radius = Random.Range(scout.hoverRadiusMin, scout.hoverRadiusMax);
        float erraticX = Random.Range(-scout.erraticness, scout.erraticness);
        float erraticY = Random.Range(-scout.erraticness, scout.erraticness);

        orbitOffset = new Vector2(
            Mathf.Cos(angle) * radius + erraticX,
            Mathf.Sin(angle) * radius + erraticY
        );
    }

    private Vector2 GetWallRepulsion()
    {
        Vector2 repulsion = Vector2.zero;
        Vector2[] directions = {
            Vector2.up, Vector2.down, Vector2.left, Vector2.right,
            new Vector2(1, 1).normalized, new Vector2(-1, 1).normalized,
            new Vector2(1, -1).normalized, new Vector2(-1, -1).normalized
        };

        foreach (Vector2 dir in directions)
        {
            RaycastHit2D hit = Physics2D.Raycast(
                scout.transform.position, dir,
                scout.wallRepulsionRadius,
                scout.wallLayer
            );

            if (hit.collider != null)
            {
                float proximity = 1f - (hit.distance / scout.wallRepulsionRadius);
                repulsion += -dir * proximity * scout.wallRepulsionForce;
            }
        }

        return repulsion;
    }

    private void MoveTowardOrbitPoint(Vector2 wallRepulsion)
    {
        Vector2 targetPoint = (Vector2)hoverTarget.position + orbitOffset;
        Vector2 currentPos = scout.transform.position;
        Vector2 direction = targetPoint - currentPos;
        float distance = direction.magnitude;

        if (distance < 0.3f)
            GenerateOrbitOffset();

        Vector2 desiredVelocity = direction.normalized * scout.moveSpeed + wallRepulsion;
        smoothedVelocity = Vector2.Lerp(smoothedVelocity, desiredVelocity, scout.wanderSmoothness * Time.deltaTime);
        scout.rb.linearVelocity = smoothedVelocity;

        if (Mathf.Abs(smoothedVelocity.x) > 0.1f)
        {
            Vector3 scale = scout.transform.localScale;
            scale.x = smoothedVelocity.x > 0f ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);
            scout.transform.localScale = scale;
        }
    }

    private void RotateTowardMovement()
    {
        Vector2 actualVelocity = scout.rb.linearVelocity;

        if (actualVelocity.sqrMagnitude < 0.05f)
        {
            scout.transform.rotation = Quaternion.Lerp(scout.transform.rotation, Quaternion.identity, 5f * Time.deltaTime);
            spinTimer = 0f;
            return;
        }

        float angle = Mathf.Atan2(actualVelocity.y, actualVelocity.x) * Mathf.Rad2Deg;
        float clampedAngle = Mathf.Clamp(angle, -scout.maxTiltAngle, scout.maxTiltAngle);

        Quaternion targetRot = Quaternion.Euler(0f, 0f, clampedAngle);

        float currentAngle = scout.transform.eulerAngles.z;
        float angleDelta = Mathf.DeltaAngle(currentAngle, clampedAngle);
        if (Mathf.Abs(angleDelta) > 45f && actualVelocity.sqrMagnitude < 0.2f)
        {
            scout.transform.rotation = Quaternion.Lerp(scout.transform.rotation, Quaternion.identity, 3f * Time.deltaTime);
            spinTimer = 0f;
            return;
        }

        float maxDegreesPerSec = scout.tiltSpeed * 60f;
        scout.transform.rotation = Quaternion.RotateTowards(
            scout.transform.rotation,
            targetRot,
            maxDegreesPerSec * Time.deltaTime
        );

        spinTimer += Time.deltaTime;
        if (spinTimer > 2f)
        {
            scout.transform.rotation = Quaternion.identity;
            spinTimer = 0f;
        }
    }

    private void TryAttackPlayer()
    {
        if (attackCooldownTimer > 0f) return;
        if (!scout.IsPlayerInAttackRange()) return;

        PlayerController pc = scout.player.GetComponent<PlayerController>();
        if (pc != null)
            pc.TakeDamage(scout.damage);

        attackCooldownTimer = scout.attackCooldown;
    }

    public Transform GetHoverTarget() => hoverTarget;
}