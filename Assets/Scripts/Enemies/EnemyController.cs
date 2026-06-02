// EnemyController.cs
using UnityEngine;

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

    [Header("Line of Sight")]
    public LayerMask obstacleMask; // assign layers that block sight (walls, floors, etc.)

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
        if (EnemyManager.Instance != null && EnemyManager.Instance.IsEnemyDead(uniqueID))
        {
            gameObject.SetActive(false);
            return;
        }
    }

    protected virtual void Update() { currentState?.Update(); }
    protected virtual void FixedUpdate() { currentState?.FixedUpdate(); }

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
        // First check if player is within distance
        Vector2 directionToPlayer = player.transform.position - transform.position;
        float distance = directionToPlayer.magnitude;
        if (distance > playerDetectionRadius) return false;

        // Apply temperature multiplier (player heat affects detection range)
        float tempMultiplier = playerThermal != null 
            ? playerThermal.GetCurrentTemperature() / 100f 
            : 1f;
        if (distance > playerDetectionRadius * tempMultiplier) return false;

        // Line of sight check: raycast towards player, ignoring enemy and player layers
        // Use obstacleMask to detect only blocking geometry
        RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToPlayer, distance, obstacleMask);
        // If the ray hits something, there's an obstacle -> player not visible
        return hit.collider == null;
    }

    public bool IsPlayerInAttackRange() =>
        (player.transform.position - transform.position).magnitude <= attackRadius;

    public void ChangeBaseTemperature(float damage) 
    {
        thermalObject.ChangeBaseTemperature(damage * damageTakenMultiplier);

        if (thermalObject.GetTemperature() >= 100f)
            Die();
    }

    protected virtual void Die()
    {
        if (EnemyManager.Instance != null)
            EnemyManager.Instance.RegisterDeath(uniqueID);

        if (smokeController != null)
            smokeController.OnOwnerDied();

        EnemyDeathExploder exploder = GetComponent<EnemyDeathExploder>();
        if (exploder != null)
            exploder.Explode();
    }

    // Optional: visualize the raycast in the editor
    private void OnDrawGizmosSelected()
    {
        if (player == null) return;
        Gizmos.color = IsPlayerInDetectionRange() ? Color.green : Color.red;
        Gizmos.DrawLine(transform.position, player.transform.position);
    }
}