using UnityEngine;

public abstract class EnemyController : MonoBehaviour
{
    protected Rigidbody2D rb;
    protected EnemyState currentState;
    protected ThermalObject thermalObject;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        thermalObject = GetComponentInChildren<ThermalObject>();
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

    // Temperature logic
    public void ChangeBaseTemperature(float damage)
    {
        thermalObject.ChangeBaseTemperature(damage);
    }
}