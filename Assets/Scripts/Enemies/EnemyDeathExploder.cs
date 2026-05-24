using UnityEngine;
using System.Collections;

public class EnemyDeathExploder : MonoBehaviour
{
    [Header("Piece Settings")]
    public GameObject[] piecePrefabs;          // assign all piece prefabs
    public Vector2 pieceForceMin = new Vector2(-3f, 2f);
    public Vector2 pieceForceMax = new Vector2(3f, 5f);
    public float pieceTorqueMin = -180f;
    public float pieceTorqueMax = 180f;

    [Header("Gore VFX")]
    public ParticleSystem bloodBurstPrefab;    // assign a prefab with blood particles

    [Header("Lifetime")]
    public float pieceLifetime = 3f;

    public void Explode()
    {
        // Optional: instantiate blood burst at enemy position
        if (bloodBurstPrefab != null)
        {
            ParticleSystem blood = Instantiate(bloodBurstPrefab, transform.position, Quaternion.identity);
            Destroy(blood.gameObject, blood.main.duration);
        }

        // Spawn each piece with random force and torque
        foreach (GameObject piecePrefab in piecePrefabs)
        {
            GameObject piece = Instantiate(piecePrefab, transform.position, Quaternion.identity);
            Rigidbody2D rb = piece.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 force = new Vector2(
                    Random.Range(pieceForceMin.x, pieceForceMax.x),
                    Random.Range(pieceForceMin.y, pieceForceMax.y)
                );
                rb.AddForce(force, ForceMode2D.Impulse);
                float torque = Random.Range(pieceTorqueMin, pieceTorqueMax);
                rb.AddTorque(torque);
            }
            // Optionally add a fade out script and destroy after lifetime
            Destroy(piece, pieceLifetime);
        }

        // Disable the main enemy (or destroy it)
        gameObject.SetActive(false);
    }
}