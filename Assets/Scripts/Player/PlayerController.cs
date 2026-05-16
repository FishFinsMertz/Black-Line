using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float walkSpeed = 5f;
    public bool isFacingRight = true;

    [HideInInspector] public Rigidbody2D rb { get; private set; }

    private PlayerState currentState;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentState = new PlayerIdleState(this);
        currentState.Enter();
    }

    void Update()
    {
        currentState?.Update();
    }

    void FixedUpdate()
    {
        currentState?.FixedUpdate();
    }

    public void Flip()
    {
        isFacingRight = !isFacingRight;

        Vector3 scale = transform.localScale;
        scale.x *= -1f;
        transform.localScale = scale;
    }

    public void ChangeState(PlayerState newState)
    {
        currentState?.Exit();
        currentState = newState;
        currentState?.Enter();
    }
}