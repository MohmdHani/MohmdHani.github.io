using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [Header("Game Settings")]
    [SerializeField] private int currentLevel = 1;
    [SerializeField] private int totalLevels = 10;
    [SerializeField] private float gameTime = 0f;
    [SerializeField] private bool isPaused = false;
    
    [Header("Player Settings")]
    [SerializeField] private int playerLives = 3;
    [SerializeField] private int playerScore = 0;
    [SerializeField] private int coinsCollected = 0;
    
    [Header("Mobile Settings")]
    [SerializeField] private bool enableVibration = true;
    [SerializeField] private bool enableSound = true;
    [SerializeField] private float vibrationDuration = 0.1f;
    
    [Header("UI References")]
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject gameOverMenu;
    [SerializeField] private GameObject levelCompleteMenu;
    
    // Events
    public System.Action<int> OnScoreChanged;
    public System.Action<int> OnLivesChanged;
    public System.Action<int> OnCoinsChanged;
    public System.Action OnGamePaused;
    public System.Action OnGameResumed;
    public System.Action OnGameOver;
    public System.Action OnLevelComplete;
    
    // Game state
    private bool isGameActive = false;
    private bool isLevelComplete = false;
    private Vector3 playerStartPosition;
    private PlayerController playerController;
    
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
            return;
        }
        
        // Initialize mobile settings
        InitializeMobileSettings();
    }
    
    void Start()
    {
        StartGame();
    }
    
    void Update()
    {
        if (isGameActive && !isPaused)
        {
            gameTime += Time.deltaTime;
        }
        
        // Handle pause input
        HandlePauseInput();
    }
    
    void InitializeMobileSettings()
    {
        // Set target frame rate for mobile
        Application.targetFrameRate = 60;
        
        // Enable mobile input
        Input.multiTouchEnabled = false;
        
        // Load saved settings
        enableVibration = PlayerPrefs.GetInt("Vibration", 1) == 1;
        enableSound = PlayerPrefs.GetInt("Sound", 1) == 1;
        
        // Set audio listener volume
        AudioListener.volume = enableSound ? 1f : 0f;
    }
    
    void HandlePauseInput()
    {
        // Pause on back button (Android) or escape key
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Menu))
        {
            TogglePause();
        }
    }
    
    public void StartGame()
    {
        isGameActive = true;
        isPaused = false;
        gameTime = 0f;
        
        // Find player
        playerController = FindObjectOfType<PlayerController>();
        if (playerController)
        {
            playerStartPosition = playerController.transform.position;
        }
        
        // Hide menus
        if (pauseMenu) pauseMenu.SetActive(false);
        if (gameOverMenu) gameOverMenu.SetActive(false);
        if (levelCompleteMenu) levelCompleteMenu.SetActive(false);
        
        // Resume game
        Time.timeScale = 1f;
    }
    
    public void PauseGame()
    {
        if (!isGameActive) return;
        
        isPaused = true;
        Time.timeScale = 0f;
        
        if (pauseMenu) pauseMenu.SetActive(true);
        
        OnGamePaused?.Invoke();
    }
    
    public void ResumeGame()
    {
        if (!isGameActive) return;
        
        isPaused = false;
        Time.timeScale = 1f;
        
        if (pauseMenu) pauseMenu.SetActive(false);
        
        OnGameResumed?.Invoke();
    }
    
    public void TogglePause()
    {
        if (isPaused)
            ResumeGame();
        else
            PauseGame();
    }
    
    public void GameOver()
    {
        if (!isGameActive) return;
        
        isGameActive = false;
        playerLives--;
        
        OnLivesChanged?.Invoke(playerLives);
        
        if (enableVibration)
        {
            Handheld.Vibrate();
        }
        
        if (playerLives <= 0)
        {
            // Game over
            if (gameOverMenu) gameOverMenu.SetActive(true);
            OnGameOver?.Invoke();
        }
        else
        {
            // Restart level
            RestartLevel();
        }
    }
    
    public void LevelComplete()
    {
        if (isLevelComplete) return;
        
        isLevelComplete = true;
        isGameActive = false;
        
        // Calculate level score and stars
        int levelScore = CalculateLevelScore();
        int stars = CalculateStars();
        AddScore(levelScore);
        
        // Save progress to level progress manager
        if (LevelProgressManager.Instance != null)
        {
            LevelProgressManager.Instance.CompleteLevel(
                currentLevel,
                stars,
                gameTime,
                playerScore,
                coinsCollected,
                true, // allCoins - you can track this separately
                true  // allEnemies - you can track this separately
            );
        }
        
        // Save progress
        SaveProgress();
        
        if (levelCompleteMenu) levelCompleteMenu.SetActive(true);
        OnLevelComplete?.Invoke();
    }
    
    int CalculateStars()
    {
        int stars = 0;
        
        // Star 1: Complete level
        stars++;
        
        // Star 2: Complete under time limit (e.g., 60 seconds)
        if (gameTime < 60f) stars++;
        
        // Star 3: Collect all coins or defeat all enemies
        // You can implement this based on your level requirements
        if (coinsCollected >= 10) stars++; // Example threshold
        
        return Mathf.Min(stars, 3);
    }
    
    public void NextLevel()
    {
        if (currentLevel < totalLevels)
        {
            currentLevel++;
            LoadLevel(currentLevel);
        }
        else
        {
            // Game completed
            LoadMainMenu();
        }
    }
    
    public void RestartLevel()
    {
        LoadLevel(currentLevel);
    }
    
    public void LoadLevel(int levelNumber)
    {
        currentLevel = levelNumber;
        SceneManager.LoadScene("Level" + levelNumber);
    }
    
    public void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
    
    public void AddScore(int points)
    {
        playerScore += points;
        OnScoreChanged?.Invoke(playerScore);
    }
    
    public void AddCoins(int amount)
    {
        coinsCollected += amount;
        OnCoinsChanged?.Invoke(coinsCollected);
        
        // Add score for coins
        AddScore(amount * 10);
    }
    
    public void AddLives(int amount)
    {
        playerLives += amount;
        OnLivesChanged?.Invoke(playerLives);
    }
    
    int CalculateLevelScore()
    {
        // Base score for completing level
        int baseScore = 1000;
        
        // Bonus for time
        int timeBonus = Mathf.Max(0, 500 - Mathf.RoundToInt(gameTime * 10));
        
        // Bonus for lives remaining
        int livesBonus = playerLives * 200;
        
        return baseScore + timeBonus + livesBonus;
    }
    
    void SaveProgress()
    {
        PlayerPrefs.SetInt("CurrentLevel", currentLevel);
        PlayerPrefs.SetInt("PlayerScore", playerScore);
        PlayerPrefs.SetInt("PlayerLives", playerLives);
        PlayerPrefs.SetInt("CoinsCollected", coinsCollected);
        PlayerPrefs.Save();
    }
    
    void LoadProgress()
    {
        currentLevel = PlayerPrefs.GetInt("CurrentLevel", 1);
        playerScore = PlayerPrefs.GetInt("PlayerScore", 0);
        playerLives = PlayerPrefs.GetInt("PlayerLives", 3);
        coinsCollected = PlayerPrefs.GetInt("CoinsCollected", 0);
    }
    
    public void ResetProgress()
    {
        PlayerPrefs.DeleteAll();
        currentLevel = 1;
        playerScore = 0;
        playerLives = 3;
        coinsCollected = 0;
        gameTime = 0f;
    }
    
    public void ToggleVibration()
    {
        enableVibration = !enableVibration;
        PlayerPrefs.SetInt("Vibration", enableVibration ? 1 : 0);
        PlayerPrefs.Save();
    }
    
    public void ToggleSound()
    {
        enableSound = !enableSound;
        AudioListener.volume = enableSound ? 1f : 0f;
        PlayerPrefs.SetInt("Sound", enableSound ? 1 : 0);
        PlayerPrefs.Save();
    }
    
    public void Vibrate()
    {
        if (enableVibration)
        {
            Handheld.Vibrate();
        }
    }
    
    // Public getters
    public int GetCurrentLevel() => currentLevel;
    public int GetTotalLevels() => totalLevels;
    public int GetPlayerLives() => playerLives;
    public int GetPlayerScore() => playerScore;
    public int GetCoinsCollected() => coinsCollected;
    public float GetGameTime() => gameTime;
    public bool IsPaused() => isPaused;
    public bool IsGameActive() => isGameActive;
    public bool IsLevelComplete() => isLevelComplete;
    public bool IsVibrationEnabled() => enableVibration;
    public bool IsSoundEnabled() => enableSound;
}