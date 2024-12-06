using UnityEngine;
public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("Level Ayarları")]
    public int currentLevel = 1;
    public int maxLevel = 10;
    public int baseNPCCount = 3;
    public int npcIncreasePerLevel = 2;

    private int remainingNPCsInLevel;
    private int totalNPCsForCurrentLevel;
    private bool isLevelActive = false;
    private int spawnedNPCsInCurrentLevel = 0;

    void Awake()
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

    void Start()
    {
        StartLevel(currentLevel);
    }

    public void StartLevel(int level)
    {
        if (level > maxLevel)
        {
            Debug.Log("Tüm leveller tamamlandı!");
            DoMaxLevelStuff();
            return;
        }

        currentLevel = level;
        totalNPCsForCurrentLevel = CalculateNPCCountForLevel(level);
        remainingNPCsInLevel = totalNPCsForCurrentLevel;
        spawnedNPCsInCurrentLevel = 0;
        isLevelActive = true;

        Debug.Log($"Level {level} başladı! Bu level için {totalNPCsForCurrentLevel} NPC spawn edilecek.");
        
        NPCManager.Instance.ResetForNewLevel();
    }

    private void DoMaxLevelStuff()
    {
        Debug.Log("Max level stuff");

        ScoreManager.Instance.DoMaxLevelStuff();
    }

    private int CalculateNPCCountForLevel(int level)
    {
        return baseNPCCount + (level - 1) * npcIncreasePerLevel;
    }

    public void OnNPCCompleted()
    {
        remainingNPCsInLevel--;
        
        if (remainingNPCsInLevel <= 0 && isLevelActive)
        {
            CompleteLevel();
        }
    }

    private void CompleteLevel()
    {
        isLevelActive = false;
        Debug.Log($"Level {currentLevel} tamamlandı!");
        
        UIManager.Instance.ShowLevelCompleteUI();
    }
    
    public void StartNextLevel()
    {
        StartLevel(currentLevel + 1);
    }

    public bool CanSpawnNPC()
    {
        if (!isLevelActive) return false;
        if (spawnedNPCsInCurrentLevel >= totalNPCsForCurrentLevel) return false;
        
        spawnedNPCsInCurrentLevel++;
        return true;
    }
}