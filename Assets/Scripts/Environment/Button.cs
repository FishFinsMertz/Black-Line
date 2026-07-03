using UnityEngine;

public class Button : MonoBehaviour
{
    private Animator animator;
    [SerializeField] private string pressTrigger = "Press";
    [SerializeField] UnityEngine.Events.UnityEvent onPress;
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void Press()
    {
        animator.SetTrigger(pressTrigger);
        onPress?.Invoke();
    }
}
