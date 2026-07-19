using UnityEngine;

public class Button : MonoBehaviour
{
    private Animator animator;
    [SerializeField] private string pressTrigger = "Press";
    [SerializeField] UnityEngine.Events.UnityEvent onPress;

    [Header("Audio")]
    [SerializeField] private AudioClip clickAudio;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void Press()
    {
        animator.SetTrigger(pressTrigger);
        
        if (clickAudio != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayOneShot(clickAudio, transform.position, volumeScale: 0.7f);
        }

        onPress?.Invoke();
    }
}
