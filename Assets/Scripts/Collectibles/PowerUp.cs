using UnityEngine;
using System.Collections;

public class PowerUp : MonoBehaviour
{
    [System.Serializable]
    public enum PowerUpType
    {
        DoubleJump,
        SpeedBoost,
        Invincibility,
        ExtraLife,
        Magnet,
        Shield
    }
    
    [Header("Power-Up Settings")]
    [SerializeField] private PowerUpType powerUpType = PowerUpType.DoubleJump;
    [SerializeField] private float duration = 10f;
    [SerializeField] private float rotationSpeed = 90f;
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobHeight = 0.3f;
    
    [Header("Visual Effects")]
    [SerializeField] private Color powerUpColor = Color.cyan;
    [SerializeField] private float glowIntensity = 2f;
    [SerializeField] private bool enablePulse = true;
    [SerializeField] private float pulseSpeed = 3f;
    
    [Header("Collection Effects")]
    [SerializeField] private GameObject collectEffect;
    [SerializeField] private AudioClip collectSound;
    [SerializeField] private ParticleSystem powerUpParticles;
    
    // Components
    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;
    private Collider2D powerUpCollider;
    private PlayerController playerController;
    
    // Animation variables
    private Vector3 startPosition;
    private float bobTimer = 0f;
    private float pulseTimer = 0f;
    private bool isCollected = false;
    
    // Visual effects
    private Material originalMaterial;
    private Material glowMaterial;
    private Color originalColor;
    
    void Start()
    {
        InitializeComponents();
        SetupVisualEffects();
    }
    
    void InitializeComponents()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        powerUpCollider = GetComponent<Collider2D>();
        
        // Store starting position for bob animation
        startPosition = transform.position;
        originalColor = spriteRenderer.color;
        
