using UnityEngine;

public class ProgressBar : MonoBehaviour
{
    [SerializeField] private float duration = 3f;
    private float currentProgress = 0f;
    private bool isProcessing = false;
    
    public delegate void OnProcessComplete();
    public event OnProcessComplete ProcessCompleted;

    public void StartProcess()
    {
        isProcessing = true;
        currentProgress = 0f;
        gameObject.SetActive(true);
    }

    void Update()
    {
        if (isProcessing)
        {
            currentProgress += Time.deltaTime / duration;
            
            if (TryGetComponent<UnityEngine.UI.Image>(out var image))
            {
                image.fillAmount = currentProgress;
            }

            if (currentProgress >= 1f)
            {
                CompleteProcess();
            }
        }
    }

    private void CompleteProcess()
    {
        isProcessing = false;
        gameObject.SetActive(false);
        ProcessCompleted?.Invoke();
    }
}