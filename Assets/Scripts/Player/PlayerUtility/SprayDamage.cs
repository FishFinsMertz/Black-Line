using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(ParticleSystem))]
public class SprayDamage : MonoBehaviour
{
    [Header("Damage Settings")]
    [SerializeField] private float damagePerHit = 10f;
    [SerializeField] private float hitCooldown = 0.5f;

    [Header("Detection")]
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private float detectionRadius = 0.1f;

    private ParticleSystem ps;
    private Dictionary<GameObject, float> lastHitTime = new Dictionary<GameObject, float>();

    private void Start()
    {
        ps = GetComponent<ParticleSystem>();
    }

    private void Update()
    {
        // Clean up destroyed enemies
        List<GameObject> toRemove = new List<GameObject>();
        foreach (var kvp in lastHitTime)
            if (kvp.Key == null) toRemove.Add(kvp.Key);
        foreach (var go in toRemove)
            lastHitTime.Remove(go);

        if (ps.particleCount == 0) return;

        // Manual particle overlap check
        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[ps.particleCount];
        int count = ps.GetParticles(particles);

        for (int i = 0; i < count; i++)
        {
            Vector2 pos = particles[i].position;
            Collider2D hit = Physics2D.OverlapCircle(pos, detectionRadius, enemyLayer);
            if (hit == null) continue;

            ThermalObject thermal = hit.GetComponentInParent<ThermalObject>();
            if (thermal == null)
                thermal = hit.GetComponentInChildren<ThermalObject>();
            if (thermal == null) continue;

            float currentTime = Time.time;
            if (lastHitTime.TryGetValue(hit.gameObject, out float lastHit))
                if (currentTime - lastHit < hitCooldown) continue;

            thermal.ChangeBaseTemperature(-damagePerHit);
            //Debug.Log($"Hit {hit.gameObject.name} for {damagePerHit} damage. New temp: {thermal.GetTemperature()}");
            lastHitTime[hit.gameObject] = currentTime;
        }
    }
}