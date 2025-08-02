using UnityEngine;
using System.Collections;

public class EnemyController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float patrolDistance = 5f;
    [SerializeField] private float detectionRange = 8f;
    [SerializeField] private float attackRange = 1.5f;
    
    [Header("Combat Settings")]
    [SerializeField] private int damage = 1;
    [SerializeField] private float attackCooldown = 1f;
    [SerializeField] private float stunDuration = 0.5f;
    
    [Header("AI Settings")]
    [SerializeField] private bool canPatrol = true;
    [SerializeField] private bool canChase = true;
    [SerializeField] private bool canAttack = true;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask groundLayer;
    
    [Header("Audio")]
    [SerializeField] private AudioClip attackSound;
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private AudioClip detectSound;
    
    // Components
    private Rigidbody2D rb;
    private Animator animator;
    private AudioSource audioSource;
    private SpriteRenderer spriteRenderer;
    private Collider2D enemyCollider;
    
    // AI State
    private enum EnemyState
    {
        Patrolling,
        Chasing,
        Attacking,
        Stunned,
        Dead
    }
    
    private EnemyState currentState = EnemyState.Patrolling;
    
    // Movement variables
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private bool facingRight = true;
    private float lastAttackTime;
    private bool isStunned = false;
    
    // Player detection
    private Transform player;
    private bool playerDetected = false;
    private float distanceToPlayer;
    
    // Patrol variables
    private float patrolTimer = 0f;
    private float patrolWaitTime = 2f;
    private bool isWaiting = false;
    
    // Animation parameters
    private const string IS_MOVING = "IsMoving";
    private const string IS_ATTACKING = "IsAttacking";
    private const string IS_STUNNED = "IsStunned";
    private const string IS_DEAD = "IsDead";
    
    void Start()
    {
        InitializeComponents();
        SetupPatrol();
    }
    
    void InitializeComponents()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        enemyCollider = GetComponent<Collider2D>();
        
        // Find player
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        
        // Set starting position
        startPosition = transform.position;
        targetPosition = startPosition + Vector3.right * patrolDistance;
    }
    
    void Update()
    {
        if (currentState == EnemyState.Dead) return;
        
        UpdatePlayerDetection();
        UpdateState();
        UpdateAnimations();
    }
    
    void FixedUpdate()
    {
        if (currentState == EnemyState.Dead) return;
        
        HandleMovement();
    }
    
    void UpdatePlayerDetection()
    {
        if (player == null) return;
        
        distanceToPlayer = Vector2.Distance(transform.position, player.position);
        bool wasDetected = playerDetected;
        playerDetected = distanceToPlayer <= detectionRange;
        
        // Play detection sound
        if (playerDetected && !wasDetected && audioSource && detectSound)
        {
            audioSource.PlayOneShot(detectSound);
        }
    }
    
    void UpdateState()
    {
        if (isStunned)
        {
            currentState = EnemyState.Stunned;
            return;
        }
        
        if (playerDetected && canChase)
        {
            if (distanceToPlayer <= attackRange && canAttack)
            {
                currentState = EnemyState.Attacking;
            }
            else
            {
                currentState = EnemyState.Chasing;
            }
        }
        else if (canPatrol)
        {
            currentState = EnemyState.Patrolling;
        }
    }
    
    void HandleMovement()
    {
        switch (currentState)
        {
            case EnemyState.Patrolling:
                HandlePatrolMovement();
                break;
                
            case EnemyState.Chasing:
                HandleChaseMovement();
                break;
                
            case EnemyState.Attacking:
                HandleAttack();
                break;
                
            case EnemyState.Stunned:
                // No movement when stunned
                rb.velocity = new Vector2(0, rb.velocity.y);
                break;
        }
    }
    
    void HandlePatrolMovement()
    {
        if (isWaiting)
        {
            patrolTimer += Time.fixedDeltaTime;
            if (patrolTimer >= patrolWaitTime)
            {
                isWaiting = false;
                patrolTimer = 0f;
                FlipDirection();
            }
            return;
        }
        
        // Move towards target position
        Vector3 direction = (targetPosition - transform.position).normalized;
        rb.velocity = new Vector2(direction.x * moveSpeed, rb.velocity.y);
        
        // Check if reached target
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            isWaiting = true;
            patrolTimer = 0f;
        }
        
        // Check for ground edge
        CheckGroundEdge();
    }
    
    void HandleChaseMovement()
    {
        if (player == null) return;
        
        Vector3 direction = (player.position - transform.position).normalized;
        rb.velocity = new Vector2(direction.x * moveSpeed * 1.5f, rb.velocity.y);
        
        // Flip based on player position
        if (direction.x > 0 && !facingRight)
        {
            Flip();
        }
        else if (direction.x < 0 && facingRight)
        {
            Flip();
        }
        
        // Check for ground edge
        CheckGroundEdge();
    }
    
    void HandleAttack()
    {
        if (Time.time - lastAttackTime < attackCooldown) return;
        
        // Stop movement
        rb.velocity = new Vector2(0, rb.velocity.y);
        
        // Perform attack
        lastAttackTime = Time.time;
        
        if (animator)
        {
            animator.SetTrigger(IS_ATTACKING);
        }
        
        if (audioSource && attackSound)
        {
            audioSource.PlayOneShot(attackSound);
        }
        
        // Check if player is in attack range
        if (player != null && distanceToPlayer <= attackRange)
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth)
            {
                playerHealth.TakeDamage(damage);
            }
        }
    }
    
    void CheckGroundEdge()
    {
        // Cast ray to check for ground edge
        Vector2 rayOrigin = transform.position;
        rayOrigin.y -= 0.5f;
        
        Vector2 rayDirection = facingRight ? Vector2.right : Vector2.left;
        rayDirection.x *= 0.5f;
        
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, rayDirection, 1f, groundLayer);
        
        if (!hit.collider)
        {
            // No ground ahead, flip direction
            Flip();
        }
    }
    
    void SetupPatrol()
    {
        if (!canPatrol) return;
        
        // Set initial target
        targetPosition = startPosition + Vector3.right * patrolDistance;
    }
    
    void FlipDirection()
    {
        if (facingRight)
        {
            targetPosition = startPosition - Vector3.right * patrolDistance;
        }
        else
        {
            targetPosition = startPosition + Vector3.right * patrolDistance;
        }
        Flip();
    }
    
    void Flip()
    {
        facingRight = !facingRight;
        spriteRenderer.flipX = !facingRight;
    }
    
    void UpdateAnimations()
    {
        if (animator == null) return;
        
        bool isMoving = Mathf.Abs(rb.velocity.x) > 0.1f;
        
        animator.SetBool(IS_MOVING, isMoving);
        animator.SetBool(IS_STUNNED, currentState == EnemyState.Stunned);
        animator.SetBool(IS_DEAD, currentState == EnemyState.Dead);
    }
    
    public void TakeDamage(int damage)
    {
        if (currentState == EnemyState.Dead) return;
        
        // Apply stun
        StartCoroutine(ApplyStun());
        
        // Check for death (you can add health system here)
        // For now, enemies die in one hit
        Die();
    }
    
    IEnumerator ApplyStun()
    {
        isStunned = true;
        currentState = EnemyState.Stunned;
        
        yield return new WaitForSeconds(stunDuration);
        
        isStunned = false;
    }
    
    void Die()
    {
        currentState = EnemyState.Dead;
        
        // Disable collider
        if (enemyCollider)
        {
            enemyCollider.enabled = false;
        }
        
        // Stop movement
        rb.velocity = Vector2.zero;
        
        // Play death sound
        if (audioSource && deathSound)
        {
            audioSource.PlayOneShot(deathSound);
        }
        
        // Add score
        if (GameManager.Instance)
        {
            GameManager.Instance.AddScore(100);
        }
        
        // Destroy after delay
        StartCoroutine(DestroyAfterDelay(1f));
    }
    
    IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        // Check if player touched enemy
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth)
            {
                playerHealth.TakeDamage(damage);
            }
        }
    }
    
    void OnDrawGizmosSelected()
    {
        // Draw patrol range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(startPosition, Vector3.one);
        Gizmos.DrawWireCube(startPosition + Vector3.right * patrolDistance, Vector3.one);
        Gizmos.DrawWireCube(startPosition - Vector3.right * patrolDistance, Vector3.one);
        
        // Draw detection range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // Draw attack range
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}