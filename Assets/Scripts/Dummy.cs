using UnityEngine;

public class Test : MonoBehaviour
{
    public float speed = 5f;

    void Update()
    {
        // Basic WASD movement using direct key checks
        float horizontal = 0f;
        float vertical = 0f;

        if (Input.GetKey(KeyCode.W)) vertical = 1f;
        if (Input.GetKey(KeyCode.S)) vertical = -1f;
        if (Input.GetKey(KeyCode.A)) horizontal = -1f;
        if (Input.GetKey(KeyCode.D)) horizontal = 1f;

        Vector3 move = new Vector2(horizontal, vertical) * speed * Time.deltaTime;
        transform.Translate(move, Space.World); // Use world space to ignore local rotation
    }
}