        // Add audio source if not present
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Configure audio source
        audioSource.playOnAwake = false;
        audioSource.volume = 0.7f;
    }
    
    void SetupVisualEffects()
    {
        if (spriteRenderer == null) return;
        
        // Store original material
        originalMaterial = spriteRenderer.material;
        
        // Create glow material
        if (glowMaterial == null)
        {
            glowMaterial = new Material(originalMaterial);
            glowMaterial.SetColor("_EmissionColor", powerUpColor * glowIntensity);
        }
        
        // Apply glow material
        spriteRenderer.material = glowMaterial;
        
        // Set power-up color
        spriteRenderer.color = powerUpColor;
    }
    
    void Update()
    {
        if (isCollected) return;
        
        HandleRotation();
        HandleBobAnimation();
        HandlePulseAnimation();
    }
    
    void HandleRotation()
    {
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
    }
    
    void HandleBobAnimation()
    {
        bobTimer += Time.deltaTime * bobSpeed;
        float bobOffset = Mathf.Sin(bobTimer) * bobHeight;
        
        Vector3 newPosition = startPosition;
        newPosition.y += bobOffset;
        transform.position = newPosition;
    }
    
    void HandlePulseAnimation()
    {
        if (!enablePulse) return;
        
        pulseTimer += Time.deltaTime * pulseSpeed;
        float pulseScale = 1f + Mathf.Sin(pulseTimer) * 0.1f;
        
        transform.localScale = Vector3.one * pulseScale;
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (isCollected) return;
        
        if (other.CompareTag("Player"))
        {
            playerController = other.GetComponent<PlayerController>();
            if (playerController)
            {
                Collect();
            }
        }
    }
    
    void Collect()
    {
        if (isCollected) return;
        
        isCollected = true;
        
        // Disable collider
        if (powerUpCollider)
        {
            powerUpCollider.enabled = false;
        }
        
        // Apply power-up effect
        ApplyPowerUpEffect();
        
        // Play collection sound
        if (audioSource && collectSound)
        {
            audioSource.PlayOneShot(collectSound);
        }
        
        // Spawn collection effect
        if (collectEffect)
        {
            GameObject effect = Instantiate(collectEffect, transform.position, Quaternion.identity);
            Destroy(effect, 2f);
        }
        
        // Play particle effect
        if (powerUpParticles)
        {
            powerUpParticles.Play();
        }
        
        // Start collection animation
        StartCoroutine(CollectionAnimation());
    }
    
    void ApplyPowerUpEffect()
    {
        if (playerController == null) return;
        
        switch (powerUpType)
        {
            case PowerUpType.DoubleJump:
                playerController.EnableDoubleJump();
                StartCoroutine(DisableDoubleJumpAfterDuration());
                break;
                
            case PowerUpType.SpeedBoost:
                StartCoroutine(ApplySpeedBoost());
                break;
                
            case PowerUpType.Invincibility:
                StartCoroutine(ApplyInvincibility());
                break;
                
            case PowerUpType.ExtraLife:
                if (GameManager.Instance)
                {
                    GameManager.Instance.AddLives(1);
                }
                break;
                
            case PowerUpType.Magnet:
                StartCoroutine(ApplyMagnetEffect());
                break;
                
            case PowerUpType.Shield:
                StartCoroutine(ApplyShieldEffect());
                break;
        }
        
        // Add score
        if (GameManager.Instance)
        {
            GameManager.Instance.AddScore(50);
        }
    }
    
    IEnumerator DisableDoubleJumpAfterDuration()
    {
        yield return new WaitForSeconds(duration);
        if (playerController)
        {
            playerController.DisableDoubleJump();
        }
    }
    
    IEnumerator ApplySpeedBoost()
    {
        // Store original speed
        float originalSpeed = 8f; // This should be accessed from PlayerController
        
        // Apply speed boost
        // playerController.SetMoveSpeed(originalSpeed * 1.5f);
        
        yield return new WaitForSeconds(duration);
        
        // Restore original speed
        // playerController.SetMoveSpeed(originalSpeed);
    }
    
    IEnumerator ApplyInvincibility()
    {
        // Make player invincible
        PlayerHealth playerHealth = playerController.GetComponent<PlayerHealth>();
        if (playerHealth)
        {
            playerHealth.SetInvincible(true);
        }
        
        yield return new WaitForSeconds(duration);
        
        // Remove invincibility
        if (playerHealth)
        {
            playerHealth.SetInvincible(false);
        }
    }
    
    IEnumerator ApplyMagnetEffect()
    {
        // Enable coin magnet effect
        // This would need to be implemented in a CoinMagnet component
        
        yield return new WaitForSeconds(duration);
        
        // Disable coin magnet effect
    }
    
    IEnumerator ApplyShieldEffect()
    {
        // Enable shield effect
        // This would need to be implemented in a Shield component
        
        yield return new WaitForSeconds(duration);
        
        // Disable shield effect
    }
    
    IEnumerator CollectionAnimation()
    {
        // Scale up and fade out
        float duration = 0.8f;
        float elapsed = 0f;
        
        Vector3 originalScale = transform.localScale;
        Color originalColor = spriteRenderer.color;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            
            // Scale up more than coins
            float scale = Mathf.Lerp(1f, 2f, progress);
            transform.localScale = originalScale * scale;
            
            // Fade out
            Color newColor = originalColor;
            newColor.a = Mathf.Lerp(1f, 0f, progress);
            spriteRenderer.color = newColor;
            
            yield return null;
        }
        
        // Destroy the power-up
        Destroy(gameObject);
    }
    
    void OnDestroy()
    {
        // Clean up materials
        if (glowMaterial != null)
        {
            DestroyImmediate(glowMaterial);
        }
    }
    
    // Public methods for external control
    public void SetPowerUpType(PowerUpType type)
    {
        powerUpType = type;
        UpdateVisualsForType();
    }
    
    public PowerUpType GetPowerUpType()
    {
        return powerUpType;
    }
    
    public void SetDuration(float newDuration)
    {
        duration = newDuration;
    }
    
    public float GetDuration()
    {
        return duration;
    }
    
    void UpdateVisualsForType()
    {
        // Update color based on power-up type
        switch (powerUpType)
        {
            case PowerUpType.DoubleJump:
                powerUpColor = Color.cyan;
                break;
            case PowerUpType.SpeedBoost:
                powerUpColor = Color.green;
                break;
            case PowerUpType.Invincibility:
                powerUpColor = Color.yellow;
                break;
            case PowerUpType.ExtraLife:
                powerUpColor = Color.red;
                break;
            case PowerUpType.Magnet:
                powerUpColor = Color.blue;
                break;
            case PowerUpType.Shield:
                powerUpColor = Color.magenta;
                break;
        }
        
        if (spriteRenderer)
        {
            spriteRenderer.color = powerUpColor;
        }
        
        if (glowMaterial)
        {
            glowMaterial.SetColor("_EmissionColor", powerUpColor * glowIntensity);
        }
    }
}