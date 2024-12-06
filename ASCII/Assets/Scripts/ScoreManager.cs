using UnityEngine;
using LootLocker.Requests;
using UnityEngine.UI;
using TMPro;
using Michsky.MUIP;
using UnityEngine.SceneManagement;


public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    private int currentScore = 0;
    public event System.Action<int> OnScoreChanged;
    public bool isSessionStarted = false;
    public GameObject loginPanel;
    public GameObject maxLevelPanel;
    public GameObject spinner;
    public TMP_InputField usernameInput;

    private string leaderboardKey = "globalLeaderboard";
    public ListView leaderboardList;
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
        Time.timeScale = 0;
        maxLevelPanel.SetActive(false);
        loginPanel.SetActive(true);

        LootLockerSDKManager.StartGuestSession((response) =>
        {
            if (!response.success)
            {
                Debug.Log("error starting LootLocker session");

                return;
            }

            Debug.Log("successfully started LootLocker session");
            isSessionStarted = true;
            checkUsername();

            //getTopScores(); test için
        });
    }

    private void checkUsername()
    {
        Debug.Log("Checking existing username");
        LootLockerSDKManager.GetPlayerName((response) =>
        {
            Debug.Log("Checking existing username response: " + response.success);
            if (response.success)
            {
                Debug.Log("Checking existing username response success " + response.name);
                if (!string.IsNullOrEmpty(response.name))
                {
                    Debug.Log("Existing username found: " + response.name);
                    if (loginPanel != null)
                        loginPanel.SetActive(false);

                    StartGame();
                }
                else
                {
                    Debug.Log("No existing username found");
                }
            }
            else
            {
                Debug.Log("No existing username found");
            }
        });
    }

    public void SetUsername()
    {
        if (string.IsNullOrEmpty(usernameInput.text))
        {
            Debug.Log("Username cannot be empty");
            return;
        }

        LootLockerSDKManager.SetPlayerName(usernameInput.text, (response) =>
        {
            if (response.success)
            {
                Debug.Log("Successfully set player name: " + usernameInput.text);
                if (loginPanel != null)
                    loginPanel.SetActive(false);

                StartGame();
            }
            else
            {
                Debug.Log("Error setting player name");
            }
        });
    }

    private void StartGame()
    {
        Time.timeScale = 1;
        Debug.Log("Game started");
        TarifCanavari.Instance.GenerateRecipe();
    }

    public void DoMaxLevelStuff()
    {
        SubmitScore();
        //getTopScores();
    }

    private void getTopScores()
    {
        Debug.Log("Getting top scores");
        LootLockerSDKManager.GetScoreList(leaderboardKey, 10, 0, (response) =>
        {
            if (response.success)
            {
                Debug.Log("Top scores fetched successfully " + response.items.Length);
                LootLockerLeaderboardMember[] members = response.items;

                ListView.ListItem placeHolder = new ListView.ListItem();

                placeHolder.row0 = new ListView.ListRow();
                placeHolder.row0.rowText = "Derece";
                placeHolder.row0.usePreferredWidth = false;

                placeHolder.row1 = new ListView.ListRow();
                placeHolder.row1.rowText = "İsim";
                placeHolder.row1.usePreferredWidth = false;

                placeHolder.row2 = new ListView.ListRow();
                placeHolder.row2.rowText = "Skor";
                placeHolder.row2.usePreferredWidth = false;

                leaderboardList.listItems.Add(placeHolder);
                leaderboardList.rowCount = ListView.RowCount.Three;

                foreach (var member in members)
                {
                    Debug.Log(member.score + " - " + member.player.name);

                    ListView.ListItem item = new ListView.ListItem();

                    item.row0 = new ListView.ListRow();
                    item.row0.rowText = member.rank.ToString();
                    item.row0.usePreferredWidth = false;

                    item.row1 = new ListView.ListRow();
                    item.row1.rowText = member.player.name;
                    item.row1.usePreferredWidth = false;

                    item.row2 = new ListView.ListRow();
                    item.row2.rowText = member.score.ToString();
                    item.row2.usePreferredWidth = false;

                    leaderboardList.listItems.Add(item);
                    leaderboardList.rowCount = ListView.RowCount.Three;
                }

                leaderboardList.InitializeItems();
                maxLevelPanel.SetActive(true);
            }
        });
    }

    public void SubmitScore()
    {
        if (currentScore < 0)
        {
            currentScore = 0;
        }
        
        LootLockerSDKManager.SubmitScore("", currentScore, leaderboardKey, (response) =>
        {
            if (!response.success)
            {
                Debug.Log("Could not submit score!");
                Debug.Log(response.errorData.ToString());
                return;
            }
            else
            {
                Debug.Log($"Submitted score: {currentScore}");
            }
            getTopScores();
        });
    }

    public void QuitGame()
    {
        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #else
                Application.Quit();
        #endif
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        NPCManager.Instance.ResetNPCs();
        TarifCanavari.Instance.GenerateRecipe();
    }

    public void ResetScore()
    {
        currentScore = 0;
        OnScoreChanged?.Invoke(currentScore);
    }

    public void IncreaseScore()
    {
        currentScore++;
        OnScoreChanged?.Invoke(currentScore);
        Debug.Log($"Skor arttı: {currentScore}");
    }

    public void DecreaseScore()
    {
        if (currentScore > 0)
        {
            currentScore--;
        }
        OnScoreChanged?.Invoke(currentScore);
        Debug.Log($"Skor azaldı: {currentScore}");
    }

    public int GetCurrentScore()
    {
        return currentScore;
    }
}
