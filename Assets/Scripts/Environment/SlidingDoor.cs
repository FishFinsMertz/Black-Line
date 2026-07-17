using UnityEngine;
using System.Collections;

public class SlidingDoor : MonoBehaviour, IInteractible
{
    [Header("References")]
    [SerializeField] private BoxCollider2D doorCollider;
    [SerializeField] private Animator animator;
    [SerializeField] private float doorCollisionDelay = 0.5f;
    private Collider2D detectionCollider;
    private bool isOpen = false;
    private Coroutine pendingCollisionCoroutine = null;
    private bool interactionEnabled = true;

    [Header("Audio")]
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip closeSound;

    private void Start()
    {
        detectionCollider = GetComponent<Collider2D>();
        if (detectionCollider != null)
            detectionCollider.isTrigger = true;

        if (doorCollider != null)
            doorCollider.enabled = true;
        else
            Debug.LogError("SlidingDoor: doorCollider not assigned!", this);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isOpen && interactionEnabled)
        {
            OpenDoor();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && isOpen && interactionEnabled)
        {
            CloseDoor();
        }
    }

    private void OpenDoor()
    {
        if (isOpen) return;
        isOpen = true;
        animator.SetTrigger("Open");

        if (openSound != null && AudioManager.Instance != null)
            AudioManager.Instance.PlayOneShot(openSound, transform.position);

        if (pendingCollisionCoroutine != null)
            StopCoroutine(pendingCollisionCoroutine);

        pendingCollisionCoroutine = StartCoroutine(UpdateCollisionAfterDelay());
    }

    private void CloseDoor()
    {
        if (!isOpen) return;
        isOpen = false;
        animator.SetTrigger("Close");

        if (closeSound != null && AudioManager.Instance != null)
            AudioManager.Instance.PlayOneShot(closeSound, transform.position);

        if (pendingCollisionCoroutine != null)
            StopCoroutine(pendingCollisionCoroutine);

        pendingCollisionCoroutine = StartCoroutine(UpdateCollisionAfterDelay());
    }

    private IEnumerator UpdateCollisionAfterDelay()
    {
        yield return new WaitForSeconds(doorCollisionDelay);
        if (doorCollider != null)
            doorCollider.enabled = !isOpen;
        pendingCollisionCoroutine = null;
    }

    public void EnableInteraction()
    {
        interactionEnabled = true;
    }

    public void DisableInteraction()
    {
        interactionEnabled = false;
    }
}