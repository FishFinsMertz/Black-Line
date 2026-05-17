using UnityEngine;

public class SceneTeleporter : MonoBehaviour
{
    [SerializeField] private string targetSceneName;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Save game before teleporting
            SaveManager.Instance?.SaveGame();
            
            // Load the target scene
            UnityEngine.SceneManagement.SceneManager.LoadScene(targetSceneName);
        }
    }
}
