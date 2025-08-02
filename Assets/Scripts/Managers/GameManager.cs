using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("UI References")]
    public Text scoreText;
    public Text healthText;
    public Text levelText;
    public GameObject gameOverPanel;
    public GameObject pausePanel;
    public Button pauseButton;
    public Button resumeButton;
    public Button restartButton;
    public Button mainMenuButton;
    
    [Header("Game Settings")]
    public int currentLevel = 1;
    public int totalLevels = 3;
    
    private int currentScore = 0;
    private int playerHealth = 3;
    private bool isPaused = false;
    private bool isGameOver = false;
    
    public static GameManager Instance { get; private set; }
    
    public int CurrentScore => currentScore;
    public int PlayerHealth => playerHealth;
    public bool IsPaused => isPaused;
    public bool IsGameOver => isGameOver;
    
    void Awake()
    {
        // Singleton pattern
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
        SetupUI();
        UpdateUI();
        Time.timeScale = 1f;
    }
    
    void SetupUI()
    {
        if (pauseButton != null)
            pauseButton.onClick.AddListener(PauseGame);
        
        if (resumeButton != null)
            resumeButton.onClick.AddListener(ResumeGame);
        
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartLevel);
        
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(GoToMainMenu);
        
        // Hide panels initially
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
        
        if (pausePanel != null)
            pausePanel.SetActive(false);
    }
    
    public void AddScore(int points)
    {
        currentScore += points;
        UpdateUI();
        
        // Check for level completion
        if (currentScore >= GetLevelTargetScore())
        {
            CompleteLevel();
        }
    }
    
    public void UpdateHealthUI(int health)
    {
        playerHealth = health;
        UpdateUI();
    }
    
    void UpdateUI()
    {
        if (scoreText != null)
            scoreText.text = "Score: " + currentScore.ToString();
        
        if (healthText != null)
            healthText.text = "Health: " + playerHealth.ToString();
        
        if (levelText != null)
            levelText.text = "Level: " + currentLevel.ToString();
    }
    
    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
        
        if (pausePanel != null)
            pausePanel.SetActive(true);
        
        AudioManager.Instance?.PlaySound("button_click");
    }
    
    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        
        if (pausePanel != null)
            pausePanel.SetActive(false);
        
        AudioManager.Instance?.PlaySound("button_click");
    }
    
    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        AudioManager.Instance?.PlaySound("button_click");
    }
    
    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
        AudioManager.Instance?.PlaySound("button_click");
    }
    
    public void PlayerDied()
    {
        if (isGameOver) return;
        
        isGameOver = true;
        
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
        
        AudioManager.Instance?.PlaySound("game_over");
    }
    
    public void CompleteLevel()
    {
        if (currentLevel < totalLevels)
        {
            currentLevel++;
            LoadNextLevel();
        }
        else
        {
            // Game completed
            ShowVictoryScreen();
        }
        
        AudioManager.Instance?.PlaySound("level_complete");
    }
    
    void LoadNextLevel()
    {
        string nextLevelName = "Level" + currentLevel;
        SceneManager.LoadScene(nextLevelName);
    }
    
    void ShowVictoryScreen()
    {
        // Load victory scene or show victory UI
        SceneManager.LoadScene("Victory");
    }
    
    int GetLevelTargetScore()
    {
        // Return target score for current level
        return currentLevel * 100;
    }
    
    public void ResetGame()
    {
        currentScore = 0;
        currentLevel = 1;
        playerHealth = 3;
        isPaused = false;
        isGameOver = false;
        Time.timeScale = 1f;
        
        UpdateUI();
    }
    
    void OnApplicationPause(bool pauseStatus)
    {
        // Auto-pause on mobile when app goes to background
        if (pauseStatus && !isPaused && !isGameOver)
        {
            PauseGame();
        }
    }
    
    void OnApplicationFocus(bool hasFocus)
    {
        // Auto-pause on desktop when window loses focus
        if (!hasFocus && !isPaused && !isGameOver)
        {
            PauseGame();
        }
    }
}