using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameSceneManager : MonoBehaviour, ISaveable
{
    private static GameSceneManager _instance;
    public static GameSceneManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<GameSceneManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("GameSceneManager");
                    _instance = go.AddComponent<GameSceneManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }

    public static event System.Action<string> OnSceneLoadedWithSpawnID;

    private string pendingSpawnID = null;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);

        SaveManager.Instance?.Register(this);
    }

    private void OnDestroy()
    {
        SaveManager.Instance?.Unregister(this);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void LoadScene(string sceneName) => LoadScene(sceneName, null);

    public void LoadScene(string sceneName, string spawnID = null)
    {
        SaveManager.Instance?.SaveGame();
        pendingSpawnID = spawnID;
        SceneManager.LoadScene(sceneName);
    }

    public void LoadScene(int sceneIndex, string spawnID = null)
    {
        SaveManager.Instance?.SaveGame();
        pendingSpawnID = spawnID;
        SceneManager.LoadScene(sceneIndex);
    }

    public void ReloadCurrentScene(string spawnID = null)
    {
        SaveManager.Instance?.SaveGame();
        int currentIndex = SceneManager.GetActiveScene().buildIndex;
        pendingSpawnID = spawnID;
        SceneManager.LoadScene(currentIndex);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(DispatchSceneLoadedNextFrame());
    }

    private IEnumerator DispatchSceneLoadedNextFrame()
    {
        yield return null;

        SaveManager.Instance?.ReloadAndApplyToAll();

        if (!string.IsNullOrEmpty(pendingSpawnID))
        {
            OnSceneLoadedWithSpawnID?.Invoke(pendingSpawnID);
            pendingSpawnID = null;
        }
    }

    public void Save(GameData data)
    {
        data.currentScene = SceneManager.GetActiveScene().name;
    }

    public void Load(GameData data)
    {
        if (data == null || string.IsNullOrEmpty(data.currentScene)) return;

        string targetScene = data.currentScene;
        string currentScene = SceneManager.GetActiveScene().name;

        if (targetScene != currentScene)
        {
            SceneManager.LoadScene(targetScene);
        }
        else
        {
            SaveManager.Instance?.ReloadAndApplyToAll();
        }
    }
}