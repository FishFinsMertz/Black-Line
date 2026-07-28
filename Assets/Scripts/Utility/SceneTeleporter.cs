using UnityEngine;

public class SceneTeleporter : MonoBehaviour
{
    [SerializeField] private string targetSceneName;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            SaveManager.Instance?.SaveGame();
            GameSceneManager.Instance.LoadScene(targetSceneName);
        }
    }
}