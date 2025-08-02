using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("HUD Elements")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI livesText;
    [SerializeField] private TextMeshProUGUI coinsText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI timeText;
    
    [Header("Mobile Controls")]
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button jumpButton;
    [SerializeField] private Button leftButton;
    [SerializeField] private Button rightButton;
    
    [Header("Menus")]
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject gameOverMenu;
    [SerializeField] private GameObject levelCompleteMenu;
    [SerializeField] private GameObject settingsMenu;
    
    [Header("Pause Menu")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button settingsButton;
    
    [Header("Game Over Menu")]
    [SerializeField] private TextMeshProUGUI finalScoreText;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button mainMenuGameOverButton;
    
    [Header("Level Complete Menu")]
    [SerializeField] private TextMeshProUGUI levelScoreText;
    [SerializeField] private TextMeshProUGUI levelTimeText;
    [SerializeField] private Button nextLevelButton;
    [SerializeField] private Button mainMenuCompleteButton;
    
    [Header("Settings Menu")]
    [SerializeField] private Toggle soundToggle;
    [SerializeField] private Toggle vibrationToggle;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    
    [Header("Mobile UI Settings")]
    [SerializeField] private bool showMobileControls = true;
    [SerializeField] private float buttonScale = 1.2f;
    [SerializeField] private float buttonPressDuration = 0.1f;
    
    // Private variables
    private PlayerController playerController;
    private bool isMobileControlsActive = false;
    
    void Start()
    {
        InitializeUI();
        SetupEventListeners();
        UpdateUI();
        
        // Hide all menus initially
        HideAllMenus();
    }
    
    void InitializeUI()
    {
        // Find player controller
        playerController = FindObjectOfType<PlayerController>();
        
        // Setup mobile controls
        if (showMobileControls && Application.isMobilePlatform)
        {
            SetupMobileControls();
        }
        else
        {
            HideMobileControls();
        }
        
        // Load settings
        LoadSettings();
    }
    
    void SetupEventListeners()
    {
        // Subscribe to game manager events
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnScoreChanged += UpdateScoreText;
            GameManager.Instance.OnLivesChanged += UpdateLivesText;
            GameManager.Instance.OnCoinsChanged += UpdateCoinsText;
            GameManager.Instance.OnGamePaused += ShowPauseMenu;
            GameManager.Instance.OnGameResumed += HidePauseMenu;
            GameManager.Instance.OnGameOver += ShowGameOverMenu;
            GameManager.Instance.OnLevelComplete += ShowLevelCompleteMenu;
        }
        
        // Setup button listeners
        if (pauseButton) pauseButton.onClick.AddListener(OnPauseButtonClicked);
        if (jumpButton) jumpButton.onClick.AddListener(OnJumpButtonClicked);
        if (leftButton) leftButton.onClick.AddListener(OnLeftButtonClicked);
        if (rightButton) rightButton.onClick.AddListener(OnRightButtonClicked);
        
        // Pause menu buttons
        if (resumeButton) resumeButton.onClick.AddListener(OnResumeButtonClicked);
        if (restartButton) restartButton.onClick.AddListener(OnRestartButtonClicked);
        if (mainMenuButton) mainMenuButton.onClick.AddListener(OnMainMenuButtonClicked);
        if (settingsButton) settingsButton.onClick.AddListener(OnSettingsButtonClicked);
        
        // Game over menu buttons
        if (retryButton) retryButton.onClick.AddListener(OnRetryButtonClicked);
        if (mainMenuGameOverButton) mainMenuGameOverButton.onClick.AddListener(OnMainMenuButtonClicked);
        
        // Level complete menu buttons
        if (nextLevelButton) nextLevelButton.onClick.AddListener(OnNextLevelButtonClicked);
        if (mainMenuCompleteButton) mainMenuCompleteButton.onClick.AddListener(OnMainMenuButtonClicked);
        
        // Settings menu
        if (soundToggle) soundToggle.onValueChanged.AddListener(OnSoundToggleChanged);
        if (vibrationToggle) vibrationToggle.onValueChanged.AddListener(OnVibrationToggleChanged);
        if (musicSlider) musicSlider.onValueChanged.AddListener(OnMusicSliderChanged);
        if (sfxSlider) sfxSlider.onValueChanged.AddListener(OnSFXSliderChanged);
    }
    
    void SetupMobileControls()
    {
        isMobileControlsActive = true;
        
        // Show mobile control buttons
        if (pauseButton) pauseButton.gameObject.SetActive(true);
        if (jumpButton) jumpButton.gameObject.SetActive(true);
        if (leftButton) leftButton.gameObject.SetActive(true);
        if (rightButton) rightButton.gameObject.SetActive(true);
        
        // Setup button animations
        SetupButtonAnimations();
    }
    
    void HideMobileControls()
    {
        isMobileControlsActive = false;
        
        if (pauseButton) pauseButton.gameObject.SetActive(false);
        if (jumpButton) jumpButton.gameObject.SetActive(false);
        if (leftButton) leftButton.gameObject.SetActive(false);
        if (rightButton) rightButton.gameObject.SetActive(false);
    }
    
    void SetupButtonAnimations()
    {
        // Add button press animations
        AddButtonAnimation(pauseButton);
        AddButtonAnimation(jumpButton);
        AddButtonAnimation(leftButton);
        AddButtonAnimation(rightButton);
    }
    
    void AddButtonAnimation(Button button)
    {
        if (button == null) return;
        
        button.onClick.AddListener(() => {
            StartCoroutine(AnimateButtonPress(button));
        });
    }
    
    System.Collections.IEnumerator AnimateButtonPress(Button button)
    {
        if (button == null) yield break;
        
        Vector3 originalScale = button.transform.localScale;
        Vector3 pressedScale = originalScale * buttonScale;
        
        // Scale down
        button.transform.localScale = pressedScale;
        
        yield return new WaitForSeconds(buttonPressDuration);
        
        // Scale back
        button.transform.localScale = originalScale;
    }
    
    void UpdateUI()
    {
        if (GameManager.Instance != null)
        {
            UpdateScoreText(GameManager.Instance.GetPlayerScore());
            UpdateLivesText(GameManager.Instance.GetPlayerLives());
            UpdateCoinsText(GameManager.Instance.GetCoinsCollected());
            UpdateLevelText(GameManager.Instance.GetCurrentLevel());
            UpdateTimeText(GameManager.Instance.GetGameTime());
        }
    }
    
    void UpdateScoreText(int score)
    {
        if (scoreText)
        {
            scoreText.text = "Score: " + score.ToString("N0");
        }
    }
    
    void UpdateLivesText(int lives)
    {
        if (livesText)
        {
            livesText.text = "Lives: " + lives;
        }
    }
    
    void UpdateCoinsText(int coins)
    {
        if (coinsText)
        {
            coinsText.text = "Coins: " + coins;
        }
    }
    
    void UpdateLevelText(int level)
    {
        if (levelText)
        {
            levelText.text = "Level " + level;
        }
    }
    
    void UpdateTimeText(float time)
    {
        if (timeText)
        {
            int minutes = Mathf.FloorToInt(time / 60f);
            int seconds = Mathf.FloorToInt(time % 60f);
            timeText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }
    
    void ShowPauseMenu()
    {
        if (pauseMenu) pauseMenu.SetActive(true);
    }
    
    void HidePauseMenu()
    {
        if (pauseMenu) pauseMenu.SetActive(false);
    }
    
    void ShowGameOverMenu()
    {
        if (gameOverMenu)
        {
            gameOverMenu.SetActive(true);
            if (finalScoreText)
            {
                finalScoreText.text = "Final Score: " + GameManager.Instance.GetPlayerScore().ToString("N0");
            }
        }
    }
    
    void ShowLevelCompleteMenu()
    {
        if (levelCompleteMenu)
        {
            levelCompleteMenu.SetActive(true);
            if (levelScoreText)
            {
                levelScoreText.text = "Level Score: " + GameManager.Instance.GetPlayerScore().ToString("N0");
            }
            if (levelTimeText)
            {
                float time = GameManager.Instance.GetGameTime();
                int minutes = Mathf.FloorToInt(time / 60f);
                int seconds = Mathf.FloorToInt(time % 60f);
                levelTimeText.text = "Time: " + string.Format("{0:00}:{1:00}", minutes, seconds);
            }
        }
    }
    
    void HideAllMenus()
    {
        if (pauseMenu) pauseMenu.SetActive(false);
        if (gameOverMenu) gameOverMenu.SetActive(false);
        if (levelCompleteMenu) levelCompleteMenu.SetActive(false);
        if (settingsMenu) settingsMenu.SetActive(false);
    }
    
    // Button event handlers
    void OnPauseButtonClicked()
    {
        if (GameManager.Instance)
        {
            GameManager.Instance.TogglePause();
        }
    }
    
    void OnJumpButtonClicked()
    {
        if (playerController && playerController.IsGrounded())
        {
            // Trigger jump through player controller
            // This would need to be implemented in PlayerController
        }
    }
    
    void OnLeftButtonClicked()
    {
        // Handle left movement
    }
    
    void OnRightButtonClicked()
    {
        // Handle right movement
    }
    
    void OnResumeButtonClicked()
    {
        if (GameManager.Instance)
        {
            GameManager.Instance.ResumeGame();
        }
    }
    
    void OnRestartButtonClicked()
    {
        if (GameManager.Instance)
        {
            GameManager.Instance.RestartLevel();
        }
    }
    
    void OnMainMenuButtonClicked()
    {
        if (GameManager.Instance)
        {
            GameManager.Instance.LoadMainMenu();
        }
    }
    
    void OnSettingsButtonClicked()
    {
        if (settingsMenu)
        {
            settingsMenu.SetActive(true);
        }
    }
    
    void OnRetryButtonClicked()
    {
        if (GameManager.Instance)
        {
            GameManager.Instance.RestartLevel();
        }
    }
    
    void OnNextLevelButtonClicked()
    {
        if (GameManager.Instance)
        {
            GameManager.Instance.NextLevel();
        }
    }
    
    // Settings handlers
    void OnSoundToggleChanged(bool value)
    {
        if (GameManager.Instance)
        {
            GameManager.Instance.ToggleSound();
        }
    }
    
    void OnVibrationToggleChanged(bool value)
    {
        if (GameManager.Instance)
        {
            GameManager.Instance.ToggleVibration();
        }
    }
    
    void OnMusicSliderChanged(float value)
    {
        // Handle music volume change
        PlayerPrefs.SetFloat("MusicVolume", value);
        PlayerPrefs.Save();
    }
    
    void OnSFXSliderChanged(float value)
    {
        // Handle SFX volume change
        PlayerPrefs.SetFloat("SFXVolume", value);
        PlayerPrefs.Save();
    }
    
    void LoadSettings()
    {
        if (soundToggle)
        {
            soundToggle.isOn = PlayerPrefs.GetInt("Sound", 1) == 1;
        }
        if (vibrationToggle)
        {
            vibrationToggle.isOn = PlayerPrefs.GetInt("Vibration", 1) == 1;
        }
        if (musicSlider)
        {
            musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);
        }
        if (sfxSlider)
        {
            sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);
        }
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnScoreChanged -= UpdateScoreText;
            GameManager.Instance.OnLivesChanged -= UpdateLivesText;
            GameManager.Instance.OnCoinsChanged -= UpdateCoinsText;
            GameManager.Instance.OnGamePaused -= ShowPauseMenu;
            GameManager.Instance.OnGameResumed -= HidePauseMenu;
            GameManager.Instance.OnGameOver -= ShowGameOverMenu;
            GameManager.Instance.OnLevelComplete -= ShowLevelCompleteMenu;
        }
    }
}