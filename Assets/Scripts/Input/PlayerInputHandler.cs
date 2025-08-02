using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using System.Collections.Generic;

public class PlayerInputHandler : MonoBehaviour
{
    [Header("Input Settings")]
    [SerializeField] private float swipeThreshold = 50f;
    [SerializeField] private float tapThreshold = 0.3f;
    [SerializeField] private float doubleTapThreshold = 0.5f;
    
    [Header("Touch Settings")]
    [SerializeField] private bool enableTouchInput = true;
    [SerializeField] private bool enableKeyboardInput = true;
    
    // Input Actions
    private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction pauseAction;
    
    // Touch tracking
    private Vector2 touchStartPosition;
    private float lastTapTime;
    private int tapCount = 0;
    private bool isTouching = false;
    
    // Input state
    private Vector2 moveInput = Vector2.zero;
    private bool jumpPressed = false;
    private bool pausePressed = false;
    
    // Events
    public System.Action<Vector2> OnMoveInput;
    public System.Action OnJumpPressed;
    public System.Action OnJumpReleased;
    public System.Action OnDoubleJumpPressed;
    public System.Action OnPausePressed;
    public System.Action<Vector2> OnSwipeDetected;
    public System.Action OnTapDetected;
    
    void Awake()
    {
        InitializeInputSystem();
    }
    
    void Start()
    {
        SetupInputActions();
    }
    
    void OnEnable()
    {
        EnableInput();
    }
    
    void OnDisable()
    {
        DisableInput();
    }
    
    void InitializeInputSystem()
    {
        // Get or create PlayerInput component
        playerInput = GetComponent<PlayerInput>();
        if (playerInput == null)
        {
            playerInput = gameObject.AddComponent<PlayerInput>();
        }
        
        // Enable enhanced touch support
        if (enableTouchInput)
        {
            EnhancedTouchSupport.Enable();
        }
    }
    
    void SetupInputActions()
    {
        // Get input actions from PlayerInput
        if (playerInput != null && playerInput.actions != null)
        {
            moveAction = playerInput.actions["Move"];
            jumpAction = playerInput.actions["Jump"];
            pauseAction = playerInput.actions["Pause"];
            
            // Subscribe to input events
            if (moveAction != null)
            {
                moveAction.performed += OnMovePerformed;
                moveAction.canceled += OnMoveCanceled;
            }
            
            if (jumpAction != null)
            {
                jumpAction.performed += OnJumpPerformed;
                jumpAction.canceled += OnJumpCanceled;
            }
            
            if (pauseAction != null)
            {
                pauseAction.performed += OnPausePerformed;
            }
        }
    }
    
    void EnableInput()
    {
        if (moveAction != null) moveAction.Enable();
        if (jumpAction != null) jumpAction.Enable();
        if (pauseAction != null) pauseAction.Enable();
    }
    
    void DisableInput()
    {
        if (moveAction != null) moveAction.Disable();
        if (jumpAction != null) jumpAction.Disable();
        if (pauseAction != null) pauseAction.Disable();
    }
    
    void Update()
    {
        HandleTouchInput();
        ProcessInput();
    }
    
    void HandleTouchInput()
    {
        if (!enableTouchInput) return;
        
        // Handle touch input using new Input System
        if (Touchscreen.current != null)
        {
            var primaryTouch = Touchscreen.current.primaryTouch;
            
            if (primaryTouch.press.wasPressedThisFrame)
            {
                OnTouchStarted(primaryTouch.position.ReadValue());
            }
            else if (primaryTouch.press.isPressed)
            {
                OnTouchMoved(primaryTouch.position.ReadValue());
            }
            else if (primaryTouch.press.wasReleasedThisFrame)
            {
                OnTouchEnded(primaryTouch.position.ReadValue());
            }
        }
    }
    
    void OnTouchStarted(Vector2 position)
    {
        touchStartPosition = position;
        isTouching = true;
        lastTapTime = Time.time;
    }
    
