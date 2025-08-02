using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour
{
    [Header("Level Settings")]
    [SerializeField] private int levelNumber = 1;
    [SerializeField] private string levelName = "Level 1";
    [SerializeField] private float timeLimit = 300f; // 5 minutes
    [SerializeField] private int targetScore = 1000;
    [SerializeField] private int targetCoins = 10;
    
    [Header("Level Completion")]
    [SerializeField] private Transform levelEndPoint;
    [SerializeField] private GameObject levelCompleteTrigger;
    [SerializeField] private bool requireAllCoins = false;
    [SerializeField] private bool requireAllEnemies = false;
    
    [Header("Checkpoints")]
    [SerializeField] private List<Transform> checkpoints;
    [SerializeField] private int currentCheckpoint = 0;
    [SerializeField] private bool useCheckpoints = true;
    
    [Header("Level Objects")]
    [SerializeField] private List<GameObject> coins;
    [SerializeField] private List<GameObject> enemies;
    [SerializeField] private List<GameObject> powerUps;
    
    [Header("Level Events")]
    [SerializeField] private GameObject[] levelEvents;
    [SerializeField] private float[] eventTimers;
    
    // Components
    private PlayerController player;
    private Transform playerTransform;
    private Vector3 playerStartPosition;
    
    // Level state
    private bool isLevelComplete = false;
    private bool isLevelFailed = false;
    private float levelTimer = 0f;
    private int coinsCollected = 0;
    private int enemiesDefeated = 0;
    private int totalCoins = 0;
    private int totalEnemies = 0;
    
    // Events
    public System.Action OnLevelComplete;
    public System.Action OnLevelFailed;
    public System.Action<float> OnTimeChanged;
    public System.Action<int> OnCoinsChanged;
    public System.Action<int> OnEnemiesChanged;
    
    void Start()
    {
        InitializeLevel();
        SetupEventListeners();
    }
    
    void Update()
    {
        if (!isLevelComplete && !isLevelFailed)
        {
            UpdateLevelTimer();
            CheckLevelConditions();
        }
    }
    
    void InitializeLevel()
    {
        // Find player
        player = FindObjectOfType<PlayerController>();
        if (player)
        {
            playerTransform = player.transform;
            playerStartPosition = playerTransform.position;
        }
        
        // Count level objects
        CountLevelObjects();
        
        // Setup checkpoints
        if (useCheckpoints && checkpoints.Count > 0)
        {
            currentCheckpoint = 0;
        }
        
        // Reset level state
        isLevelComplete = false;
        isLevelFailed = false;
        levelTimer = 0f;
        coinsCollected = 0;
        enemiesDefeated = 0;
        
        // Trigger events
        OnCoinsChanged?.Invoke(coinsCollected);
        OnEnemiesChanged?.Invoke(enemiesDefeated);
    }
    
    void SetupEventListeners()
    {
        // Subscribe to game manager events
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnCoinsChanged += OnCoinsCollected;
        }
        
        // Subscribe to player events
        if (player)
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth)
            {
                playerHealth.OnPlayerDied += OnPlayerDied;
            }
        }
    }
    
    void CountLevelObjects()
    {
        // Count coins
        coins.Clear();
        GameObject[] coinObjects = GameObject.FindGameObjectsWithTag("Coin");
        coins.AddRange(coinObjects);
        totalCoins = coins.Count;
        
        // Count enemies
        enemies.Clear();
        GameObject[] enemyObjects = GameObject.FindGameObjectsWithTag("Enemy");
        enemies.AddRange(enemyObjects);
        totalEnemies = enemies.Count;
        
        // Count power-ups
        powerUps.Clear();
        GameObject[] powerUpObjects = GameObject.FindGameObjectsWithTag("PowerUp");
        powerUps.AddRange(powerUpObjects);
    }
    
    void UpdateLevelTimer()
    {
        levelTimer += Time.deltaTime;
        OnTimeChanged?.Invoke(levelTimer);
        
        // Check time limit
        if (timeLimit > 0 && levelTimer >= timeLimit)
        {
            LevelFailed("Time's up!");
        }
    }
    
    void CheckLevelConditions()
    {
        // Check if player reached level end
        if (levelEndPoint && playerTransform)
        {
            float distanceToEnd = Vector2.Distance(playerTransform.position, levelEndPoint.position);
            if (distanceToEnd < 1f)
            {
                CheckLevelCompletion();
            }
        }
        
        // Check if all required conditions are met
        if (requireAllCoins && coinsCollected >= totalCoins)
        {
            CheckLevelCompletion();
        }
        
        if (requireAllEnemies && enemiesDefeated >= totalEnemies)
        {
            CheckLevelCompletion();
        }
    }
    
    void CheckLevelCompletion()
    {
        if (isLevelComplete) return;
        
        // Check if all conditions are met
        bool canComplete = true;
        
        if (requireAllCoins && coinsCollected < totalCoins)
        {
            canComplete = false;
        }
        
        if (requireAllEnemies && enemiesDefeated < totalEnemies)
        {
            canComplete = false;
        }
        
        if (canComplete)
        {
            CompleteLevel();
        }
    }
    
    void CompleteLevel()
    {
        isLevelComplete = true;
        
        // Calculate level score
        int levelScore = CalculateLevelScore();
        
        // Add bonus score
        if (GameManager.Instance)
        {
            GameManager.Instance.AddScore(levelScore);
        }
        
        // Trigger completion event
        OnLevelComplete?.Invoke();
        
        // Notify game manager
        if (GameManager.Instance)
        {
            GameManager.Instance.LevelComplete();
        }
        
        // Show completion UI
        StartCoroutine(ShowLevelCompleteUI());
    }
    
    void LevelFailed(string reason)
    {
        isLevelFailed = true;
        
        // Trigger failure event
        OnLevelFailed?.Invoke();
        
        // Show failure UI
        StartCoroutine(ShowLevelFailedUI(reason));
    }
    
    void OnPlayerDied()
    {
        if (useCheckpoints && checkpoints.Count > 0)
        {
            RespawnAtCheckpoint();
        }
        else
        {
            LevelFailed("Player died!");
        }
    }
    
    void OnCoinsCollected(int totalCoinsCollected)
    {
        coinsCollected = totalCoinsCollected;
        OnCoinsChanged?.Invoke(coinsCollected);
        
        // Check if all coins collected
        if (requireAllCoins && coinsCollected >= totalCoins)
        {
            CheckLevelCompletion();
        }
    }
    
    public void EnemyDefeated()
    {
        enemiesDefeated++;
        OnEnemiesChanged?.Invoke(enemiesDefeated);
        
        // Check if all enemies defeated
        if (requireAllEnemies && enemiesDefeated >= totalEnemies)
        {
            CheckLevelCompletion();
        }
    }
    
    void RespawnAtCheckpoint()
    {
        if (checkpoints.Count == 0 || currentCheckpoint >= checkpoints.Count) return;
        
        // Respawn player at checkpoint
        if (playerTransform)
        {
            playerTransform.position = checkpoints[currentCheckpoint].position;
        }
        
        // Respawn player health
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth)
        {
            playerHealth.Respawn();
        }
    }
    
    public void SetCheckpoint(int checkpointIndex)
    {
        if (checkpointIndex >= 0 && checkpointIndex < checkpoints.Count)
        {
            currentCheckpoint = checkpointIndex;
        }
    }
    
    public void AddCheckpoint(Transform checkpoint)
    {
        if (checkpoint != null && !checkpoints.Contains(checkpoint))
        {
            checkpoints.Add(checkpoint);
        }
    }
    
    public void RemoveCheckpoint(Transform checkpoint)
    {
        if (checkpoints.Contains(checkpoint))
        {
            checkpoints.Remove(checkpoint);
        }
    }
    
    int CalculateLevelScore()
    {
        int baseScore = 1000;
        
        // Time bonus (faster = more points)
        int timeBonus = Mathf.Max(0, 500 - Mathf.RoundToInt(levelTimer * 2));
        
        // Coin bonus
        int coinBonus = coinsCollected * 50;
        
        // Enemy bonus
        int enemyBonus = enemiesDefeated * 100;
        
        // Lives bonus
        if (GameManager.Instance)
        {
            int livesBonus = GameManager.Instance.GetPlayerLives() * 200;
            return baseScore + timeBonus + coinBonus + enemyBonus + livesBonus;
        }
        
        return baseScore + timeBonus + coinBonus + enemyBonus;
    }
    
    IEnumerator ShowLevelCompleteUI()
    {
        // Wait a moment before showing UI
        yield return new WaitForSeconds(1f);
        
        // Show level complete UI
        // This would be handled by UIManager
    }
    
    IEnumerator ShowLevelFailedUI(string reason)
    {
        // Wait a moment before showing UI
        yield return new WaitForSeconds(1f);
        
        // Show level failed UI
        // This would be handled by UIManager
    }
    
    public void RestartLevel()
    {
        // Reset player position
        if (playerTransform)
        {
            playerTransform.position = playerStartPosition;
        }
        
        // Reset level state
        isLevelComplete = false;
        isLevelFailed = false;
        levelTimer = 0f;
        coinsCollected = 0;
        enemiesDefeated = 0;
        currentCheckpoint = 0;
        
        // Respawn player
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth)
        {
            playerHealth.Respawn();
        }
        
        // Reset level objects
        ResetLevelObjects();
        
        // Trigger events
        OnCoinsChanged?.Invoke(coinsCollected);
        OnEnemiesChanged?.Invoke(enemiesDefeated);
        OnTimeChanged?.Invoke(levelTimer);
    }
    
    void ResetLevelObjects()
    {
        // Reset coins
        foreach (GameObject coin in coins)
        {
            if (coin != null)
            {
                coin.SetActive(true);
                Coin coinComponent = coin.GetComponent<Coin>();
                if (coinComponent)
                {
                    // Reset coin state
                }
            }
        }
        
        // Reset enemies
        foreach (GameObject enemy in enemies)
        {
            if (enemy != null)
            {
                enemy.SetActive(true);
                EnemyController enemyComponent = enemy.GetComponent<EnemyController>();
                if (enemyComponent)
                {
                    // Reset enemy state
                }
            }
        }
        
        // Reset power-ups
        foreach (GameObject powerUp in powerUps)
        {
            if (powerUp != null)
            {
                powerUp.SetActive(true);
                PowerUp powerUpComponent = powerUp.GetComponent<PowerUp>();
                if (powerUpComponent)
                {
                    // Reset power-up state
                }
            }
        }
    }
    
    // Public getters
    public int GetLevelNumber() => levelNumber;
    public string GetLevelName() => levelName;
    public float GetLevelTimer() => levelTimer;
    public float GetTimeLimit() => timeLimit;
    public int GetCoinsCollected() => coinsCollected;
    public int GetTotalCoins() => totalCoins;
    public int GetEnemiesDefeated() => enemiesDefeated;
    public int GetTotalEnemies() => totalEnemies;
    public bool IsLevelComplete() => isLevelComplete;
    public bool IsLevelFailed() => isLevelFailed;
    public int GetCurrentCheckpoint() => currentCheckpoint;
    public int GetTotalCheckpoints() => checkpoints.Count;
    
    void OnDestroy()
    {
        // Unsubscribe from events
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnCoinsChanged -= OnCoinsCollected;
        }
        
        if (player)
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth)
            {
                playerHealth.OnPlayerDied -= OnPlayerDied;
            }
        }
    }
    
    void OnDrawGizmosSelected()
    {
        // Draw level end point
        if (levelEndPoint)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(levelEndPoint.position, 1f);
        }
        
        // Draw checkpoints
        Gizmos.color = Color.yellow;
        for (int i = 0; i < checkpoints.Count; i++)
        {
            if (checkpoints[i] != null)
            {
                Gizmos.DrawWireCube(checkpoints[i].position, Vector3.one);
                
                // Draw checkpoint number
                #if UNITY_EDITOR
                UnityEditor.Handles.Label(checkpoints[i].position + Vector3.up, i.ToString());
                #endif
            }
        }
    }
}