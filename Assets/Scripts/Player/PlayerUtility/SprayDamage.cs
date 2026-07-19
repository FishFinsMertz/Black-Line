using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(ParticleSystem))]
public class SprayDamage : MonoBehaviour
{
    [Header("Damage Settings")]
    [SerializeField] private float enemyTempChange = 10f;
    [SerializeField] private float playerTempChange = -5f;
    [SerializeField] private float hitCooldown = 0.5f;

    [Header("Detection")]
    [SerializeField] private LayerMask affectedLayerMask;
    [SerializeField] private float detectionRadius = 0.1f;

    [Header("Performance")]
    [SerializeField] private int maxParticles = 1000;

    private ParticleSystem ps;
    private ParticleSystem.Particle[] particles;
    private Dictionary<GameObject, float> lastHitTime = new Dictionary<GameObject, float>();

    private void Start()
    {
        ps = GetComponent<ParticleSystem>();
        particles = new ParticleSystem.Particle[maxParticles];
    }

    private void Update()
    {
        CleanupDeadEntries();

        if (ps.particleCount == 0) return;

        int count = ps.GetParticles(particles);
        if (count > maxParticles) count = maxParticles;

        for (int i = 0; i < count; i++)
        {
            Vector2 pos = particles[i].position;
            float currentTime = Time.time;

            Collider2D hit = Physics2D.OverlapCircle(pos, detectionRadius, affectedLayerMask);
            if (hit == null) continue;

            GameObject hitObject = hit.gameObject;

            if (lastHitTime.TryGetValue(hitObject, out float lastHit))
                if (currentTime - lastHit < hitCooldown) continue;

            ITemperatureChangeable tempChangeable = hitObject.GetComponent<ITemperatureChangeable>();
            if (tempChangeable == null) continue;

            bool isPlayer = hitObject.CompareTag("Player");
            if (isPlayer)
            {
                PlayerController pc = hitObject.GetComponent<PlayerController>();
                if (pc != null)
                    pc.TakeDamage(playerTempChange);
                else
                    tempChangeable.ChangeBaseTemperature(playerTempChange);
            }
            else
            {
                tempChangeable.ChangeBaseTemperature(enemyTempChange);
            }

            lastHitTime[hitObject] = currentTime;
        }
    }

    private void CleanupDeadEntries()
    {
        List<GameObject> toRemove = new List<GameObject>();
        foreach (var kvp in lastHitTime)
            if (kvp.Key == null) toRemove.Add(kvp.Key);
        foreach (var go in toRemove)
            lastHitTime.Remove(go);
    }
}