using UnityEngine;

public class ScoutController : EnemyController
{
    [Header("Scout Settings")]
    public float moveSpeed = 3f;
    public float wanderRadius = 4f;
    public float wanderSmoothness = 2f;
    private Vector2 wanderTarget;
}
