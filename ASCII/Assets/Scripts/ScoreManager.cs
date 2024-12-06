using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }
    
    private int currentScore = 0;
    public event System.Action<int> OnScoreChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void IncreaseScore()
    {
        currentScore++;
        OnScoreChanged?.Invoke(currentScore);
        Debug.Log($"Skor arttı: {currentScore}");
    }

    public void DecreaseScore()
    {
        currentScore--;
        OnScoreChanged?.Invoke(currentScore);
        Debug.Log($"Skor azaldı: {currentScore}");
    }

    public int GetCurrentScore()
    {
        return currentScore;
    }
}
