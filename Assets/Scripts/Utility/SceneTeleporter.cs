using UnityEngine;

public class SceneTeleporter : MonoBehaviour
{
    [SerializeField] private string targetSceneName;
    [SerializeField] private string spawnID;

    [Header("Spawn Position")]
    [SerializeField] private Transform exitPoint;

    public string SpawnID => spawnID;

    public Vector3 GetSpawnPosition()
    {
        return exitPoint != null ? exitPoint.position : transform.position;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            SaveManager.Instance?.SaveGame();
            GameSceneManager.Instance.LoadScene(targetSceneName, spawnID);
        }
    }
}