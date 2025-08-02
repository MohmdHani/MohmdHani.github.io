using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private int currentHealth;
    [SerializeField] private float invincibilityDuration = 2f;
    [SerializeField] private float invincibilityFlashRate = 0.1f;
    
    [Header("Damage Settings")]
    [SerializeField] private float knockbackForce = 5f;
    [SerializeField] private float knockbackDuration = 0.3f;
    [SerializeField] private LayerMask damageLayers;
    
    [Header("Visual Effects")]
    [SerializeField] private Color damageColor = Color.red;
    [SerializeField] private Color invincibilityColor = Color.white;
    [SerializeField] private GameObject damageEffect;
    [SerializeField] private GameObject deathEffect;
    
    [Header("Audio")]
    [SerializeField] private AudioClip damageSound;
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private AudioClip healSound;
    
    // Components
    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;
    private Rigidbody2D rb;
    private PlayerController playerController;
    private CameraFollow cameraFollow;
    
    // State variables
    private bool isInvincible = false;
    private bool isDead = false;
    private Color originalColor;
    private Vector3 originalPosition;
    
    // Events
    public System.Action<int> OnHealthChanged;
    public System.Action OnPlayerDied;
    public System.Action OnPlayerDamaged;
    
    void Start()
    {
        InitializeComponents();
        SetupHealth();
    }
    
    void InitializeComponents()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody2D>();
        playerController = GetComponent<PlayerController>();
        cameraFollow = FindObjectOfType<CameraFollow>();
        
        // Store original color
        if (spriteRenderer)
        {
            originalColor = spriteRenderer.color;
        }
        
        // Store original position
        originalPosition = transform.position;
        
        // Add audio source if not present
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Configure audio source
        audioSource.playOnAwake = false;
        audioSource.volume = 0.7f;
    }
    
    void SetupHealth()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth);
    }
    
    public void TakeDamage(int damage)
    {
        if (isDead || isInvincible) return;
        
        // Apply damage
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);
        
        // Trigger events
        OnHealthChanged?.Invoke(currentHealth);
        OnPlayerDamaged?.Invoke();
        
        // Play damage sound
        if (audioSource && damageSound)
        {
            audioSource.PlayOneShot(damageSound);
        }
        
        // Spawn damage effect
        if (damageEffect)
        {
            Instantiate(damageEffect, transform.position, Quaternion.identity);
        }
        
        // Shake camera
        if (cameraFollow)
        {
            cameraFollow.ShakeCamera(0.2f, 0.3f);
        }
        
        // Apply knockback
        StartCoroutine(ApplyKnockback());
        
        // Start invincibility
        StartCoroutine(InvincibilityFrames());
        
        // Check for death
        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // Vibrate device
            if (Application.isMobilePlatform)
            {
                Handheld.Vibrate();
            }
        }
    }
    
    public void Heal(int amount)
    {
        if (isDead) return;
        
        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        
        OnHealthChanged?.Invoke(currentHealth);
        
        // Play heal sound
        if (audioSource && healSound)
        {
            audioSource.PlayOneShot(healSound);
        }
        
        // Visual feedback
        StartCoroutine(HealFlash());
    }
    
    public void SetInvincible(bool invincible)
    {
        isInvincible = invincible;
        
        if (invincible)
        {
            StartCoroutine(InvincibilityFrames());
        }
    }
    
    public void SetMaxHealth(int newMaxHealth)
    {
        maxHealth = newMaxHealth;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        OnHealthChanged?.Invoke(currentHealth);
    }
    
    public void RestoreFullHealth()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth);
    }
    
    void Die()
    {
        if (isDead) return;
        
        isDead = true;
        
        // Disable player controller
        if (playerController)
        {
            playerController.enabled = false;
        }
        
        // Disable rigidbody
        if (rb)
        {
            rb.velocity = Vector2.zero;
            rb.isKinematic = true;
        }
        
        // Play death sound
        if (audioSource && deathSound)
        {
            audioSource.PlayOneShot(deathSound);
        }
        
        // Spawn death effect
        if (deathEffect)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }
        
        // Shake camera
        if (cameraFollow)
        {
            cameraFollow.ShakeCamera(0.5f, 0.8f);
        }
        
        // Vibrate device
        if (Application.isMobilePlatform)
        {
            Handheld.Vibrate();
        }
        
        // Trigger death event
        OnPlayerDied?.Invoke();
        
        // Notify game manager
        if (GameManager.Instance)
        {
            GameManager.Instance.GameOver();
        }
        
        // Start death animation
        StartCoroutine(DeathAnimation());
    }
    
    IEnumerator InvincibilityFrames()
    {
        isInvincible = true;
        float elapsed = 0f;
        
        while (elapsed < invincibilityDuration)
        {
            elapsed += Time.deltaTime;
            
            // Flash effect
            if (spriteRenderer)
            {
                float flashValue = Mathf.Sin(elapsed * invincibilityFlashRate * Mathf.PI * 2f);
                spriteRenderer.color = flashValue > 0 ? invincibilityColor : originalColor;
            }
            
            yield return null;
        }
        
        // Restore original color
        if (spriteRenderer)
        {
            spriteRenderer.color = originalColor;
        }
        
        isInvincible = false;
    }
    
    IEnumerator ApplyKnockback()
    {
        if (rb == null) yield break;
        
        // Calculate knockback direction (away from damage source)
        Vector2 knockbackDirection = Vector2.up + Vector2.right * Random.Range(-0.5f, 0.5f);
        knockbackDirection.Normalize();
        
        // Apply knockback force
        rb.velocity = knockbackDirection * knockbackForce;
        
        yield return new WaitForSeconds(knockbackDuration);
        
        // Reset velocity
        rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y);
    }
    
    IEnumerator HealFlash()
    {
        if (spriteRenderer == null) yield break;
        
        // Flash green for healing
        Color healColor = Color.green;
        spriteRenderer.color = healColor;
        
        yield return new WaitForSeconds(0.2f);
        
        spriteRenderer.color = originalColor;
    }
    
    IEnumerator DeathAnimation()
    {
        // Fade out
        float duration = 1f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            
            if (spriteRenderer)
            {
                Color color = spriteRenderer.color;
                color.a = alpha;
                spriteRenderer.color = color;
            }
            
            yield return null;
        }
        
        // Disable sprite renderer
        if (spriteRenderer)
        {
            spriteRenderer.enabled = false;
        }
    }
    
    public void Respawn()
    {
        if (!isDead) return;
        
        // Reset position
        transform.position = originalPosition;
        
        // Restore health
        currentHealth = maxHealth;
        
        // Reset state
        isDead = false;
        isInvincible = false;
        
        // Restore components
        if (playerController)
        {
            playerController.enabled = true;
        }
        
        if (rb)
        {
            rb.isKinematic = false;
        }
        
        if (spriteRenderer)
        {
            spriteRenderer.enabled = true;
            spriteRenderer.color = originalColor;
        }
        
        // Trigger events
        OnHealthChanged?.Invoke(currentHealth);
    }
    
    // Public getters
    public int GetCurrentHealth() => currentHealth;
    public int GetMaxHealth() => maxHealth;
    public bool IsDead() => isDead;
    public bool IsInvincible() => isInvincible;
    public float GetHealthPercentage() => (float)currentHealth / maxHealth;
    
    void OnTriggerEnter2D(Collider2D other)
    {
        // Check for damage sources
        if (isInvincible || isDead) return;
        
        // Check if other object is in damage layers
        if (((1 << other.gameObject.layer) & damageLayers) != 0)
        {
            // Get damage amount from enemy or hazard
            EnemyController enemy = other.GetComponent<EnemyController>();
            if (enemy)
            {
                TakeDamage(1); // Default damage
            }
            else
            {
                // Check for hazard component
                Hazard hazard = other.GetComponent<Hazard>();
                if (hazard)
                {
                    TakeDamage(hazard.GetDamage());
                }
                else
                {
                    // Default damage for unknown damage sources
                    TakeDamage(1);
                }
            }
        }
    }
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        // Check for damage sources in collision
        if (isInvincible || isDead) return;
        
        if (((1 << collision.gameObject.layer) & damageLayers) != 0)
        {
            // Get damage amount
            EnemyController enemy = collision.gameObject.GetComponent<EnemyController>();
            if (enemy)
            {
                TakeDamage(1);
            }
        }
    }
}

// Simple hazard class for damage sources
public class Hazard : MonoBehaviour
{
    [SerializeField] private int damage = 1;
    
    public int GetDamage() => damage;
}