    void OnTouchMoved(Vector2 position)
    {
        if (!isTouching) return;
        
        Vector2 delta = position - touchStartPosition;
        
        // Detect swipe for movement
        if (Mathf.Abs(delta.x) > swipeThreshold)
        {
            float moveDirection = Mathf.Sign(delta.x);
            moveInput = new Vector2(moveDirection, 0);
            OnSwipeDetected?.Invoke(delta);
        }
        
        // Detect swipe up for jump
        if (delta.y > swipeThreshold)
        {
            jumpPressed = true;
            OnJumpPressed?.Invoke();
        }
    }
    
    void OnTouchEnded(Vector2 position)
    {
        if (!isTouching) return;
        
        isTouching = false;
        moveInput = Vector2.zero;
        jumpPressed = false;
        
        // Detect tap
        Vector2 delta = position - touchStartPosition;
        if (delta.magnitude < swipeThreshold)
        {
            float timeSinceLastTap = Time.time - lastTapTime;
            
            if (timeSinceLastTap < tapThreshold)
            {
                tapCount++;
                
                if (tapCount == 1)
                {
                    // Single tap
                    OnTapDetected?.Invoke();
                    OnJumpPressed?.Invoke();
                }
                else if (tapCount == 2 && timeSinceLastTap < doubleTapThreshold)
                {
                    // Double tap
                    OnDoubleJumpPressed?.Invoke();
                    tapCount = 0;
                }
            }
            else
            {
                tapCount = 1;
            }
            
            lastTapTime = Time.time;
        }
        else
        {
            tapCount = 0;
        }
    }
    
    void ProcessInput()
    {
        // Process move input
        if (moveInput != Vector2.zero)
        {
            OnMoveInput?.Invoke(moveInput);
        }
        
        // Process jump input
        if (jumpPressed)
        {
            OnJumpPressed?.Invoke();
        }
        
        // Process pause input
        if (pausePressed)
        {
            OnPausePressed?.Invoke();
            pausePressed = false;
        }
    }
    
    // Input Action callbacks
    void OnMovePerformed(InputAction.CallbackContext context)
    {
        if (!enableKeyboardInput) return;
        
        moveInput = context.ReadValue<Vector2>();
        OnMoveInput?.Invoke(moveInput);
    }
    
    void OnMoveCanceled(InputAction.CallbackContext context)
    {
        if (!enableKeyboardInput) return;
        
        moveInput = Vector2.zero;
        OnMoveInput?.Invoke(moveInput);
    }
    
    void OnJumpPerformed(InputAction.CallbackContext context)
    {
        if (!enableKeyboardInput) return;
        
        jumpPressed = true;
        OnJumpPressed?.Invoke();
    }
    
    void OnJumpCanceled(InputAction.CallbackContext context)
    {
        if (!enableKeyboardInput) return;
        
        jumpPressed = false;
        OnJumpReleased?.Invoke();
    }
    
    void OnPausePerformed(InputAction.CallbackContext context)
    {
        pausePressed = true;
        OnPausePressed?.Invoke();
    }
    
    // Public methods for external control
    public Vector2 GetMoveInput()
    {
        return moveInput;
    }
    
    public bool IsJumpPressed()
    {
        return jumpPressed;
    }
    
    public bool IsTouching()
    {
        return isTouching;
    }
    
    public void SetTouchInputEnabled(bool enabled)
    {
        enableTouchInput = enabled;
    }
    
    public void SetKeyboardInputEnabled(bool enabled)
    {
        enableKeyboardInput = enabled;
    }
    
    public void SetSwipeThreshold(float threshold)
    {
        swipeThreshold = threshold;
    }
    
    public void SetTapThreshold(float threshold)
    {
        tapThreshold = threshold;
    }
    
    public void SetDoubleTapThreshold(float threshold)
    {
        doubleTapThreshold = threshold;
    }
    
    void OnDestroy()
    {
        // Unsubscribe from input events
        if (moveAction != null)
        {
            moveAction.performed -= OnMovePerformed;
            moveAction.canceled -= OnMoveCanceled;
        }
        
        if (jumpAction != null)
        {
            jumpAction.performed -= OnJumpPerformed;
            jumpAction.canceled -= OnJumpCanceled;
        }
        
        if (pauseAction != null)
        {
            pauseAction.performed -= OnPausePerformed;
        }
    }
}