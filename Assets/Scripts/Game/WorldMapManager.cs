using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class WorldMapManager : MonoBehaviour
{
    [Header("World Map UI")]
    [SerializeField] private GameObject worldMapPanel;
    [SerializeField] private Transform worldContainer;
    [SerializeField] private GameObject worldButtonPrefab;
    [SerializeField] private GameObject levelButtonPrefab;
    
    [Header("World Display")]
    [SerializeField] private TextMeshProUGUI worldTitleText;
    [SerializeField] private TextMeshProUGUI worldProgressText;
    [SerializeField] private TextMeshProUGUI totalStarsText;
    [SerializeField] private Image worldBackgroundImage;
    [SerializeField] private Button backToWorldsButton;
    [SerializeField] private Button nextWorldButton;
    [SerializeField] private Button previousWorldButton;
    
    [Header("Level Display")]
    [SerializeField] private Transform levelContainer;
    [SerializeField] private TextMeshProUGUI levelTitleText;
    [SerializeField] private GameObject levelInfoPanel;
    [SerializeField] private TextMeshProUGUI levelStatsText;
    [SerializeField] private Button playLevelButton;
    [SerializeField] private Button levelBackButton;
    
    [Header("Navigation")]
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button shopButton;
    [SerializeField] private Button settingsButton;
    
    [Header("Visual Effects")]
    [SerializeField] private GameObject unlockEffect;
    [SerializeField] private AudioClip unlockSound;
    [SerializeField] private AudioClip selectSound;
    
    // Private variables
    private int currentWorld = 1;
    private int selectedLevel = 1;
    private List<GameObject> worldButtons = new List<GameObject>();
    private List<GameObject> levelButtons = new List<GameObject>();
    private AudioSource audioSource;
    
    void Start()
    {
        InitializeUI();
        SetupEventListeners();
        ShowWorldMap();
    }
    
    void InitializeUI()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Hide panels initially
        if (levelInfoPanel) levelInfoPanel.SetActive(false);
    }
    
    void SetupEventListeners()
    {
        if (backToWorldsButton) backToWorldsButton.onClick.AddListener(ShowWorldMap);
        if (nextWorldButton) nextWorldButton.onClick.AddListener(NextWorld);
        if (previousWorldButton) previousWorldButton.onClick.AddListener(PreviousWorld);
        if (playLevelButton) playLevelButton.onClick.AddListener(PlaySelectedLevel);
        if (levelBackButton) levelBackButton.onClick.AddListener(ShowLevelsInWorld);
        if (mainMenuButton) mainMenuButton.onClick.AddListener(GoToMainMenu);
        if (shopButton) shopButton.onClick.AddListener(GoToShop);
        if (settingsButton) settingsButton.onClick.AddListener(GoToSettings);
        
        // Subscribe to progress events
        if (LevelProgressManager.Instance != null)
        {
            LevelProgressManager.Instance.OnLevelUnlocked += OnLevelUnlocked;
            LevelProgressManager.Instance.OnLevelCompleted += OnLevelCompleted;
            LevelProgressManager.Instance.OnWorldUnlocked += OnWorldUnlocked;
        }
    }
    
    public void ShowWorldMap()
    {
        if (worldMapPanel) worldMapPanel.SetActive(true);
        if (levelInfoPanel) levelInfoPanel.SetActive(false);
        
        CreateWorldButtons();
        UpdateTotalStars();
    }
    
    void CreateWorldButtons()
    {
        // Clear existing buttons
        foreach (GameObject button in worldButtons)
        {
            Destroy(button);
        }
        worldButtons.Clear();
        
        if (LevelProgressManager.Instance == null) return;
        
        List<LevelProgressManager.WorldData> worlds = LevelProgressManager.Instance.GetAllWorlds();
        
        for (int i = 0; i < worlds.Count; i++)
        {
            LevelProgressManager.WorldData world = worlds[i];
            GameObject worldButton = Instantiate(worldButtonPrefab, worldContainer);
            
            // Setup world button
            SetupWorldButton(worldButton, world);
            
            worldButtons.Add(worldButton);
        }
    }
    
    void SetupWorldButton(GameObject worldButton, LevelProgressManager.WorldData world)
    {
        // Get button components
        Button button = worldButton.GetComponent<Button>();
        Image buttonImage = worldButton.GetComponent<Image>();
        TextMeshProUGUI worldText = worldButton.GetComponentInChildren<TextMeshProUGUI>();
        
        // Set world name
        if (worldText) worldText.text = world.worldName;
        
        // Set world color
        if (buttonImage && world.worldColor != Color.clear)
        {
            buttonImage.color = world.worldColor;
        }
        
        // Set button interactability based on unlock status
        if (button)
        {
            button.interactable = world.isUnlocked;
            button.onClick.AddListener(() => ShowLevelsInWorld(world.worldNumber));
        }
        
        // Show lock icon if not unlocked
        Transform lockIcon = worldButton.transform.Find("LockIcon");
        if (lockIcon)
        {
            lockIcon.gameObject.SetActive(!world.isUnlocked);
        }
        
        // Show progress
        Transform progressText = worldButton.transform.Find("ProgressText");
        if (progressText)
        {
            TextMeshProUGUI progress = progressText.GetComponent<TextMeshProUGUI>();
            if (progress)
            {
                progress.text = $"{world.completedLevels}/{world.totalLevels}";
            }
        }
        
        // Show stars
        Transform starsContainer = worldButton.transform.Find("StarsContainer");
        if (starsContainer)
        {
            ShowStarsInContainer(starsContainer, world.totalStars);
        }
    }
    
    public void ShowLevelsInWorld(int worldNumber = -1)
    {
        if (worldNumber > 0) currentWorld = worldNumber;
        
        if (worldMapPanel) worldMapPanel.SetActive(false);
        if (levelInfoPanel) levelInfoPanel.SetActive(false);
        
        CreateLevelButtons();
        UpdateWorldDisplay();
    }
    
    void CreateLevelButtons()
    {
        // Clear existing buttons
        foreach (GameObject button in levelButtons)
        {
            Destroy(button);
        }
        levelButtons.Clear();
        
        if (LevelProgressManager.Instance == null) return;
        
        List<LevelProgressManager.LevelData> levels = LevelProgressManager.Instance.GetLevelsInWorld(currentWorld);
        
        for (int i = 0; i < levels.Count; i++)
        {
            LevelProgressManager.LevelData level = levels[i];
            GameObject levelButton = Instantiate(levelButtonPrefab, levelContainer);
            
            // Setup level button
            SetupLevelButton(levelButton, level);
            
            levelButtons.Add(levelButton);
        }
    }
    
    void SetupLevelButton(GameObject levelButton, LevelProgressManager.LevelData level)
    {
        // Get button components
        Button button = levelButton.GetComponent<Button>();
        TextMeshProUGUI levelText = levelButton.GetComponentInChildren<TextMeshProUGUI>();
        
        // Set level number
        if (levelText) levelText.text = level.levelNumber.ToString();
        
        // Set button interactability
        if (button)
        {
            button.interactable = level.isUnlocked;
            button.onClick.AddListener(() => ShowLevelInfo(level.levelNumber));
        }
        
        // Show lock icon if not unlocked
        Transform lockIcon = levelButton.transform.Find("LockIcon");
        if (lockIcon)
        {
            lockIcon.gameObject.SetActive(!level.isUnlocked);
        }
        
        // Show completion status
        Transform completionIcon = levelButton.transform.Find("CompletionIcon");
        if (completionIcon)
        {
            completionIcon.gameObject.SetActive(level.isCompleted);
        }
        
        // Show stars
        Transform starsContainer = levelButton.transform.Find("StarsContainer");
        if (starsContainer)
        {
            ShowStarsInContainer(starsContainer, level.starsEarned);
        }
        
        // Show best time
        Transform timeText = levelButton.transform.Find("TimeText");
        if (timeText && level.bestTime > 0)
        {
            TextMeshProUGUI time = timeText.GetComponent<TextMeshProUGUI>();
            if (time)
            {
                int minutes = Mathf.FloorToInt(level.bestTime / 60f);
                int seconds = Mathf.FloorToInt(level.bestTime % 60f);
                time.text = string.Format("{0:00}:{1:00}", minutes, seconds);
            }
        }
    }
    
    void ShowStarsInContainer(Transform container, int stars)
    {
        for (int i = 0; i < container.childCount; i++)
        {
            Transform star = container.GetChild(i);
            if (star)
            {
                star.gameObject.SetActive(i < stars);
            }
        }
    }
    
    public void ShowLevelInfo(int levelNumber)
    {
        selectedLevel = levelNumber;
        
        if (levelInfoPanel) levelInfoPanel.SetActive(true);
        
        UpdateLevelInfo();
    }
    
    void UpdateLevelInfo()
    {
        if (LevelProgressManager.Instance == null) return;
        
        LevelProgressManager.LevelData levelData = LevelProgressManager.Instance.GetLevelData(selectedLevel);
        if (levelData == null) return;
        
        // Update level title
        if (levelTitleText)
        {
            levelTitleText.text = $"Level {selectedLevel}";
        }
        
        // Update level stats
        if (levelStatsText)
        {
            string stats = $"Stars: {levelData.starsEarned}/3\n";
            
            if (levelData.bestTime > 0)
            {
                int minutes = Mathf.FloorToInt(levelData.bestTime / 60f);
                int seconds = Mathf.FloorToInt(levelData.bestTime % 60f);
                stats += $"Best Time: {minutes:00}:{seconds:00}\n";
            }
            
            if (levelData.bestScore > 0)
            {
                stats += $"Best Score: {levelData.bestScore}\n";
            }
            
            stats += $"Coins: {levelData.coinsCollected}";
            
            levelStatsText.text = stats;
        }
        
        // Update play button
        if (playLevelButton)
        {
            playLevelButton.interactable = levelData.isUnlocked;
        }
        
        // Play select sound
        if (audioSource && selectSound)
        {
            audioSource.PlayOneShot(selectSound);
        }
    }
    
    void UpdateWorldDisplay()
    {
        if (LevelProgressManager.Instance == null) return;
        
        LevelProgressManager.WorldData worldData = LevelProgressManager.Instance.GetWorldData(currentWorld);
        if (worldData == null) return;
        
        // Update world title
        if (worldTitleText)
        {
            worldTitleText.text = worldData.worldName;
        }
        
        // Update world progress
        if (worldProgressText)
        {
            worldProgressText.text = $"Progress: {worldData.completedLevels}/{worldData.totalLevels}";
        }
        
        // Update navigation buttons
        if (previousWorldButton)
        {
            previousWorldButton.gameObject.SetActive(currentWorld > 1);
        }
        
        if (nextWorldButton)
        {
            bool nextWorldExists = LevelProgressManager.Instance.GetWorldData(currentWorld + 1) != null;
            bool nextWorldUnlocked = LevelProgressManager.Instance.IsWorldUnlocked(currentWorld + 1);
            nextWorldButton.gameObject.SetActive(nextWorldExists && nextWorldUnlocked);
        }
        
        // Update world background
        if (worldBackgroundImage && worldData.worldColor != Color.clear)
        {
            worldBackgroundImage.color = worldData.worldColor;
        }
    }
    
    void UpdateTotalStars()
    {
        if (LevelProgressManager.Instance == null) return;
        
        if (totalStarsText)
        {
            totalStarsText.text = $"Total Stars: {LevelProgressManager.Instance.GetTotalStars()}";
        }
    }
    
    void NextWorld()
    {
        if (LevelProgressManager.Instance == null) return;
        
        LevelProgressManager.WorldData nextWorld = LevelProgressManager.Instance.GetWorldData(currentWorld + 1);
        if (nextWorld != null && nextWorld.isUnlocked)
        {
            currentWorld++;
            ShowLevelsInWorld();
        }
    }
    
    void PreviousWorld()
    {
        if (currentWorld > 1)
        {
            currentWorld--;
            ShowLevelsInWorld();
        }
    }
    
    void PlaySelectedLevel()
    {
        if (LevelProgressManager.Instance == null) return;
        
        if (LevelProgressManager.Instance.IsLevelUnlocked(selectedLevel))
        {
            // Load the level scene
            if (GameManager.Instance)
            {
                GameManager.Instance.LoadLevel(selectedLevel);
            }
        }
    }
    
    void GoToMainMenu()
    {
        if (GameManager.Instance)
        {
            GameManager.Instance.LoadMainMenu();
        }
    }
    
    void GoToShop()
    {
        // Load shop scene or show shop UI
        Debug.Log("Opening Shop...");
    }
    
    void GoToSettings()
    {
        // Show settings menu
        Debug.Log("Opening Settings...");
    }
    
    // Event handlers
    void OnLevelUnlocked(int levelNumber)
    {
        // Show unlock effect
        if (unlockEffect)
        {
            // Find the level button and show effect
            foreach (GameObject levelButton in levelButtons)
            {
                // You might need to store level numbers in buttons for this
                // For now, just show a general unlock effect
            }
        }
        
        // Play unlock sound
        if (audioSource && unlockSound)
        {
            audioSource.PlayOneShot(unlockSound);
        }
        
        // Refresh UI
        if (levelInfoPanel && levelInfoPanel.activeInHierarchy)
        {
            ShowLevelsInWorld();
        }
    }
    
    void OnLevelCompleted(int levelNumber)
    {
        // Refresh UI to show new progress
        if (levelInfoPanel && levelInfoPanel.activeInHierarchy)
        {
            ShowLevelsInWorld();
        }
        
        UpdateTotalStars();
    }
    
    void OnWorldUnlocked(int worldNumber)
    {
        // Show world unlock effect
        if (unlockEffect)
        {
            // Show effect on world button
        }
        
        // Play unlock sound
        if (audioSource && unlockSound)
        {
            audioSource.PlayOneShot(unlockSound);
        }
        
        // Refresh world map
        if (worldMapPanel && worldMapPanel.activeInHierarchy)
        {
            ShowWorldMap();
        }
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events
        if (LevelProgressManager.Instance != null)
        {
            LevelProgressManager.Instance.OnLevelUnlocked -= OnLevelUnlocked;
            LevelProgressManager.Instance.OnLevelCompleted -= OnLevelCompleted;
            LevelProgressManager.Instance.OnWorldUnlocked -= OnWorldUnlocked;
        }
    }
}