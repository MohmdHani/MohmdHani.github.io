using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [Header("Checkpoint Settings")]
    public bool isActive = false;
    public bool isFinalCheckpoint = false;
    
    [Header("Visual Effects")]
    public SpriteRenderer checkpointSprite;
    public Color inactiveColor = Color.gray;
    public Color activeColor = Color.green;
    public Color finalColor = Color.yellow;
    public float pulseSpeed = 2f;
    public float pulseIntensity = 0.2f;
    
    [Header("Audio")]
    public AudioClip activateSound;
    
    private bool isPulsing = false;
    private Color targetColor;
    private Vector3 originalScale;
    
    void Start()
    {
        originalScale = transform.localScale;
        UpdateVisualState();
    }
    
    void Update()
    {
        if (isActive && isPulsing)
        {
            // Pulse effect
            float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseIntensity;
            transform.localScale = originalScale * pulse;
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            ActivateCheckpoint();
        }
    }
    
    void ActivateCheckpoint()
    {
        if (isActive) return;
        
        isActive = true;
        
        // Set player respawn point
        PlayerHealth playerHealth = FindObjectOfType<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.SetRespawnPoint(transform.position);
        }
        
        // Update visual state
        UpdateVisualState();
        StartPulsing();
        
        // Play sound
        if (activateSound != null)
        {
            AudioManager.Instance?.PlaySound("checkpoint");
        }
        
        // Deactivate other checkpoints in the level
        DeactivateOtherCheckpoints();
        
        // Check if this is the final checkpoint
        if (isFinalCheckpoint)
        {
            OnFinalCheckpointReached();
        }
    }
    
    void UpdateVisualState()
    {
        if (checkpointSprite != null)
        {
            if (isFinalCheckpoint)
            {
                targetColor = finalColor;
            }
            else if (isActive)
            {
                targetColor = activeColor;
            }
            else
            {
                targetColor = inactiveColor;
            }
            
            checkpointSprite.color = targetColor;
        }
    }
    
    void StartPulsing()
    {
        isPulsing = true;
    }
    
    void StopPulsing()
    {
        isPulsing = false;
        transform.localScale = originalScale;
    }
    
    void DeactivateOtherCheckpoints()
    {
        Checkpoint[] allCheckpoints = FindObjectsOfType<Checkpoint>();
        foreach (Checkpoint checkpoint in allCheckpoints)
        {
            if (checkpoint != this)
            {
                checkpoint.Deactivate();
            }
        }
    }
    
    public void Deactivate()
    {
        isActive = false;
        StopPulsing();
        UpdateVisualState();
    }
    
    void OnFinalCheckpointReached()
    {
        // Trigger level completion
        GameManager.Instance?.CompleteLevel();
        
        // Show completion message
        Debug.Log("Level completed! Final checkpoint reached.");
    }
    
    public void SetAsFinalCheckpoint()
    {
        isFinalCheckpoint = true;
        UpdateVisualState();
    }
    
    void OnDrawGizmosSelected()
    {
        // Draw checkpoint area
        Gizmos.color = isActive ? Color.green : Color.gray;
        Gizmos.DrawWireCube(transform.position, Vector3.one);
        
        // Draw respawn point
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}