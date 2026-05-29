using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemyManager : MonoBehaviour, ISaveable
{
    public static EnemyManager Instance { get; private set; }

    [Header("Persistence Settings")]
    [SerializeField] private bool enablePersistence = true; // Turn off for testing

    private HashSet<string> deadEnemies = new HashSet<string>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (enablePersistence)
            SaveManager.Instance?.Register(this);
    }

    public void RegisterDeath(string uniqueID)
    {
        if (!enablePersistence) return;
        if (string.IsNullOrEmpty(uniqueID)) return;
        deadEnemies.Add(uniqueID);
    }

    public bool IsEnemyDead(string uniqueID)
    {
        if (!enablePersistence) return false; // assume alive during tests
        return deadEnemies.Contains(uniqueID);
    }

    public void ReviveEnemy(string uniqueID)
    {
        if (!enablePersistence) return;
        if (deadEnemies.Remove(uniqueID))
            Debug.Log($"EnemyManager: {uniqueID} revived.");
    }

    public void ResetAllDeaths()
    {
        if (!enablePersistence) return;
        deadEnemies.Clear();
        Debug.Log("EnemyManager: All death records cleared.");
    }

    // --- ISaveable implementation ---
    public void Save(GameData data)
    {
        if (!enablePersistence) return;
        data.deadEnemyIDs = new List<string>(deadEnemies);
    }

    public void Load(GameData data)
    {
        if (!enablePersistence) return;
        deadEnemies.Clear();
        if (data.deadEnemyIDs != null)
            deadEnemies.UnionWith(data.deadEnemyIDs);
    }

    private void OnDestroy()
    {
        if (enablePersistence)
            SaveManager.Instance?.Unregister(this);
    }
}