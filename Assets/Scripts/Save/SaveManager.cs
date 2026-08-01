using UnityEngine;
using System.IO;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    [SerializeField] private string defaultScene = "MainMenu";

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
        if (Input.GetKeyDown(KeyCode.F9)) LoadGame();
    }

    public void Register(ISaveable saveable)
    {
        if (saveable == null) return;
        if (!saveableObjects.Contains(saveable))
            saveableObjects.Add(saveable);

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

        if (cachedSaveData != null)
        {
            data.collectedAccessItems = new List<string>(cachedSaveData.collectedAccessItems);
            data.componentStates = new List<ComponentState>(cachedSaveData.componentStates);
        }

        foreach (var saveable in saveableObjects)
            saveable.Save(data);

        if (string.IsNullOrEmpty(data.currentScene))
            data.currentScene = SceneManager.GetActiveScene().name;

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);
        cachedSaveData = data;
        hasLoadedSave = true;
        Debug.Log("Game saved.");
    }

    public void LoadGame()
    {
        if (cachedSaveData == null)
        {
            LoadSaveFileIntoCache();
        }

        if (cachedSaveData == null)
        {
            Debug.Log("No save file to load.");
            return;
        }

        if (string.IsNullOrEmpty(cachedSaveData.currentScene))
        {
            Debug.LogWarning("Save file found but saved scene is missing. Loading default scene instead.");
            cachedSaveData.currentScene = defaultScene;
        }

        GameSceneManager sceneManager = FindFirstObjectByType<GameSceneManager>();
        if (sceneManager != null)
        {
            sceneManager.Load(cachedSaveData);
        }
        else
        {
            ReloadAndApplyToAll();
        }
    }

    public void NewGame()
    {
        if (File.Exists(SavePath))
            File.Delete(SavePath);

        cachedSaveData = null;
        hasLoadedSave = false;

        saveableObjects.RemoveAll(item => item == null);

        SceneManager.LoadScene(defaultScene);

        Debug.Log("New game started.");
    }

    public bool HasSaveFile()
    {
        return File.Exists(SavePath);
    }

    public void ReloadAndApplyToAll()
    {
        if (cachedSaveData == null) return;
        saveableObjects.RemoveAll(item => item == null);
        foreach (var saveable in saveableObjects)
            saveable.Load(cachedSaveData);
        Debug.Log("Reloaded save data to all objects.");
    }

    public GameData GetCurrentData() => cachedSaveData;

    private void LoadSaveFileIntoCache()
    {
        if (!File.Exists(SavePath))
        {
            cachedSaveData = null;
            hasLoadedSave = false;
            Debug.Log("No save file found, starting fresh.");
            return;
        }

        string json = File.ReadAllText(SavePath);
        cachedSaveData = JsonUtility.FromJson<GameData>(json);
        hasLoadedSave = true;
        Debug.Log("Save file loaded into cache.");
    }
}