using UnityEngine;

public class Coin : MonoBehaviour
{
    [Header("Coin Settings")]
    public int coinValue = 10;
    public float rotationSpeed = 90f;
    public float bobSpeed = 2f;
    public float bobHeight = 0.5f;
    
    [Header("Effects")]
    public GameObject collectEffect;
    public AudioClip collectSound;
    
    private Vector3 startPosition;
    private SpriteRenderer spriteRenderer;
    private bool isCollected = false;
    
    void Start()
    {
        startPosition = transform.position;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    
    void Update()
    {
        if (!isCollected)
        {
            // Rotate the coin
            transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
            
            // Bob up and down
            float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isCollected)
        {
            Collect();
        }
    }
    
    void Collect()
    {
        isCollected = true;
        
        // Add score
        GameManager.Instance?.AddScore(coinValue);
        
        // Play sound
        if (collectSound != null)
        {
            AudioManager.Instance?.PlaySound("coin_collect");
        }
        
        // Spawn collect effect
        if (collectEffect != null)
        {
            Instantiate(collectEffect, transform.position, Quaternion.identity);
        }
        
        // Hide the coin
        spriteRenderer.enabled = false;
        
        // Destroy after effect
        Destroy(gameObject, 0.5f);
    }
    
    void OnDrawGizmosSelected()
    {
        // Draw bob range
        Gizmos.color = Color.yellow;
        Vector3 topPos = transform.position + Vector3.up * bobHeight;
        Vector3 bottomPos = transform.position - Vector3.up * bobHeight;
        Gizmos.DrawLine(topPos, bottomPos);
    }
}