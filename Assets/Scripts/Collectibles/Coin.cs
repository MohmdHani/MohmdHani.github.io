using UnityEngine;
using System.Collections;

public class Coin : MonoBehaviour
{
    [Header("Coin Settings")]
    [SerializeField] private int coinValue = 1;
    [SerializeField] private float rotationSpeed = 90f;
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobHeight = 0.2f;
    
    [Header("Collection Effects")]
    [SerializeField] private GameObject collectEffect;
    [SerializeField] private AudioClip collectSound;
    [SerializeField] private float collectEffectDuration = 1f;
    
    [Header("Visual Effects")]
    [SerializeField] private bool enableRotation = true;
    [SerializeField] private bool enableBob = true;
    [SerializeField] private Color glowColor = Color.yellow;
    [SerializeField] private float glowIntensity = 1f;
    
    // Components
    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;
    private Collider2D coinCollider;
    
    // Animation variables
    private Vector3 startPosition;
    private float bobTimer = 0f;
    private bool isCollected = false;
    
    // Visual effects
    private Material originalMaterial;
    private Material glowMaterial;
    
    void Start()
    {
        InitializeComponents();
        SetupVisualEffects();
    }
    
    void InitializeComponents()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        coinCollider = GetComponent<Collider2D>();
        
        // Store starting position for bob animation
        startPosition = transform.position;
        
        // Add audio source if not present
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Configure audio source
        audioSource.playOnAwake = false;
        audioSource.volume = 0.5f;
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
            glowMaterial.SetColor("_EmissionColor", glowColor * glowIntensity);
        }
        
        // Apply glow material
        spriteRenderer.material = glowMaterial;
    }
    
    void Update()
    {
        if (isCollected) return;
        
        HandleRotation();
        HandleBobAnimation();
    }
    
    void HandleRotation()
    {
        if (!enableRotation) return;
        
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
    }
    
    void HandleBobAnimation()
    {
        if (!enableBob) return;
        
        bobTimer += Time.deltaTime * bobSpeed;
        float bobOffset = Mathf.Sin(bobTimer) * bobHeight;
        
        Vector3 newPosition = startPosition;
        newPosition.y += bobOffset;
        transform.position = newPosition;
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (isCollected) return;
        
        if (other.CompareTag("Player"))
        {
            Collect();
        }
    }
    
    void Collect()
    {
        if (isCollected) return;
        
        isCollected = true;
        
        // Disable collider
        if (coinCollider)
        {
            coinCollider.enabled = false;
        }
        
        // Add score and coins
        if (GameManager.Instance)
        {
            GameManager.Instance.AddCoins(coinValue);
        }
        
        // Play collection sound
        if (audioSource && collectSound)
        {
            audioSource.PlayOneShot(collectSound);
        }
        
        // Spawn collection effect
        if (collectEffect)
        {
            GameObject effect = Instantiate(collectEffect, transform.position, Quaternion.identity);
            Destroy(effect, collectEffectDuration);
        }
        
        // Start collection animation
        StartCoroutine(CollectionAnimation());
    }
    
    IEnumerator CollectionAnimation()
    {
        // Scale up and fade out
        float duration = 0.5f;
        float elapsed = 0f;
        
        Vector3 originalScale = transform.localScale;
        Color originalColor = spriteRenderer.color;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            
            // Scale up
            float scale = Mathf.Lerp(1f, 1.5f, progress);
            transform.localScale = originalScale * scale;
            
            // Fade out
            Color newColor = originalColor;
            newColor.a = Mathf.Lerp(1f, 0f, progress);
            spriteRenderer.color = newColor;
            
            yield return null;
        }
        
        // Destroy the coin
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
    public void SetCoinValue(int value)
    {
        coinValue = value;
    }
    
    public int GetCoinValue()
    {
        return coinValue;
    }
    
    public void SetGlowColor(Color color)
    {
        glowColor = color;
        if (glowMaterial != null)
        {
            glowMaterial.SetColor("_EmissionColor", glowColor * glowIntensity);
        }
    }
    
    public void SetGlowIntensity(float intensity)
    {
        glowIntensity = intensity;
        if (glowMaterial != null)
        {
            glowMaterial.SetColor("_EmissionColor", glowColor * glowIntensity);
        }
    }
}