using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float walkSpeed = 5f;
    [HideInInspector] public Rigidbody2D rb { get; private set; }
    private PlayerState currentState;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentState = new PlayerIdleState(this);
        currentState.Enter();
    }

    // Update is called once per frame
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
        Vector3 localScale = transform.localScale;
        localScale.x *= -1f;
        transform.localScale = localScale;
    }

    public void ChangeState(PlayerState newState)
    {
        currentState?.Exit();
        currentState = newState;
        currentState?.Enter();
    }
}
