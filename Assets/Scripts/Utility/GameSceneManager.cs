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

    [Header("Scene Transition")]
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private float fadeDuration = 1f;

    private string pendingSpawnID = null;
    private bool isTransitioning = false;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);

        if (fadeCanvasGroup == null)
        {
            GameObject canvasObj = new GameObject("FadeCanvas");
            canvasObj.transform.SetParent(transform);
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999;
            canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            GameObject panel = new GameObject("Panel");
            panel.transform.SetParent(canvasObj.transform);
            UnityEngine.UI.Image image = panel.AddComponent<UnityEngine.UI.Image>();
            image.color = Color.black;
            image.rectTransform.anchorMin = Vector2.zero;
            image.rectTransform.anchorMax = Vector2.one;
            image.rectTransform.sizeDelta = Vector2.zero;

            fadeCanvasGroup = panel.AddComponent<CanvasGroup>();
        }

        fadeCanvasGroup.alpha = 0f;
        fadeCanvasGroup.blocksRaycasts = false;
        DontDestroyOnLoad(fadeCanvasGroup.gameObject);

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

    public void LoadScene(string sceneName) => LoadScene(sceneName, null, true);

    public void LoadScene(string sceneName, string spawnID = null, bool saveBeforeLoad = true)
    {
        if (isTransitioning) return;
        if (saveBeforeLoad)
            SaveManager.Instance?.SaveGame();
        StartCoroutine(TransitionCoroutine(sceneName, spawnID));
    }

    public void LoadScene(int sceneIndex, string spawnID = null, bool saveBeforeLoad = true)
    {
        if (isTransitioning) return;
        if (saveBeforeLoad)
            SaveManager.Instance?.SaveGame();
        StartCoroutine(TransitionCoroutine(sceneIndex, spawnID));
    }

    public void ReloadCurrentScene(string spawnID = null, bool saveBeforeLoad = true)
    {
        if (isTransitioning) return;
        if (saveBeforeLoad)
            SaveManager.Instance?.SaveGame();
        int currentIndex = SceneManager.GetActiveScene().buildIndex;
        StartCoroutine(TransitionCoroutine(currentIndex, spawnID));
    }

    private IEnumerator TransitionCoroutine(string sceneName, string spawnID)
    {
        isTransitioning = true;
        pendingSpawnID = spawnID;

        yield return Fade(0f, 1f, fadeDuration);

        SceneManager.LoadScene(sceneName);
    }

    private IEnumerator TransitionCoroutine(int sceneIndex, string spawnID)
    {
        isTransitioning = true;
        pendingSpawnID = spawnID;

        yield return Fade(0f, 1f, fadeDuration);

        SceneManager.LoadScene(sceneIndex);
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        if (fadeCanvasGroup == null) yield break;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }
        fadeCanvasGroup.alpha = to;
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

        if (isTransitioning)
        {
            yield return Fade(1f, 0f, fadeDuration);
            fadeCanvasGroup.blocksRaycasts = false;
            isTransitioning = false;
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
            if (isTransitioning) return;
            StartCoroutine(TransitionCoroutine(targetScene, null));
        }
        else
        {
            SaveManager.Instance?.ReloadAndApplyToAll();
        }
    }
}