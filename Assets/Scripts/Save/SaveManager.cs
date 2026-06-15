using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    private List<ISaveable> saveableObjects = new List<ISaveable>();
    private GameData cachedSaveData;
    private bool hasLoadedSave = false;

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

        LoadSaveFileIntoCache();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F5)) SaveGame();
        if (Input.GetKeyDown(KeyCode.F9)) ReloadAndApplyToAll();
    }

    public void Register(ISaveable saveable)
    {
        if (saveable == null) return;
        if (!saveableObjects.Contains(saveable))
            saveableObjects.Add(saveable);

        // Auto‑apply cached save data to this new object
        if (hasLoadedSave && cachedSaveData != null)
            saveable.Load(cachedSaveData);
    }

    public void Unregister(ISaveable saveable)
    {
        saveableObjects.Remove(saveable);
    }

    public void SaveGame()
    {
        saveableObjects.RemoveAll(item => item == null);

        GameData data = new GameData();
        foreach (var saveable in saveableObjects)
            saveable.Save(data);

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);
        cachedSaveData = data;
        hasLoadedSave = true;
        Debug.Log("Game saved.");
    }

    public void RequestSave()
    {
        SaveGame();
    }

    private void LoadSaveFileIntoCache()
    {
        if (!File.Exists(SavePath))
        {
            cachedSaveData = new GameData();
            hasLoadedSave = true;
            Debug.Log("No save file found, starting fresh.");
            return;
        }

        string json = File.ReadAllText(SavePath);
        cachedSaveData = JsonUtility.FromJson<GameData>(json);
        hasLoadedSave = true;
        //Debug.Log("Save file loaded into cache.");
    }

    public void ReloadAndApplyToAll()
    {
        if (cachedSaveData == null) return;
        saveableObjects.RemoveAll(item => item == null);
        foreach (var saveable in saveableObjects)
            saveable.Load(cachedSaveData);
        Debug.Log("Reloaded save data to all objects.");
    }
}