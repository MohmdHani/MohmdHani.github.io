using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class LevelProgressManager : MonoBehaviour
{
    public static LevelProgressManager Instance { get; private set; }
    
    [System.Serializable]
    public class LevelData
    {
        public int levelNumber;
        public int worldNumber;
        public bool isUnlocked;
        public bool isCompleted;
        public int starsEarned; // 0-3 stars
        public float bestTime;
        public int bestScore;
        public int coinsCollected;
        public bool allCoinsCollected;
        public bool allEnemiesDefeated;
    }
    
    [System.Serializable]
    public class WorldData
    {
        public int worldNumber;
        public string worldName;
        public bool isUnlocked;
        public int totalLevels;
        public int completedLevels;
        public int totalStars;
        public int requiredStarsToUnlock; // Stars needed from previous world
        public Sprite worldIcon;
        public Color worldColor;
    }
    
    [Header("World Settings")]
    [SerializeField] private List<WorldData> worlds = new List<WorldData>();
    [SerializeField] private int totalWorlds = 5;
    [SerializeField] private int levelsPerWorld = 10;
    
    [Header("Progression Settings")]
    [SerializeField] private int starsRequiredToUnlockWorld = 15;
    [SerializeField] private bool requirePreviousWorldCompletion = true;
    
    [Header("Save Settings")]
    [SerializeField] private string saveKey = "LevelProgress";
    [SerializeField] private bool autoSave = true;
    
    // Private data
    private Dictionary<int, LevelData> levelProgress = new Dictionary<int, LevelData>();
    private Dictionary<int, WorldData> worldProgress = new Dictionary<int, WorldData>();
    
    // Events
    public System.Action<int> OnLevelUnlocked;
    public System.Action<int> OnLevelCompleted;
    public System.Action<int> OnWorldUnlocked;
    public System.Action<int> OnStarsChanged;
    
    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeProgress();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        LoadProgress();
        UnlockInitialContent();
    }
    
    void InitializeProgress()
    {
        // Initialize worlds
        for (int i = 1; i <= totalWorlds; i++)
        {
            WorldData world = new WorldData
            {
                worldNumber = i,
                worldName = $"World {i}",
                isUnlocked = i == 1, // Only first world unlocked initially
                totalLevels = levelsPerWorld,
                completedLevels = 0,
                totalStars = 0,
                requiredStarsToUnlock = i == 1 ? 0 : starsRequiredToUnlockWorld * (i - 1)
            };
            
            worlds.Add(world);
            worldProgress[i] = world;
        }
        
        // Initialize levels
        for (int world = 1; world <= totalWorlds; world++)
        {
            for (int level = 1; level <= levelsPerWorld; level++)
            {
                int levelNumber = GetLevelNumber(world, level);
                LevelData levelData = new LevelData
                {
                    levelNumber = levelNumber,
                    worldNumber = world,
                    isUnlocked = world == 1 && level == 1, // Only first level unlocked initially
                    isCompleted = false,
                    starsEarned = 0,
                    bestTime = 0f,
                    bestScore = 0,
                    coinsCollected = 0,
                    allCoinsCollected = false,
                    allEnemiesDefeated = false
                };
                
                levelProgress[levelNumber] = levelData;
            }
        }
    }
    
    void UnlockInitialContent()
    {
        // Unlock first world and first level
        UnlockWorld(1);
        UnlockLevel(1);
    }
    
    public void CompleteLevel(int levelNumber, int stars, float time, int score, int coins, bool allCoins, bool allEnemies)
    {
        if (!levelProgress.ContainsKey(levelNumber)) return;
        
        LevelData level = levelProgress[levelNumber];
        bool wasCompleted = level.isCompleted;
        
        // Update level data
        level.isCompleted = true;
        level.starsEarned = Mathf.Max(level.starsEarned, stars);
        level.bestTime = level.bestTime == 0f ? time : Mathf.Min(level.bestTime, time);
        level.bestScore = Mathf.Max(level.bestScore, score);
        level.coinsCollected = Mathf.Max(level.coinsCollected, coins);
        level.allCoinsCollected = allCoins;
        level.allEnemiesDefeated = allEnemies;
        
        // Update world progress
        UpdateWorldProgress(level.worldNumber);
        
        // Unlock next level
        UnlockNextLevel(levelNumber);
        
        // Trigger events
        OnLevelCompleted?.Invoke(levelNumber);
        OnStarsChanged?.Invoke(GetTotalStars());
        
        // Save progress
        if (autoSave) SaveProgress();
    }
    
    void UpdateWorldProgress(int worldNumber)
    {
        if (!worldProgress.ContainsKey(worldNumber)) return;
        
        WorldData world = worldProgress[worldNumber];
        world.completedLevels = GetCompletedLevelsInWorld(worldNumber);
        world.totalStars = GetStarsInWorld(worldNumber);
        
        // Check if world is complete
        if (world.completedLevels >= world.totalLevels)
        {
            UnlockNextWorld(worldNumber);
        }
    }
    
    void UnlockNextLevel(int currentLevelNumber)
    {
        int nextLevelNumber = currentLevelNumber + 1;
        
        // Check if next level exists and unlock it
        if (levelProgress.ContainsKey(nextLevelNumber))
        {
            LevelData nextLevel = levelProgress[nextLevelNumber];
            
            // Unlock if it's in the same world or if previous world is complete
            if (nextLevel.worldNumber == levelProgress[currentLevelNumber].worldNumber ||
                IsWorldComplete(nextLevel.worldNumber - 1))
            {
                UnlockLevel(nextLevelNumber);
            }
        }
    }
    
    void UnlockNextWorld(int currentWorldNumber)
    {
        int nextWorldNumber = currentWorldNumber + 1;
        
        if (worldProgress.ContainsKey(nextWorldNumber))
        {
            WorldData nextWorld = worldProgress[nextWorldNumber];
            
            // Check if enough stars earned
            int totalStars = GetTotalStars();
            if (totalStars >= nextWorld.requiredStarsToUnlock)
            {
                UnlockWorld(nextWorldNumber);
            }
        }
    }
    
    public void UnlockLevel(int levelNumber)
    {
        if (!levelProgress.ContainsKey(levelNumber)) return;
        
        LevelData level = levelProgress[levelNumber];
        if (!level.isUnlocked)
        {
            level.isUnlocked = true;
            OnLevelUnlocked?.Invoke(levelNumber);
        }
    }
    
    public void UnlockWorld(int worldNumber)
    {
        if (!worldProgress.ContainsKey(worldNumber)) return;
        
        WorldData world = worldProgress[worldNumber];
        if (!world.isUnlocked)
        {
            world.isUnlocked = true;
            
            // Unlock first level of this world
            int firstLevelNumber = GetLevelNumber(worldNumber, 1);
            UnlockLevel(firstLevelNumber);
            
            OnWorldUnlocked?.Invoke(worldNumber);
        }
    }
    
    public bool IsLevelUnlocked(int levelNumber)
    {
        return levelProgress.ContainsKey(levelNumber) && levelProgress[levelNumber].isUnlocked;
    }
    
    public bool IsLevelCompleted(int levelNumber)
    {
        return levelProgress.ContainsKey(levelNumber) && levelProgress[levelNumber].isCompleted;
    }
    
    public bool IsWorldUnlocked(int worldNumber)
    {
        return worldProgress.ContainsKey(worldNumber) && worldProgress[worldNumber].isUnlocked;
    }
    
    public bool IsWorldComplete(int worldNumber)
    {
        if (!worldProgress.ContainsKey(worldNumber)) return false;
        
        WorldData world = worldProgress[worldNumber];
        return world.completedLevels >= world.totalLevels;
    }
    
    public LevelData GetLevelData(int levelNumber)
    {
        return levelProgress.ContainsKey(levelNumber) ? levelProgress[levelNumber] : null;
    }
    
    public WorldData GetWorldData(int worldNumber)
    {
        return worldProgress.ContainsKey(worldNumber) ? worldProgress[worldNumber] : null;
    }
    
    public List<LevelData> GetLevelsInWorld(int worldNumber)
    {
        return levelProgress.Values
            .Where(level => level.worldNumber == worldNumber)
            .OrderBy(level => level.levelNumber)
            .ToList();
    }
    
    public List<WorldData> GetAllWorlds()
    {
        return worlds;
    }
    
    public int GetCompletedLevelsInWorld(int worldNumber)
    {
        return levelProgress.Values.Count(level => level.worldNumber == worldNumber && level.isCompleted);
    }
    
    public int GetStarsInWorld(int worldNumber)
    {
        return levelProgress.Values
            .Where(level => level.worldNumber == worldNumber)
            .Sum(level => level.starsEarned);
    }
    
    public int GetTotalStars()
    {
        return levelProgress.Values.Sum(level => level.starsEarned);
    }
    
    public int GetTotalCompletedLevels()
    {
        return levelProgress.Values.Count(level => level.isCompleted);
    }
    
    public int GetLevelNumber(int world, int level)
    {
        return (world - 1) * levelsPerWorld + level;
    }
    
    public Vector2Int GetWorldAndLevel(int levelNumber)
    {
        int world = (levelNumber - 1) / levelsPerWorld + 1;
        int level = ((levelNumber - 1) % levelsPerWorld) + 1;
        return new Vector2Int(world, level);
    }
    
    public int GetNextUnlockedLevel()
    {
        return levelProgress.Values
            .Where(level => level.isUnlocked && !level.isCompleted)
            .OrderBy(level => level.levelNumber)
            .FirstOrDefault()?.levelNumber ?? 1;
    }
    
    public void SaveProgress()
    {
        string progressJson = JsonUtility.ToJson(new ProgressData
        {
            levelProgress = levelProgress.Values.ToList(),
            worldProgress = worldProgress.Values.ToList()
        });
        
        PlayerPrefs.SetString(saveKey, progressJson);
        PlayerPrefs.Save();
    }
    
    public void LoadProgress()
    {
        if (PlayerPrefs.HasKey(saveKey))
        {
            string progressJson = PlayerPrefs.GetString(saveKey);
            ProgressData data = JsonUtility.FromJson<ProgressData>(progressJson);
            
            // Load level progress
            foreach (LevelData levelData in data.levelProgress)
            {
                levelProgress[levelData.levelNumber] = levelData;
            }
            
            // Load world progress
            foreach (WorldData worldData in data.worldProgress)
            {
                worldProgress[worldData.worldNumber] = worldData;
                worlds[worldData.worldNumber - 1] = worldData;
            }
        }
    }
    
    public void ResetProgress()
    {
        PlayerPrefs.DeleteKey(saveKey);
        PlayerPrefs.Save();
        
        // Reinitialize
        levelProgress.Clear();
        worldProgress.Clear();
        worlds.Clear();
        
        InitializeProgress();
        UnlockInitialContent();
    }
    
    public void UnlockAllLevels()
    {
        foreach (var level in levelProgress.Values)
        {
            level.isUnlocked = true;
        }
        
        foreach (var world in worldProgress.Values)
        {
            world.isUnlocked = true;
        }
        
        SaveProgress();
    }
    
    [System.Serializable]
    private class ProgressData
    {
        public List<LevelData> levelProgress;
        public List<WorldData> worldProgress;
    }
}