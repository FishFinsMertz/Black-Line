using UnityEngine;
using System.Collections;

public class RiftController : EnemyController
{
    [Header("Lunge Settings")]
    public float lungeSpeed = 10f;
    public float lungeDuration = 1f;
    public float lungeDelay = 0.5f;

    [Header("Bounce Settings")]
    public float bounceSpeed = 5f;
    public float bounceDelay = 0.5f;
    public float chanceToLungeDirectly = 0.1f;
    public int bounceSegments = 3;

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

    // Coroutines
    public IEnumerator BounceRoutine(float t)
    {
        yield return new WaitForSeconds(t);
    }
}
