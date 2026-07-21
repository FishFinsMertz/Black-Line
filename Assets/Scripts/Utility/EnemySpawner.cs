using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class EnemySpawner : MonoBehaviour, ISaveable
{
    [Header("Save ID")]
    [SerializeField] private string saveID;

    [Header("Enemy Spawning")]
    [SerializeField] private List<GameObject> enemiesToSpawn;
    [SerializeField] private float spawnDelayMin = 0.2f;
    [SerializeField] private float spawnDelayMax = 0.8f;
    [SerializeField] private float initialDelay = 0.5f;

    private bool hasSpawned = false;

    private void Start()
    {
        if (!string.IsNullOrEmpty(saveID))
            SaveManager.Instance?.Register(this);
    }

    private void OnDestroy()
    {
        if (!string.IsNullOrEmpty(saveID))
            SaveManager.Instance?.Unregister(this);
    }

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
            }
            float delay = Random.Range(spawnDelayMin, spawnDelayMax);
            yield return new WaitForSeconds(delay);
        }
    }

    // --- ISaveable ---
    public void Save(GameData data)
    {
        if (string.IsNullOrEmpty(saveID)) return;
        data.componentStates.RemoveAll(c => c.id == saveID);
        data.componentStates.Add(new ComponentState { id = saveID, state = hasSpawned ? "Spawned" : "NotSpawned" });
    }

    public void Load(GameData data)
    {
        if (string.IsNullOrEmpty(saveID)) return;
        ComponentState cs = data.componentStates.FirstOrDefault(c => c.id == saveID);
        if (cs != null && cs.state == "Spawned")
        {
            hasSpawned = true;
            foreach (GameObject enemy in enemiesToSpawn)
            {
                if (enemy == null) continue;
                EnemyController ec = enemy.GetComponent<EnemyController>();
                if (ec != null && EnemyManager.Instance.IsEnemyDead(ec.GetUniqueID()))
                    continue;
                enemy.SetActive(true);
            }
        }
    }
}