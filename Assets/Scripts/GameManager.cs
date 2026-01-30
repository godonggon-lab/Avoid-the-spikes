using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("References")]
    public Camera mainCamera;
    public SpikeSpawner spikeSpawner;
    
    [Header("Background Settings")]
    public SpriteRenderer backgroundSpriteRenderer;
    public Sprite[] stageBackgroundSprites;
    public Sprite[] stageBerrySprites; // Added for per-stage berry sprites
    
    [Header("UI Aesthetics")]
    public Color[] uiColors;

    [Header("UI References")]
    public TMP_Text scoreText;
    public TMP_Text berryText;
    public TMP_Text bestScoreText;   // Added
    public TMP_Text gamesPlayedText; // Added
    public GameObject titlePanel;
    public GameObject gameOverPanel;
    public TMP_InputField nicknameInputField; // Added for User Nickname

    [Header("Level Settings")]
    public int currentStage = 1;
    public int scorePerStage = 15;


    public int score = 0;
    public int totalBerries = 0;
    public int sessionBerries = 0;
    public bool isGameStarted = false;
    public bool isGameOver = false;

    private BirdController bird;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        
        totalBerries = PlayerPrefs.GetInt("TotalBerries", 0);
    }

    private void Start()
    {
        bird = FindObjectOfType<BirdController>();
        if (mainCamera == null) mainCamera = Camera.main;
        


        isGameStarted = false;
        isGameOver = false;
        sessionBerries = 0;
        currentStage = 1;
        
        if (titlePanel != null) titlePanel.SetActive(true);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (scoreText != null) scoreText.gameObject.SetActive(false);

        UpdateBackgroundSprite(0);
        ApplyUIStyle();
        UpdateScoreUI();
        UpdateTitleStats(); // Added
    }





    void UpdateTitleStats()
    {
        int best = PlayerPrefs.GetInt("BestScore", 0);
        int played = PlayerPrefs.GetInt("GamesPlayed", 0);

        if (bestScoreText != null) bestScoreText.text = $"BEST Score: {best}";
        if (gamesPlayedText != null) gamesPlayedText.text = $"Games Played: {played}";
    }

    public void StartGameFromTitle()
    {
        if (isGameStarted) return;
        
        // Increase Games Played count
        int played = PlayerPrefs.GetInt("GamesPlayed", 0);
        played++;
        PlayerPrefs.SetInt("GamesPlayed", played);
        PlayerPrefs.Save();

        isGameStarted = true;
        if (titlePanel != null) titlePanel.SetActive(false);
        if (scoreText != null) scoreText.gameObject.SetActive(true);
        
        Debug.Log("Game Started!");
        if (ItemSpawner.Instance != null)
        {
            ItemSpawner.Instance.SpawnInitialItem();
        }
    }

    public void AddScore()
    {
        if (isGameOver || !isGameStarted) return;
        score++;
        
        CheckStageProgress();
        UpdateScoreUI();
    }

    void CheckStageProgress()
    {
        int newStage = (score / scorePerStage) + 1;
        if (newStage > 10) newStage = 10;

        if (newStage != currentStage)
        {
            currentStage = newStage;
            UpdateBackgroundSprite(currentStage - 1);
            ApplyUIStyle();
        }
    }

    void UpdateBackgroundSprite(int stageIndex)
    {
        if (stageBackgroundSprites == null || stageBackgroundSprites.Length == 0) return;
        if (backgroundSpriteRenderer == null) return;

        int index = stageIndex % stageBackgroundSprites.Length;
        Sprite nextSprite = stageBackgroundSprites[index];
        if (nextSprite != null)
        {
            backgroundSpriteRenderer.sprite = nextSprite;
        }
    }

    public Sprite GetCurrentBerrySprite()
    {
        if (stageBerrySprites == null || stageBerrySprites.Length == 0) return null;
        // 2스테이지마다 바뀌도록 수정 (1~2: 0번, 3~4: 1번 ...)
        int index = ((currentStage - 1) / 2) % stageBerrySprites.Length;
        return stageBerrySprites[index];
    }

    void ApplyUIStyle()
    {
        Color targetColor = Color.white;
        if (uiColors != null && currentStage - 1 < uiColors.Length)
        {
            targetColor = uiColors[currentStage - 1];
        }

        if (scoreText != null)
        {
            Color c = targetColor;
            c.a = 0.6f; 
            scoreText.color = c;
        }
    }

    public void AddBerry()
    {
        if (isGameOver || !isGameStarted) return;
        sessionBerries++;
        totalBerries++;
        PlayerPrefs.SetInt("TotalBerries", totalBerries);
        UpdateScoreUI();
    }

    void UpdateScoreUI()
    {
        if (scoreText != null) 
        {
            scoreText.text = $"{score}";
        }
        if (berryText != null)
        {
            berryText.text = $"Berries: {sessionBerries}";
        }
    }

    public void SpawnSpikes(bool onRightWall)
    {
        if (spikeSpawner != null && !isGameOver)
        {
            spikeSpawner.SpawnPattern(onRightWall);
        }
    }

    public void GameOver()
    {
        if (isGameOver) return;
        isGameOver = true;
        
        // Check Best Score
        int best = PlayerPrefs.GetInt("BestScore", 0);
        if (score > best)
        {
            best = score;
            PlayerPrefs.SetInt("BestScore", best);
            PlayerPrefs.Save();
        }

        // Report to Toss API
        if (TossManager.Instance != null)
        {
            TossManager.Instance.ReportScore(score);
        }
        


        if (gameOverPanel != null) gameOverPanel.SetActive(true);
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // --- Toss API Integration ---

    public void OnRankingButtonClicked()
    {
        if (TossManager.Instance != null)
        {
            TossManager.Instance.ShowLeaderboard();
        }
    }

    public void UpdateNickname()
    {
        if (nicknameInputField != null && !string.IsNullOrEmpty(nicknameInputField.text))
        {
            string name = nicknameInputField.text;
            PlayerPrefs.SetString("UserNickname", name);
            
            if (TossManager.Instance != null)
            {
                TossManager.Instance.SyncNickname(name);
            }
            Debug.Log($"Nickname Updated: {name}");
        }
    }
}
