using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 3;
    public float invincibilityTime = 2f;
    public float knockbackForce = 5f;
    
    [Header("Visual Feedback")]
    public SpriteRenderer spriteRenderer;
    public Color damageColor = Color.red;
    public float flashDuration = 0.1f;
    
    private int currentHealth;
    private bool isInvincible = false;
    private Rigidbody2D rb;
    private Vector3 respawnPoint;
    
    public int CurrentHealth => currentHealth;
    public bool IsInvincible => isInvincible;
    
    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        respawnPoint = transform.position;
        
        // Update UI
        GameManager.Instance?.UpdateHealthUI(currentHealth);
    }
    
    public void TakeDamage(int damage, Vector2 damageSource)
    {
        if (isInvincible) return;
        
        currentHealth -= damage;
        GameManager.Instance?.UpdateHealthUI(currentHealth);
        
        // Visual feedback
        StartCoroutine(FlashRed());
        
        // Knockback
        Vector2 knockbackDirection = (Vector2)transform.position - damageSource;
        knockbackDirection.Normalize();
        rb.velocity = Vector2.zero;
        rb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
        
        // Invincibility
        StartCoroutine(InvincibilityFrames());
        
        // Sound effect
        AudioManager.Instance?.PlaySound("player_hurt");
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    System.Collections.IEnumerator FlashRed()
    {
        if (spriteRenderer != null)
        {
            Color originalColor = spriteRenderer.color;
            spriteRenderer.color = damageColor;
            yield return new WaitForSeconds(flashDuration);
            spriteRenderer.color = originalColor;
        }
    }
    
    System.Collections.IEnumerator InvincibilityFrames()
    {
        isInvincible = true;
        
        // Flash effect during invincibility
        float elapsed = 0f;
        while (elapsed < invincibilityTime)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = !spriteRenderer.enabled;
            }
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
        }
        
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }
        
        isInvincible = false;
    }
    
    void Die()
    {
        AudioManager.Instance?.PlaySound("player_death");
        GameManager.Instance?.PlayerDied();
        
        // Respawn after delay
        Invoke(nameof(Respawn), 2f);
    }
    
    void Respawn()
    {
        currentHealth = maxHealth;
        transform.position = respawnPoint;
        rb.velocity = Vector2.zero;
        GameManager.Instance?.UpdateHealthUI(currentHealth);
    }
    
    public void SetRespawnPoint(Vector3 newRespawnPoint)
    {
        respawnPoint = newRespawnPoint;
    }
    
    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        GameManager.Instance?.UpdateHealthUI(currentHealth);
        AudioManager.Instance?.PlaySound("heal");
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Checkpoint"))
        {
            SetRespawnPoint(other.transform.position);
            AudioManager.Instance?.PlaySound("checkpoint");
        }
    }
}