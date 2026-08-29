using UnityEngine;

public class SceneTeleporter : MonoBehaviour
{
    [SerializeField] private string targetSceneName;
    [SerializeField] private string spawnID;

    [Header("Spawn Position")]
    [SerializeField] private Transform exitPoint;

    [Header("Optional: Location Notification")]
    [SerializeField] private string locationName;
    [SerializeField] private string temperature;

    public string SpawnID => spawnID;

    public Vector3 GetSpawnPosition()
    {
        return exitPoint != null ? exitPoint.position : transform.position;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            GameSceneManager.Instance.LoadScene(targetSceneName, spawnID);
            if (!string.IsNullOrEmpty(locationName) && !string.IsNullOrEmpty(temperature))
            {
                NotificationManager.Instance.NotifyLocation(locationName, temperature);
            }
        }
    }
}