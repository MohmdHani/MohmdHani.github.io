using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 12f;
    public float doubleJumpForce = 10f;
    
    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;
    
    [Header("Mobile Controls")]
    public float touchSensitivity = 0.5f;
    
    private Rigidbody2D rb;
    private Animator animator;
    private bool isGrounded;
    private bool canDoubleJump = true;
    private bool facingRight = true;
    private float horizontalInput;
    
    // Mobile touch variables
    private bool isTouchingLeft = false;
    private bool isTouchingRight = false;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }
    
    void Update()
    {
        CheckGrounded();
        HandleInput();
        HandleJump();
        UpdateAnimations();
    }
    
    void FixedUpdate()
    {
        Move();
    }
    
    void CheckGrounded()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        
        if (isGrounded)
        {
            canDoubleJump = true;
        }
    }
    
    void HandleInput()
    {
        // Keyboard input for PC
        horizontalInput = Input.GetAxisRaw("Horizontal");
        
        // Mobile touch input
        if (Input.touchCount > 0)
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);
                Vector2 touchPosition = Camera.main.ScreenToViewportPoint(touch.position);
                
                if (touch.phase == TouchPhase.Began)
                {
                    if (touchPosition.x < touchSensitivity)
                    {
                        isTouchingLeft = true;
                    }
                    else if (touchPosition.x > (1f - touchSensitivity))
                    {
                        isTouchingRight = true;
                    }
                    else
                    {
                        // Center tap for jump
                        Jump();
                    }
                }
                else if (touch.phase == TouchPhase.Ended)
                {
                    if (touchPosition.x < touchSensitivity)
                    {
                        isTouchingLeft = false;
                    }
                    else if (touchPosition.x > (1f - touchSensitivity))
                    {
                        isTouchingRight = false;
                    }
                }
            }
        }
        
        // Apply mobile input
        if (isTouchingLeft) horizontalInput = -1f;
        else if (isTouchingRight) horizontalInput = 1f;
    }
    
    void HandleJump()
    {
        // Keyboard jump
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Jump();
        }
    }
    
    void Jump()
    {
        if (isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            AudioManager.Instance?.PlaySound("jump");
        }
        else if (canDoubleJump)
        {
            rb.velocity = new Vector2(rb.velocity.x, doubleJumpForce);
            canDoubleJump = false;
            AudioManager.Instance?.PlaySound("double_jump");
        }
    }
    
    void Move()
    {
        rb.velocity = new Vector2(horizontalInput * moveSpeed, rb.velocity.y);
        
        // Flip character based on direction
        if (horizontalInput > 0 && !facingRight)
        {
            Flip();
        }
        else if (horizontalInput < 0 && facingRight)
        {
            Flip();
        }
    }
    
    void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }
    
    void UpdateAnimations()
    {
        if (animator != null)
        {
            animator.SetBool("IsGrounded", isGrounded);
            animator.SetFloat("Speed", Mathf.Abs(horizontalInput));
            animator.SetFloat("VerticalSpeed", rb.velocity.y);
        }
    }
    
    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}