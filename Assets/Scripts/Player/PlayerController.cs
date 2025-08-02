using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float jumpForce = 16f;
    [SerializeField] private float doubleJumpForce = 12f;
    [SerializeField] private float acceleration = 50f;
    [SerializeField] private float deceleration = 50f;
    [SerializeField] private float airControl = 0.5f;
    
    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;
    
    [Header("Mobile Settings")]
    [SerializeField] private float swipeThreshold = 50f;
    [SerializeField] private float tapThreshold = 0.3f;
    
    [Header("Audio")]
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip landSound;
    
    // Components
    private Rigidbody2D rb;
    private Animator animator;
    private AudioSource audioSource;
    private SpriteRenderer spriteRenderer;
    
    // Movement state
    private bool isGrounded;
    private bool canDoubleJump = false;
    private bool hasDoubleJumped = false;
    private float horizontalInput = 0f;
    private bool facingRight = true;
    
    // Mobile input
    private Vector2 touchStart;
    private float lastTapTime;
    private bool isTouching = false;
    
    // Animation parameters
    private const string IS_RUNNING = "IsRunning";
    private const string IS_JUMPING = "IsJumping";
    private const string IS_FALLING = "IsFalling";
    
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (groundCheck == null)
        {
            groundCheck = transform.Find("GroundCheck");
            if (groundCheck == null)
            {
                GameObject check = new GameObject("GroundCheck");
                check.transform.SetParent(transform);
                check.transform.localPosition = new Vector3(0, -0.5f, 0);
                groundCheck = check.transform;
            }
        }
    }
    
    void Update()
    {
        HandleMobileInput();
        CheckGrounded();
        UpdateAnimations();
    }
    
    void FixedUpdate()
    {
        HandleMovement();
    }
    
    void HandleMobileInput()
    {
        // Handle touch input
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    touchStart = touch.position;
                    isTouching = true;
                    break;
                    
                case TouchPhase.Moved:
                    if (isTouching)
                    {
                        Vector2 swipeDelta = touch.position - touchStart;
                        
                        // Horizontal movement
                        if (Mathf.Abs(swipeDelta.x) > swipeThreshold)
                        {
                            horizontalInput = Mathf.Sign(swipeDelta.x);
                        }
                        
                        // Jump on swipe up
                        if (swipeDelta.y > swipeThreshold && isGrounded)
                        {
                            Jump();
                        }
                    }
                    break;
                    
                case TouchPhase.Ended:
                    if (isTouching)
                    {
                        // Check for tap to jump
                        float tapTime = Time.time - lastTapTime;
                        if (tapTime < tapThreshold && isGrounded)
                        {
                            Jump();
                        }
                        else if (tapTime < tapThreshold && canDoubleJump && !hasDoubleJumped)
                        {
                            DoubleJump();
                        }
                        
                        lastTapTime = Time.time;
                        horizontalInput = 0f;
                        isTouching = false;
                    }
                    break;
            }
        }
        
        // Keyboard input for testing
        #if UNITY_EDITOR
        horizontalInput = Input.GetAxisRaw("Horizontal");
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isGrounded)
                Jump();
            else if (canDoubleJump && !hasDoubleJumped)
                DoubleJump();
        }
        #endif
    }
    
    void HandleMovement()
    {
        float targetVelocityX = horizontalInput * moveSpeed;
        float currentVelocityX = rb.velocity.x;
        
        // Apply acceleration/deceleration
        float accelerationRate = (Mathf.Abs(targetVelocityX) > 0.1f) ? acceleration : deceleration;
        float newVelocityX = Mathf.MoveTowards(currentVelocityX, targetVelocityX, accelerationRate * Time.fixedDeltaTime);
        
        // Reduce air control when not grounded
        if (!isGrounded)
        {
            newVelocityX = Mathf.Lerp(currentVelocityX, targetVelocityX, airControl);
        }
        
        rb.velocity = new Vector2(newVelocityX, rb.velocity.y);
        
        // Flip character based on movement direction
        if (horizontalInput > 0 && !facingRight)
        {
            Flip();
        }
        else if (horizontalInput < 0 && facingRight)
        {
            Flip();
        }
    }
    
    void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        hasDoubleJumped = false;
        
        if (audioSource && jumpSound)
        {
            audioSource.PlayOneShot(jumpSound);
        }
        
        // Enable double jump if power-up is active
        if (canDoubleJump)
        {
            StartCoroutine(DoubleJumpCooldown());
        }
    }
    
    void DoubleJump()
    {
        rb.velocity = new Vector2(rb.velocity.x, doubleJumpForce);
        hasDoubleJumped = true;
        
        if (audioSource && jumpSound)
        {
            audioSource.PlayOneShot(jumpSound);
        }
    }
    
    IEnumerator DoubleJumpCooldown()
    {
        yield return new WaitForSeconds(0.1f);
        hasDoubleJumped = false;
    }
    
    void CheckGrounded()
    {
        bool wasGrounded = isGrounded;
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        
        // Play land sound when touching ground
        if (isGrounded && !wasGrounded && audioSource && landSound)
        {
            audioSource.PlayOneShot(landSound);
        }
        
        // Reset double jump when grounded
        if (isGrounded)
        {
            hasDoubleJumped = false;
        }
    }
    
    void UpdateAnimations()
    {
        if (animator)
        {
            animator.SetBool(IS_RUNNING, Mathf.Abs(horizontalInput) > 0.1f && isGrounded);
            animator.SetBool(IS_JUMPING, !isGrounded && rb.velocity.y > 0);
            animator.SetBool(IS_FALLING, !isGrounded && rb.velocity.y < 0);
        }
    }
    
    void Flip()
    {
        facingRight = !facingRight;
        spriteRenderer.flipX = !facingRight;
    }
    
    // Public methods for power-ups
    public void EnableDoubleJump()
    {
        canDoubleJump = true;
    }
    
    public void DisableDoubleJump()
    {
        canDoubleJump = false;
        hasDoubleJumped = false;
    }
    
    public bool IsGrounded()
    {
        return isGrounded;
    }
    
    public bool IsMoving()
    {
        return Mathf.Abs(horizontalInput) > 0.1f;
    }
    
    // Gizmos for debugging
    void OnDrawGizmosSelected()
    {
        if (groundCheck)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}