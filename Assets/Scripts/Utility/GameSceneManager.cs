using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSceneManager : MonoBehaviour
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
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void LoadScene(string sceneName)
    {
        LoadScene(sceneName, null);
    }

    public void LoadScene(string sceneName, string spawnID = null)
    {
        pendingSpawnID = spawnID;
        SceneManager.LoadScene(sceneName);
    }

    public void LoadScene(int sceneIndex, string spawnID = null)
    {
        pendingSpawnID = spawnID;
        SceneManager.LoadScene(sceneIndex);
    }

    public void ReloadCurrentScene(string spawnID = null)
    {
        int currentIndex = SceneManager.GetActiveScene().buildIndex;
        pendingSpawnID = spawnID;
        SceneManager.LoadScene(currentIndex);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!string.IsNullOrEmpty(pendingSpawnID))
        {
            OnSceneLoadedWithSpawnID?.Invoke(pendingSpawnID);
            pendingSpawnID = null;
        }
    }
}