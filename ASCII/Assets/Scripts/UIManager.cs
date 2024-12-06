using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Level Complete UI")]
    public GameObject levelCompletePanel;
    public Button nextLevelButton;
    public TextMeshProUGUI levelCompletedText;
    public TextMeshProUGUI scoreText;

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

    private void Start()
    {
        levelCompletePanel.SetActive(false);
        nextLevelButton.onClick.AddListener(OnNextLevelButtonClicked);
        
        ScoreManager.Instance.OnScoreChanged += UpdateScoreText;
        
        UpdateScoreText(ScoreManager.Instance.GetCurrentScore());
    }

    public void ShowLevelCompleteUI()
    {
        levelCompletePanel.SetActive(true);
        Time.timeScale = 0f;
        levelCompletedText.text = $"Level {LevelManager.Instance.currentLevel} Completed!";
    }

    private void OnNextLevelButtonClicked()
    {
        levelCompletePanel.SetActive(false);
        Time.timeScale = 1f;
        LevelManager.Instance.StartNextLevel();
    }
    
    private void UpdateScoreText(int score)
    {
        scoreText.text = $"Score: {score}";
    }
}
