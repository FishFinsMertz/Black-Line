using UnityEngine;

public abstract class EnemyController : MonoBehaviour
{
    [Header("Enemy Stats")]
    [SerializeField] protected float detectionRadius = 5f;
    [SerializeField] protected float attackRadius = 1f;

    public Rigidbody2D rb;
    protected EnemyState currentState;
    protected ThermalObject thermalObject;
    protected GameObject player;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        thermalObject = GetComponentInChildren<ThermalObject>();
        player = GameObject.FindGameObjectWithTag("Player");
    }

    protected virtual void Start() { } // subclass sets initial state

    protected virtual void Update()
    {
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

    public void Flip()
    {
        Vector3 scale = transform.localScale;
        scale.x *= -1f;
        transform.localScale = scale;
    }

    // Locational Logic
    public bool IsPlayerInDetectionRange()
    {
        Vector2 direction = player.transform.position - transform.position;
        return direction.magnitude <= detectionRadius;
    }

    public bool IsPlayerInAttackRange()
    {
        Vector2 direction = player.transform.position - transform.position;
        return direction.magnitude <= attackRadius;
    }

    // Temperature logic
    public void ChangeBaseTemperature(float damage)
    {
        thermalObject.ChangeBaseTemperature(damage);
    }
}