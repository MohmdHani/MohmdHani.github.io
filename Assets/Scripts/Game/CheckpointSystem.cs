using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CheckpointSystem : MonoBehaviour
{
    [System.Serializable]
    public class Checkpoint
    {
        public Transform checkpointTransform;
        public bool isActivated = false;
        public bool isEndPoint = false;
        public int checkpointNumber;
        public GameObject activationEffect;
        public AudioClip activationSound;
        public string checkpointName = "Checkpoint";
    }
    
    [Header("Checkpoint Settings")]
    [SerializeField] private List<Checkpoint> checkpoints = new List<Checkpoint>();
    [SerializeField] private int currentCheckpointIndex = 0;
    [SerializeField] private bool autoActivateFirst = true;
    [SerializeField] private float respawnDelay = 1f;
    
    [Header("Visual Effects")]
    [SerializeField] private GameObject checkpointActivationEffect;
    [SerializeField] private GameObject respawnEffect;
    [SerializeField] private Color activatedColor = Color.green;
    [SerializeField] private Color inactiveColor = Color.gray;
    
    [Header("Audio")]
    [SerializeField] private AudioClip checkpointActivationSound;
    [SerializeField] private AudioClip respawnSound;
    
    // Components
    private PlayerController player;
    private Transform playerTransform;
    private AudioSource audioSource;
    private LevelManager levelManager;
    
    // State
    private bool isRespawning = false;
    private Vector3 lastCheckpointPosition;
    
    // Events
    public System.Action<int> OnCheckpointActivated;
    public System.Action<int> OnCheckpointReached;
    public System.Action OnEndPointReached;
    
    void Start()
    {
        InitializeCheckpointSystem();
    }
    
    void InitializeCheckpointSystem()
    {
        // Find components
        player = FindObjectOfType<PlayerController>();
        if (player) playerTransform = player.transform;
        
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        levelManager = FindObjectOfType<LevelManager>();
        
        // Setup checkpoints
        SetupCheckpoints();
        
        // Activate first checkpoint if auto-activate is enabled
        if (autoActivateFirst && checkpoints.Count > 0)
        {
            ActivateCheckpoint(0);
        }
        
        // Store initial position
        if (playerTransform)
        {
            lastCheckpointPosition = playerTransform.position;
        }
    }
    
    void SetupCheckpoints()
    {
        for (int i = 0; i < checkpoints.Count; i++)
        {
            Checkpoint checkpoint = checkpoints[i];
            checkpoint.checkpointNumber = i;
            
            // Setup visual representation
            SetupCheckpointVisual(checkpoint);
            
            // Add trigger collider if not present
            if (checkpoint.checkpointTransform)
            {
                AddCheckpointTrigger(checkpoint);
            }
        }
    }
    
    void SetupCheckpointVisual(Checkpoint checkpoint)
    {
        if (checkpoint.checkpointTransform == null) return;
        
        // Find or create visual representation
        SpriteRenderer visual = checkpoint.checkpointTransform.GetComponent<SpriteRenderer>();
        if (visual == null)
        {
            visual = checkpoint.checkpointTransform.gameObject.AddComponent<SpriteRenderer>();
        }
        
        // Set initial color
        visual.color = checkpoint.isActivated ? activatedColor : inactiveColor;
        
        // Add glow effect for end point
        if (checkpoint.isEndPoint)
        {
            // Add special visual for end point
            visual.color = Color.yellow;
        }
    }
    
    void AddCheckpointTrigger(Checkpoint checkpoint)
    {
        // Add trigger collider
        Collider2D trigger = checkpoint.checkpointTransform.GetComponent<Collider2D>();
        if (trigger == null)
        {
            trigger = checkpoint.checkpointTransform.gameObject.AddComponent<BoxCollider2D>();
        }
        
        trigger.isTrigger = true;
        
        // Add checkpoint component for detection
        CheckpointTrigger checkpointTrigger = checkpoint.checkpointTransform.GetComponent<CheckpointTrigger>();
        if (checkpointTrigger == null)
        {
            checkpointTrigger = checkpoint.checkpointTransform.gameObject.AddComponent<CheckpointTrigger>();
        }
        
        checkpointTrigger.Initialize(checkpoint, this);
    }
    
    public void ActivateCheckpoint(int checkpointIndex)
    {
        if (checkpointIndex < 0 || checkpointIndex >= checkpoints.Count) return;
        
        Checkpoint checkpoint = checkpoints[checkpointIndex];
        if (checkpoint.isActivated) return;
        
        // Activate checkpoint
        checkpoint.isActivated = true;
        currentCheckpointIndex = checkpointIndex;
        
        // Update visual
        UpdateCheckpointVisual(checkpoint);
        
        // Store position
        if (checkpoint.checkpointTransform)
        {
            lastCheckpointPosition = checkpoint.checkpointTransform.position;
        }
        
        // Play effects
        PlayActivationEffects(checkpoint);
        
        // Trigger events
        OnCheckpointActivated?.Invoke(checkpointIndex);
        
        // Notify level manager
        if (levelManager)
        {
            levelManager.SetCheckpoint(checkpointIndex);
        }
        
        // Save progress
        SaveCheckpointProgress();
    }
    
    void UpdateCheckpointVisual(Checkpoint checkpoint)
    {
        if (checkpoint.checkpointTransform == null) return;
        
        SpriteRenderer visual = checkpoint.checkpointTransform.GetComponent<SpriteRenderer>();
        if (visual)
        {
            visual.color = activatedColor;
        }
    }
    
    void PlayActivationEffects(Checkpoint checkpoint)
    {
        // Play activation sound
        AudioClip soundToPlay = checkpoint.activationSound != null ? checkpoint.activationSound : checkpointActivationSound;
        if (audioSource && soundToPlay)
        {
            audioSource.PlayOneShot(soundToPlay);
        }
        
        // Spawn activation effect
        GameObject effectToSpawn = checkpoint.activationEffect != null ? checkpoint.activationEffect : checkpointActivationEffect;
        if (effectToSpawn && checkpoint.checkpointTransform)
        {
            Instantiate(effectToSpawn, checkpoint.checkpointTransform.position, Quaternion.identity);
        }
    }
    
    public void RespawnPlayer()
    {
        if (isRespawning) return;
        
        StartCoroutine(RespawnCoroutine());
    }
    
    IEnumerator RespawnCoroutine()
    {
        isRespawning = true;
        
        // Disable player temporarily
        if (player)
        {
            player.enabled = false;
        }
        
        // Wait for respawn delay
        yield return new WaitForSeconds(respawnDelay);
        
        // Move player to checkpoint
        if (playerTransform)
        {
            playerTransform.position = lastCheckpointPosition;
        }
        
        // Spawn respawn effect
        if (respawnEffect && playerTransform)
        {
            Instantiate(respawnEffect, playerTransform.position, Quaternion.identity);
        }
        
        // Play respawn sound
        if (audioSource && respawnSound)
        {
            audioSource.PlayOneShot(respawnSound);
        }
        
        // Re-enable player
        if (player)
        {
            player.enabled = true;
            
            // Restore player health
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth)
            {
                playerHealth.Respawn();
            }
        }
        
        isRespawning = false;
    }
    
    public void ReachedEndPoint()
    {
        // Trigger level completion
        OnEndPointReached?.Invoke();
        
        // Notify level manager
        if (levelManager)
        {
            levelManager.LevelComplete();
        }
    }
    
    public void SetCurrentCheckpoint(int checkpointIndex)
    {
        if (checkpointIndex >= 0 && checkpointIndex < checkpoints.Count)
        {
            currentCheckpointIndex = checkpointIndex;
            
            // Update last checkpoint position
            if (checkpoints[checkpointIndex].checkpointTransform)
            {
                lastCheckpointPosition = checkpoints[checkpointIndex].checkpointTransform.position;
            }
        }
    }
    
    public Checkpoint GetCurrentCheckpoint()
    {
        if (currentCheckpointIndex >= 0 && currentCheckpointIndex < checkpoints.Count)
        {
            return checkpoints[currentCheckpointIndex];
        }
        return null;
    }
    
    public Vector3 GetCurrentCheckpointPosition()
    {
        return lastCheckpointPosition;
    }
    
    public int GetCurrentCheckpointIndex()
    {
        return currentCheckpointIndex;
    }
    
    public List<Checkpoint> GetAllCheckpoints()
    {
        return checkpoints;
    }
    
    public bool HasEndPoint()
    {
        return checkpoints.Exists(cp => cp.isEndPoint);
    }
    
    public Checkpoint GetEndPoint()
    {
        return checkpoints.Find(cp => cp.isEndPoint);
    }
    
    void SaveCheckpointProgress()
    {
        // Save checkpoint progress to PlayerPrefs
        PlayerPrefs.SetInt("CurrentCheckpoint", currentCheckpointIndex);
        PlayerPrefs.Save();
    }
    
    void LoadCheckpointProgress()
    {
        if (PlayerPrefs.HasKey("CurrentCheckpoint"))
        {
            int savedCheckpoint = PlayerPrefs.GetInt("CurrentCheckpoint");
            if (savedCheckpoint >= 0 && savedCheckpoint < checkpoints.Count)
            {
                currentCheckpointIndex = savedCheckpoint;
                
                // Activate all checkpoints up to the saved one
                for (int i = 0; i <= savedCheckpoint; i++)
                {
                    if (i < checkpoints.Count)
                    {
                        checkpoints[i].isActivated = true;
                        UpdateCheckpointVisual(checkpoints[i]);
                    }
                }
                
                // Update last checkpoint position
                if (checkpoints[savedCheckpoint].checkpointTransform)
                {
                    lastCheckpointPosition = checkpoints[savedCheckpoint].checkpointTransform.position;
                }
            }
        }
    }
    
    void OnDrawGizmosSelected()
    {
        // Draw checkpoint connections
        for (int i = 0; i < checkpoints.Count - 1; i++)
        {
            if (checkpoints[i].checkpointTransform && checkpoints[i + 1].checkpointTransform)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(
                    checkpoints[i].checkpointTransform.position,
                    checkpoints[i + 1].checkpointTransform.position
                );
            }
        }
        
        // Draw checkpoint positions
        for (int i = 0; i < checkpoints.Count; i++)
        {
            if (checkpoints[i].checkpointTransform)
            {
                Gizmos.color = checkpoints[i].isEndPoint ? Color.yellow : Color.green;
                Gizmos.DrawWireSphere(checkpoints[i].checkpointTransform.position, 0.5f);
                
                // Draw checkpoint number
                #if UNITY_EDITOR
                UnityEditor.Handles.Label(checkpoints[i].checkpointTransform.position + Vector3.up, i.ToString());
                #endif
            }
        }
    }
}

// Helper component for checkpoint detection
public class CheckpointTrigger : MonoBehaviour
{
    private Checkpoint checkpoint;
    private CheckpointSystem checkpointSystem;
    
    public void Initialize(Checkpoint cp, CheckpointSystem cs)
    {
        checkpoint = cp;
        checkpointSystem = cs;
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Activate checkpoint
            checkpointSystem.ActivateCheckpoint(checkpoint.checkpointNumber);
            
            // Check if this is an end point
            if (checkpoint.isEndPoint)
            {
                checkpointSystem.ReachedEndPoint();
            }
        }
    }
}