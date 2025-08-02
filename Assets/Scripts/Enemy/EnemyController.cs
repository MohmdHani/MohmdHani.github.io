using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 2f;
    public float patrolDistance = 3f;
    public float detectionRange = 5f;
    public float attackRange = 1f;
    
    [Header("Combat")]
    public int damage = 1;
    public float attackCooldown = 1f;
    public float knockbackForce = 3f;
    
    [Header("Ground Check")]
    public Transform groundCheck;
    public Transform wallCheck;
    public LayerMask groundLayer;
    
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Vector2 startPosition;
    private Vector2 targetPosition;
    private bool movingRight = true;
    private bool isAttacking = false;
    private float lastAttackTime;
    private Transform player;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        startPosition = transform.position;
        targetPosition = startPosition + Vector2.right * patrolDistance;
        
        // Find player
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }
    
    void Update()
    {
        if (player != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);
            
            if (distanceToPlayer <= attackRange && Time.time - lastAttackTime > attackCooldown)
            {
                Attack();
            }
            else if (distanceToPlayer <= detectionRange)
            {
                ChasePlayer();
            }
            else
            {
                Patrol();
            }
        }
        else
        {
            Patrol();
        }
        
        UpdateAnimations();
    }
    
    void Patrol()
    {
        // Move towards target position
        Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
        rb.velocity = new Vector2(direction.x * moveSpeed, rb.velocity.y);
        
        // Check if reached target or hit wall/edge
        if (Vector2.Distance(transform.position, targetPosition) < 0.1f || 
            !IsGrounded() || IsWallAhead())
        {
            FlipDirection();
        }
    }
    
    void ChasePlayer()
    {
        if (player == null) return;
        
        Vector2 direction = (player.position - transform.position).normalized;
        rb.velocity = new Vector2(direction.x * moveSpeed * 1.5f, rb.velocity.y);
        
        // Flip based on player direction
        if (direction.x > 0 && !movingRight)
        {
            FlipDirection();
        }
        else if (direction.x < 0 && movingRight)
        {
            FlipDirection();
        }
    }
    
    void Attack()
    {
        if (isAttacking) return;
        
        isAttacking = true;
        lastAttackTime = Time.time;
        
        // Stop movement
        rb.velocity = Vector2.zero;
        
        // Play attack animation
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }
        
        // Attack logic
        if (player != null)
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage, transform.position);
            }
        }
        
        AudioManager.Instance?.PlaySound("enemy_attack");
        
        // Reset attack state
        Invoke(nameof(ResetAttack), 0.5f);
    }
    
    void ResetAttack()
    {
        isAttacking = false;
    }
    
    void FlipDirection()
    {
        movingRight = !movingRight;
        
        // Flip sprite
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = !movingRight;
        }
        
        // Update target position
        if (movingRight)
        {
            targetPosition = startPosition + Vector2.right * patrolDistance;
        }
        else
        {
            targetPosition = startPosition - Vector2.right * patrolDistance;
        }
    }
    
    bool IsGrounded()
    {
        if (groundCheck == null) return true;
        return Physics2D.OverlapCircle(groundCheck.position, 0.1f, groundLayer);
    }
    
    bool IsWallAhead()
    {
        if (wallCheck == null) return false;
        return Physics2D.OverlapCircle(wallCheck.position, 0.1f, groundLayer);
    }
    
    void UpdateAnimations()
    {
        if (animator != null)
        {
            animator.SetFloat("Speed", Mathf.Abs(rb.velocity.x));
            animator.SetBool("IsAttacking", isAttacking);
        }
    }
    
    void OnDrawGizmosSelected()
    {
        // Draw patrol area
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(startPosition, new Vector3(patrolDistance * 2, 1, 1));
        
        // Draw detection range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // Draw attack range
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        // Draw ground and wall checks
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, 0.1f);
        }
        
        if (wallCheck != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(wallCheck.position, 0.1f);
        }
    }
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Apply knockback to player
            Rigidbody2D playerRb = collision.gameObject.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                Vector2 knockbackDirection = (collision.transform.position - transform.position).normalized;
                playerRb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
            }
        }
    }
}