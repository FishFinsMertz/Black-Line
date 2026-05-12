using UnityEngine;

public class CameraController : MonoBehaviour
{       
    [Header("Follow Settings")]
    [SerializeField] private float smoothSpeed = 5f;     
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f); 

    private Transform target;      
    private string defaultTargetTag = "Player"; 
    
    private void Start()
    {
        // If no target assigned, try to find one with default tag
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag(defaultTargetTag);
            if (player != null)
                target = player.transform;
            else
                Debug.LogWarning($"No object with tag '{defaultTargetTag}' found for camera to follow.");
        }
    }
    
    private void LateUpdate()
    {
        if (target == null) return;
        
        // Desired position based on target + offset
        Vector3 desiredPosition = target.position + offset;
        
        // Smoothly interpolate to desired position
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
    }
    
    // Public method to change target during runtime
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
    
    // Public method to reset to player by tag
    public void ResetToPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag(defaultTargetTag);
        if (player != null)
            target = player.transform;
        else
            Debug.LogWarning($"No object with tag '{defaultTargetTag}' found.");
    }
}