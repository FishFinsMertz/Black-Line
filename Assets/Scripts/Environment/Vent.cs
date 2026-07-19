using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Vent : MonoBehaviour, ISaveable
{
    [Header("Save ID (only needed if oneShot is true)")]
    [SerializeField] private string saveID;

    [Header("References")]
    [SerializeField] private PartsExploder partsExploder;
    [SerializeField] private EnemySpawner enemySpawner; //optional

    [Header("Audio")]
    [SerializeField] private AudioClip ventExplodeAudio;

    [Header("Timing Settings")]
    [SerializeField] private float explosionDelayMin = 0f;
    [SerializeField] private float explosionDelayMax = 0f;

    [Header("Behaviour")]
    [SerializeField] private bool oneShot = true;

    private bool hasExploded = false;

    private void Start()
    {
        if (oneShot && !string.IsNullOrEmpty(saveID))
            SaveManager.Instance?.Register(this);
    }

    private void OnDestroy()
    {
        if (oneShot && !string.IsNullOrEmpty(saveID))
            SaveManager.Instance?.Unregister(this);
    }

    public void ExplodeVent()
    {
        if (oneShot && hasExploded) return;
        hasExploded = true;
        StartCoroutine(VentSequence());
    }

    private IEnumerator VentSequence()
    {
        float explosionDelay = Random.Range(explosionDelayMin, explosionDelayMax);
        if (explosionDelay > 0f)
            yield return new WaitForSeconds(explosionDelay);

        if (enemySpawner != null)
        {
            // Detach enemy spawner child
            if (enemySpawner.transform.IsChildOf(transform))
                enemySpawner.transform.SetParent(null);
            
            enemySpawner.StartSpawning();
        }

        if (partsExploder != null)
            partsExploder.Explode();
        
        if (ventExplodeAudio != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayOneShot(ventExplodeAudio, transform.position, volumeScale: 0.35f);
        }
    }

    // --- ISaveable implementation ---
    public void Save(GameData data)
    {
        if (!oneShot || string.IsNullOrEmpty(saveID)) return;
        data.componentStates.RemoveAll(c => c.id == saveID);
        data.componentStates.Add(new ComponentState { id = saveID, state = hasExploded ? "Exploded" : "NotExploded" });
    }

    public void Load(GameData data)
    {
        if (!oneShot || string.IsNullOrEmpty(saveID)) return;
        ComponentState cs = data.componentStates.FirstOrDefault(c => c.id == saveID);
        if (cs != null)
        {
            hasExploded = cs.state == "Exploded";
            if (hasExploded && partsExploder != null)
                partsExploder.HideWithoutExplosion();
        }
    }
}