using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(ParticleSystem))]
public class SprayDamage : MonoBehaviour
{
    [Header("Damage Settings")]
    [SerializeField] private float damagePerHit = 10f;
    [SerializeField] private float hitCooldown = 0.5f;

    [Header("Player Settings")]
    [SerializeField] private bool affectPlayer = true;
    [SerializeField] private float playerTempChange = -5f;

    [Header("Camera Shake")]
    [SerializeField] private bool shakeOnPlayerHit = true;
    [SerializeField] private float shakeIntensity = 0.2f;
    [SerializeField] private float shakeDuration = 0.2f;
    [SerializeField] private float shakeSmoothness = 0.5f;

    [Header("Detection")]
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private float detectionRadius = 0.1f;

    [Header("Performance")]
    [SerializeField] private int maxParticles = 1000;

    private ParticleSystem ps;
    private ParticleSystem.Particle[] particles;
    private Dictionary<GameObject, float> lastHitTime = new Dictionary<GameObject, float>();
    private CameraController camController;

    private void Start()
    {
        ps = GetComponent<ParticleSystem>();
        particles = new ParticleSystem.Particle[maxParticles];
        camController = Camera.main.GetComponent<CameraController>();
    }

    private void Update()
    {
        // Clean up destroyed references
        List<GameObject> toRemove = new List<GameObject>();
        foreach (var kvp in lastHitTime)
            if (kvp.Key == null) toRemove.Add(kvp.Key);
        foreach (var go in toRemove)
            lastHitTime.Remove(go);

        if (ps.particleCount == 0) return;

        int count = ps.GetParticles(particles);
        if (count > maxParticles) count = maxParticles;

        for (int i = 0; i < count; i++)
        {
            Vector2 pos = particles[i].position;
            float currentTime = Time.time;

            // 1) Check for player (using player layer)
            if (affectPlayer)
            {
                Collider2D playerHit = Physics2D.OverlapCircle(pos, detectionRadius, playerLayer);
                if (playerHit != null && playerHit.CompareTag("Player"))
                {
                    GeneralThermalRegulator thermal = playerHit.GetComponent<GeneralThermalRegulator>();
                    if (thermal != null)
                    {
                        if (lastHitTime.TryGetValue(playerHit.gameObject, out float last))
                            if (currentTime - last < hitCooldown) goto SkipPlayer;
                        thermal.ChangeGlobalBaseTemperature(playerTempChange);
                        lastHitTime[playerHit.gameObject] = currentTime;

                        // Trigger camera shake when player is hit
                        if (shakeOnPlayerHit && camController != null)
                            camController.TriggerShake(shakeIntensity, shakeDuration, shakeSmoothness);
                    }
                    SkipPlayer:;
                }
            }

            // 2) Check for enemies (using enemy layer)
            Collider2D hit = Physics2D.OverlapCircle(pos, detectionRadius, enemyLayer);
            if (hit == null) continue;

            EnemyController enemy = hit.GetComponentInParent<EnemyController>();
            if (enemy == null) enemy = hit.GetComponentInChildren<EnemyController>();
            if (enemy == null) continue;

            if (lastHitTime.TryGetValue(enemy.gameObject, out float lastHit))
                if (currentTime - lastHit < hitCooldown) continue;

            enemy.ChangeBaseTemperature(damagePerHit);
            lastHitTime[enemy.gameObject] = currentTime;
        }
    }
}