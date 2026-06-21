using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Spawning")]
    [SerializeField] private List<GameObject> enemiesToSpawn; // all initially inactive
    [SerializeField] private float spawnDelayMin = 0.2f;
    [SerializeField] private float spawnDelayMax = 0.8f;
    [SerializeField] private float initialDelay = 0.5f;

    private bool hasSpawned = false;

    // Public API – call to start spawning
    public void StartSpawning()
    {
        if (hasSpawned) return;
        hasSpawned = true;
        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        yield return new WaitForSeconds(initialDelay);

        foreach (GameObject enemy in enemiesToSpawn)
        {
            if (enemy != null)
            {
                enemy.SetActive(true);
                Debug.Log($"Spawned {enemy.name}");
            }
            float delay = Random.Range(spawnDelayMin, spawnDelayMax);
            yield return new WaitForSeconds(delay);
        }
    }
}