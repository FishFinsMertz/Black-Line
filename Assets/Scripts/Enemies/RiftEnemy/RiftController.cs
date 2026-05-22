using UnityEngine;

public class RiftController : EnemyController
{
    [Header("Rift Settings")]
    public float lungeSpeed = 10f;
    public float lungeDuration = 1f;
    public float lungeDelay = 0.5f;
    public float bounceSpeed = 5f;
    public float bounceDelay = 0.5f;
    public float chanceToLungeDirectly = 0.1f;

    private bool isUpright = true;

    protected override void Start()
    {
        base.Start();
        ChangeState(new RiftIdleState(this));
    }

    public void FlipVerical()
    {
        isUpright = !isUpright;
        Vector3 scale = transform.localScale;
        scale.y *= -1f;
        transform.localScale = scale;
    }
}
