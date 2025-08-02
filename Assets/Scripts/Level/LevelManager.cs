using UnityEngine;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour
{
    [Header("Level Settings")]
    public int levelNumber = 1;
    public string levelName = "Level 1";
    public int targetScore = 100;
    public float timeLimit = 300f; // 5 minutes
    
    [Header("Spawn Points")]
    public Transform playerSpawnPoint;
    public Transform[] enemySpawnPoints;
    public Transform[] coinSpawnPoints;
    
    [Header("Prefabs")]
    public GameObject playerPrefab;
    public GameObject[] enemyPrefabs;
    public GameObject coinPrefab;
    public GameObject checkpointPrefab;
    
    [Header("Level Objects")]
    public GameObject[] levelObjects;
    public Checkpoint[] checkpoints;
    
    [Header("UI")]
    public Text levelNameText;
    public Text timeText;
    public Text targetScoreText;
    
    private float currentTime;
    private bool levelCompleted = false;
    private bool levelFailed = false;
    private GameObject currentPlayer;
    private List<GameObject> spawnedEnemies = new List<GameObject>();
    private List<GameObject> spawnedCoins = new List<GameObject>();
    
    void Start()
    {
        InitializeLevel();
        SpawnLevelObjects();
        UpdateUI();
        
        // Start background music
        AudioManager.Instance?.PlayGameplayMusic();
    }
    
    void Update()
    {
        if (!levelCompleted && !levelFailed)
        {
            UpdateTimer();
            CheckLevelCompletion();
        }
    }
    
    void InitializeLevel()
    {
        currentTime = timeLimit;
        levelCompleted = false;
        levelFailed = false;
        
        // Update UI
        if (levelNameText != null)
            levelNameText.text = levelName;
        
        if (targetScoreText != null)
            targetScoreText.text = "Target: " + targetScore.ToString();
    }
    
    void SpawnLevelObjects()
    {
        // Spawn player
        if (playerPrefab != null && playerSpawnPoint != null)
        {
            currentPlayer = Instantiate(playerPrefab, playerSpawnPoint.position, playerSpawnPoint.rotation);
            
            // Setup camera to follow player
            CameraFollow cameraFollow = FindObjectOfType<CameraFollow>();
            if (cameraFollow != null)
            {
                cameraFollow.SetTarget(currentPlayer.transform);
            }
        }
        
        // Spawn enemies
        if (enemyPrefabs != null && enemyPrefabs.Length > 0 && enemySpawnPoints != null)
        {
            for (int i = 0; i < enemySpawnPoints.Length && i < enemyPrefabs.Length; i++)
            {
                GameObject enemy = Instantiate(enemyPrefabs[i], enemySpawnPoints[i].position, enemySpawnPoints[i].rotation);
                spawnedEnemies.Add(enemy);
            }
        }
        
        // Spawn coins
        if (coinPrefab != null && coinSpawnPoints != null)
        {
            foreach (Transform spawnPoint in coinSpawnPoints)
            {
                GameObject coin = Instantiate(coinPrefab, spawnPoint.position, spawnPoint.rotation);
                spawnedCoins.Add(coin);
            }
        }
    }
    
    void UpdateTimer()
    {
        currentTime -= Time.deltaTime;
        
        if (currentTime <= 0)
        {
            currentTime = 0;
            LevelFailed("Time's up!");
        }
        
        UpdateTimeUI();
    }
    
    void UpdateTimeUI()
    {
        if (timeText != null)
        {
            int minutes = Mathf.FloorToInt(currentTime / 60);
            int seconds = Mathf.FloorToInt(currentTime % 60);
            timeText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }
    
    void CheckLevelCompletion()
    {
        // Check if target score is reached
        if (GameManager.Instance != null && GameManager.Instance.CurrentScore >= targetScore)
        {
            LevelCompleted();
        }
        
        // Check if all coins are collected
        if (spawnedCoins.Count == 0)
        {
            LevelCompleted();
        }
    }
    
    void LevelCompleted()
    {
        if (levelCompleted) return;
        
        levelCompleted = true;
        Debug.Log("Level " + levelNumber + " completed!");
        
        // Play completion sound
        AudioManager.Instance?.PlayLevelCompleteSound();
        
        // Show completion UI
        GameManager.Instance?.CompleteLevel();
    }
    
    void LevelFailed(string reason)
    {
        if (levelFailed) return;
        
        levelFailed = true;
        Debug.Log("Level " + levelNumber + " failed: " + reason);
        
        // Play failure sound
        AudioManager.Instance?.PlayGameOverSound();
        
        // Show failure UI
        GameManager.Instance?.PlayerDied();
    }
    
    void UpdateUI()
    {
        UpdateTimeUI();
    }
    
    public void RestartLevel()
    {
        // Clear spawned objects
        if (currentPlayer != null)
            Destroy(currentPlayer);
        
        foreach (GameObject enemy in spawnedEnemies)
        {
            if (enemy != null)
                Destroy(enemy);
        }
        
        foreach (GameObject coin in spawnedCoins)
        {
            if (coin != null)
                Destroy(coin);
        }
        
        spawnedEnemies.Clear();
        spawnedCoins.Clear();
        
        // Reinitialize level
        InitializeLevel();
        SpawnLevelObjects();
    }
    
    public void PauseLevel()
    {
        Time.timeScale = 0f;
    }
    
    public void ResumeLevel()
    {
        Time.timeScale = 1f;
    }
    
    public void RemoveCoin(GameObject coin)
    {
        if (spawnedCoins.Contains(coin))
        {
            spawnedCoins.Remove(coin);
        }
    }
    
    public void RemoveEnemy(GameObject enemy)
    {
        if (spawnedEnemies.Contains(enemy))
        {
            spawnedEnemies.Remove(enemy);
        }
    }
    
    void OnDrawGizmosSelected()
    {
        // Draw spawn points
        if (playerSpawnPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(playerSpawnPoint.position, 0.5f);
        }
        
        if (enemySpawnPoints != null)
        {
            Gizmos.color = Color.red;
            foreach (Transform spawnPoint in enemySpawnPoints)
            {
                if (spawnPoint != null)
                    Gizmos.DrawWireSphere(spawnPoint.position, 0.3f);
            }
        }
        
        if (coinSpawnPoints != null)
        {
            Gizmos.color = Color.yellow;
            foreach (Transform spawnPoint in coinSpawnPoints)
            {
                if (spawnPoint != null)
                    Gizmos.DrawWireSphere(spawnPoint.position, 0.2f);
            }
        }
    }
}