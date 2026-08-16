using UnityEngine;
using System.Collections;

public class SaveStation : MonoBehaviour
{
    [SerializeField] private float saveDelay = 1.5f;

    public void QuickSave()
    {
        StartCoroutine(DelayedSave());
    }

    private IEnumerator DelayedSave()
    {
        yield return new WaitForSeconds(saveDelay);
        SaveManager.Instance.SaveGame();
        if (NotificationManager.Instance != null)
            NotificationManager.Instance.NotifyBottom("Game Saved...");
    }
}