// EnemyController.cs
using UnityEngine;

public abstract class EnemyController : MonoBehaviour
{
    [Header("Enemy Stats")]
    [SerializeField] protected float playerDetectionRadius = 5f;
    public float attackRadius = 1f;
    public float damage = -10f;
    public float temperatureSteal = 20f;

    [Header("Animator")]
    public Animator animator;

    [Header("Misc")]
    public bool isFacingRight = false;
    protected EnemyState currentState;
    [HideInInspector] public ThermalObject thermalObject;
    [HideInInspector] public Rigidbody2D rb;
    [HideInInspector] public GameObject player;

    private GeneralThermalRegulator playerThermal;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        thermalObject = GetComponentInChildren<ThermalObject>();
        player = GameObject.FindGameObjectWithTag("Player");
        playerThermal = player.GetComponent<GeneralThermalRegulator>();
    }

    protected virtual void Start() { }

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
        float tempMultiplier = playerThermal != null 
            ? playerThermal.GetCurrentTemperature() / 100f 
            : 1f; // fallback to full range if not found
        
        return (player.transform.position - transform.position).magnitude 
            <= playerDetectionRadius * tempMultiplier;
    }

    public bool IsPlayerInAttackRange() =>
        (player.transform.position - transform.position).magnitude <= attackRadius;

    public void ChangeBaseTemperature(float damage) 
    {
        thermalObject.ChangeBaseTemperature(damage);
        // If temp greater or equal to 100, die
        if (thermalObject.GetTemperature() >= 100f)
            Die();
    }

    protected virtual void Die()
    {
        EnemyDeathExploder exploder = GetComponent<EnemyDeathExploder>();
        if (exploder != null)
            exploder.Explode();
    }
}