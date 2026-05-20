using UnityEngine;

public class PlayerController : MonoBehaviour
{

    [Header("Settings")]
    public float walkSpeed = 5f;
    public bool isFacingRight = true;

    [Header("Animators")]
    public Animator bodyAnimator;

    [HideInInspector] public Rigidbody2D rb { get; private set; }

    private PlayerState currentState;
    [HideInInspector] public Inventory inventory;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentState = new PlayerIdleState(this);
        inventory = GetComponent<Inventory>();
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