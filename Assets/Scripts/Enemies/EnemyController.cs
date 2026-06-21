using UnityEngine;
using System.Collections;

public abstract class EnemyController : MonoBehaviour
{
    [Header("Identification")]
    [SerializeField] protected string uniqueID;
    
    [Header("Enemy Stats")]
    [SerializeField] protected float playerDetectionRadius = 5f;
    public float attackRadius = 1f;
    public float damage = -10f;
    public float damageTakenMultiplier = 1f;
    public float temperatureSteal = 20f;

    [Header("Critical Temperatures")]
    public float frozenTime = 5f;
    public float smokingTime = 5f;

    [Header("Temperature Drift (Natural Revert)")]
    public float temperatureDriftSpeed = 5f;

    [Header("Line of Sight")]
    public LayerMask obstacleMask;

    [Header("Animator and VFX")]
    public Animator animator;
    
    [Header("Misc")]
    public bool isFacingRight = false;
    protected EnemyState currentState;
    [HideInInspector] public ThermalObject thermalObject;
    [HideInInspector] public Rigidbody2D rb;
    [HideInInspector] public GameObject player;
    [HideInInspector] public SmokeController smokeController;

    private GeneralThermalRegulator playerThermal;
    private bool isFrozen = false;
    private bool isSmoking = false;
    private float initialTemperature;
    private Coroutine criticalStateTimer;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        thermalObject = GetComponentInChildren<ThermalObject>();
        player = GameObject.FindGameObjectWithTag("Player");
        playerThermal = player.GetComponent<GeneralThermalRegulator>();
        smokeController = GetComponentInChildren<SmokeController>();
    }

    protected virtual void Start()
    {
        if (thermalObject != null)
            initialTemperature = thermalObject.GetTemperature();

        if (EnemyManager.Instance != null && EnemyManager.Instance.IsEnemyDead(uniqueID))
        {
            gameObject.SetActive(false);
            return;
        }
    }

    protected virtual void Update()
    {
        // Natural temperature drift towards initial temperature (only if not in critical state)
        if (!isFrozen && !isSmoking && thermalObject != null)
        {
            float current = thermalObject.GetTemperature();
            float target = initialTemperature;
            float step = temperatureDriftSpeed * Time.deltaTime;
            float newTemp = Mathf.MoveTowards(current, target, step);
            if (!Mathf.Approximately(current, newTemp))
                thermalObject.SetBaseTemperature(newTemp);
        }

        currentState?.Update();
    }

    protected virtual void FixedUpdate()
    {
        currentState?.FixedUpdate();
    }

    public void ChangeState(EnemyState newState)
    {
        currentState?.Exit();
        currentState = newState;
        currentState?.Enter();
    }

    public void FacePlayer()
    {
        float dx = player.transform.position.x - transform.position.x;
        bool shouldFaceRight = dx > 0f;
        if (shouldFaceRight != isFacingRight)
        {
            isFacingRight = shouldFaceRight;
            Vector3 scale = transform.localScale;
            scale.x *= -1f;
            transform.localScale = scale;
        }
    }

    public bool IsPlayerInDetectionRange()
    {
        Vector2 directionToPlayer = player.transform.position - transform.position;
        float distance = directionToPlayer.magnitude;
        if (distance > playerDetectionRadius) return false;

        float tempMultiplier = playerThermal != null 
            ? playerThermal.GetCurrentTemperature() / 100f 
            : 1f;
        if (distance > playerDetectionRadius * tempMultiplier) return false;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToPlayer, distance, obstacleMask);
        return hit.collider == null;
    }

    public bool IsPlayerInAttackRange() =>
        (player.transform.position - transform.position).magnitude <= attackRadius;

    public void ChangeBaseTemperature(float damage) 
    {
        float newTemp = thermalObject.GetTemperature() + damage * damageTakenMultiplier;
        
        thermalObject.ChangeBaseTemperature(damage * damageTakenMultiplier);

        if (isFrozen && damage > 0f)
        {
            Die();
            return;
        }
        if (isSmoking && damage < 0f)
        {
            Die();
            return;
        }

        float finalTemp = thermalObject.GetTemperature();
        if (finalTemp >= 100f && !isSmoking)
        {
            isSmoking = true;
            OnSmokingStarted();
            StartCriticalStateTimer(false);
        }
        if (finalTemp <= 0f && !isFrozen)
        {
            isFrozen = true;
            StartCriticalStateTimer(true);
        }
    }

    private void StartCriticalStateTimer(bool isFrozenState)
    {
        if (criticalStateTimer != null)
            StopCoroutine(criticalStateTimer);
        criticalStateTimer = StartCoroutine(CriticalStateTimerRoutine(isFrozenState));
    }

    private IEnumerator CriticalStateTimerRoutine(bool isFrozenState)
    {
        float delay = isFrozenState ? frozenTime : smokingTime;
        yield return new WaitForSeconds(delay);

        if (isFrozenState)
        {
            isFrozen = false;
        }
        else
        {
            isSmoking = false;
            OnSmokingEnded();
        }
        criticalStateTimer = null;
    }

    protected virtual void OnSmokingStarted() { }
    protected virtual void OnSmokingEnded() { }

    protected virtual void Die()
    {
        if (criticalStateTimer != null)
            StopCoroutine(criticalStateTimer);

        if (EnemyManager.Instance != null)
            EnemyManager.Instance.RegisterDeath(uniqueID);

        if (smokeController != null)
            smokeController.OnOwnerDied();

        PartsExploder exploder = GetComponent<PartsExploder>();
        if (exploder != null)
            exploder.Explode();
    }

    private void OnDrawGizmosSelected()
    {
        if (player == null) return;
        Gizmos.color = IsPlayerInDetectionRange() ? Color.green : Color.red;
        Gizmos.DrawLine(transform.position, player.transform.position);
    }

    public bool IsFrozen() => isFrozen;
}