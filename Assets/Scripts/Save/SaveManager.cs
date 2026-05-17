using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    private List<ISaveable> saveableObjects = new List<ISaveable>();

    private string SavePath => Application.persistentDataPath + "/game_save.json";

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

    private void Update()
    {
        //Testing purposes
        if (Input.GetKeyDown(KeyCode.F5)) SaveGame();
        if (Input.GetKeyDown(KeyCode.F9)) LoadGame();
    }

    // Register a saveable component (called from each saveable object's Start)
    public void Register(ISaveable saveable)
    {
        if (!saveableObjects.Contains(saveable))
            saveableObjects.Add(saveable);
    }

    // Unregister if needed (e.g., destroyed object)
    public void Unregister(ISaveable saveable)
    {
        saveableObjects.Remove(saveable);
    }

    public void SaveGame()
    {
        GameData data = new GameData();
        foreach (var saveable in saveableObjects)
            saveable.Save(data);
        
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);
        Debug.Log("Game saved.");
    }

    public void LoadGame()
    {
        if (!File.Exists(SavePath))
        {
            Debug.Log("No save file found.");
            return;
        }

        string json = File.ReadAllText(SavePath);
        GameData data = JsonUtility.FromJson<GameData>(json);
        
        foreach (var saveable in saveableObjects)
            saveable.Load(data);
        
        Debug.Log("Game loaded.");
    }

    public void RequestSave()
    {
        // Optional: delayed save or cooldown
        SaveGame();
    }
}