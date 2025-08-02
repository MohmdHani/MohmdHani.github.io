using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIInputHandler : MonoBehaviour
{
    [Header("UI Input Settings")]
    [SerializeField] private bool enableUINavigation = true;
    [SerializeField] private bool enableTouchUI = true;
    [SerializeField] private float uiTouchThreshold = 10f;
    
    // Input Actions
    private PlayerInput playerInput;
    private InputAction navigateAction;
    private InputAction submitAction;
    private InputAction cancelAction;
    private InputAction pointAction;
    private InputAction clickAction;
    
    // UI Components
    private InputSystemUIInputModule uiInputModule;
    private EventSystem eventSystem;
    
    // Input state
    private Vector2 navigationInput = Vector2.zero;
    private bool submitPressed = false;
    private bool cancelPressed = false;
    private bool clickPressed = false;
    private Vector2 pointerPosition = Vector2.zero;
    
    // Events
    public System.Action<Vector2> OnNavigateInput;
    public System.Action OnSubmitPressed;
    public System.Action OnCancelPressed;
    public System.Action<Vector2> OnPointerMoved;
    public System.Action OnClickPressed;
    public System.Action OnClickReleased;
    
    void Awake()
    {
        InitializeUIInputSystem();
    }
    
    void Start()
    {
        SetupUIInputActions();
    }
    
    void OnEnable()
    {
        EnableUIInput();
    }
    
    void OnDisable()
    {
        DisableUIInput();
    }
    
    void InitializeUIInputSystem()
    {
        // Get or create PlayerInput component
        playerInput = GetComponent<PlayerInput>();
        if (playerInput == null)
        {
            playerInput = gameObject.AddComponent<PlayerInput>();
        }
        
        // Get or create InputSystemUIInputModule
        uiInputModule = FindObjectOfType<InputSystemUIInputModule>();
        if (uiInputModule == null)
        {
            // Create UI Input Module if it doesn't exist
            var eventSystem = FindObjectOfType<EventSystem>();
            if (eventSystem == null)
            {
                var eventSystemGO = new GameObject("EventSystem");
                eventSystem = eventSystemGO.AddComponent<EventSystem>();
                eventSystemGO.AddComponent<StandaloneInputModule>();
            }
            
            // Replace StandaloneInputModule with InputSystemUIInputModule
            var standaloneModule = eventSystem.GetComponent<StandaloneInputModule>();
            if (standaloneModule != null)
            {
                DestroyImmediate(standaloneModule);
            }
            
            uiInputModule = eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
        }
        
        // Get EventSystem
        eventSystem = FindObjectOfType<EventSystem>();
    }
    
    void SetupUIInputActions()
    {
        // Get input actions from PlayerInput
        if (playerInput != null && playerInput.actions != null)
        {
            navigateAction = playerInput.actions["Navigate"];
            submitAction = playerInput.actions["Submit"];
            cancelAction = playerInput.actions["Cancel"];
            pointAction = playerInput.actions["Point"];
            clickAction = playerInput.actions["Click"];
            
            // Subscribe to input events
            if (navigateAction != null)
            {
                navigateAction.performed += OnNavigatePerformed;
                navigateAction.canceled += OnNavigateCanceled;
            }
            
            if (submitAction != null)
            {
                submitAction.performed += OnSubmitPerformed;
                submitAction.canceled += OnSubmitCanceled;
            }
            
            if (cancelAction != null)
            {
                cancelAction.performed += OnCancelPerformed;
                cancelAction.canceled += OnCancelCanceled;
            }
            
            if (pointAction != null)
            {
                pointAction.performed += OnPointPerformed;
            }
            
            if (clickAction != null)
            {
                clickAction.performed += OnClickPerformed;
                clickAction.canceled += OnClickCanceled;
            }
        }
    }
    
    void EnableUIInput()
    {
        if (navigateAction != null) navigateAction.Enable();
        if (submitAction != null) submitAction.Enable();
        if (cancelAction != null) cancelAction.Enable();
        if (pointAction != null) pointAction.Enable();
        if (clickAction != null) clickAction.Enable();
    }
    
    void DisableUIInput()
    {
        if (navigateAction != null) navigateAction.Disable();
        if (submitAction != null) submitAction.Disable();
        if (cancelAction != null) cancelAction.Disable();
        if (pointAction != null) pointAction.Disable();
        if (clickAction != null) clickAction.Disable();
    }
    
    void Update()
    {
        ProcessUIInput();
    }
    
    void ProcessUIInput()
    {
        // Process navigation input
        if (navigationInput != Vector2.zero)
        {
            OnNavigateInput?.Invoke(navigationInput);
        }
        
        // Process submit input
        if (submitPressed)
        {
            OnSubmitPressed?.Invoke();
            submitPressed = false;
        }
        
        // Process cancel input
        if (cancelPressed)
        {
            OnCancelPressed?.Invoke();
            cancelPressed = false;
        }
        
        // Process click input
        if (clickPressed)
        {
            OnClickPressed?.Invoke();
        }
    }
    
    // Input Action callbacks
    void OnNavigatePerformed(InputAction.CallbackContext context)
    {
        if (!enableUINavigation) return;
        
        navigationInput = context.ReadValue<Vector2>();
        OnNavigateInput?.Invoke(navigationInput);
    }
    
    void OnNavigateCanceled(InputAction.CallbackContext context)
    {
        if (!enableUINavigation) return;
        
        navigationInput = Vector2.zero;
        OnNavigateInput?.Invoke(navigationInput);
    }
    
    void OnSubmitPerformed(InputAction.CallbackContext context)
    {
        submitPressed = true;
        OnSubmitPressed?.Invoke();
    }
    
    void OnSubmitCanceled(InputAction.CallbackContext context)
    {
        submitPressed = false;
    }
    
    void OnCancelPerformed(InputAction.CallbackContext context)
    {
        cancelPressed = true;
        OnCancelPressed?.Invoke();
    }
    
    void OnCancelCanceled(InputAction.CallbackContext context)
    {
        cancelPressed = false;
    }
    
    void OnPointPerformed(InputAction.CallbackContext context)
    {
        if (!enableTouchUI) return;
        
        pointerPosition = context.ReadValue<Vector2>();
        OnPointerMoved?.Invoke(pointerPosition);
    }
    
    void OnClickPerformed(InputAction.CallbackContext context)
    {
        if (!enableTouchUI) return;
        
        clickPressed = true;
        OnClickPressed?.Invoke();
    }
    
    void OnClickCanceled(InputAction.CallbackContext context)
    {
        if (!enableTouchUI) return;
        
        clickPressed = false;
        OnClickReleased?.Invoke();
    }
    
    // Public methods for UI navigation
    public void SetSelectedGameObject(GameObject gameObject)
    {
        if (eventSystem != null)
        {
            eventSystem.SetSelectedGameObject(gameObject);
        }
    }
    
    public GameObject GetSelectedGameObject()
    {
        if (eventSystem != null)
        {
            return eventSystem.currentSelectedGameObject;
        }
        return null;
    }
    
    public void SelectNextUIElement()
    {
        if (eventSystem != null && eventSystem.currentSelectedGameObject != null)
        {
            var selectable = eventSystem.currentSelectedGameObject.GetComponent<Selectable>();
            if (selectable != null)
            {
                var nextSelectable = selectable.FindSelectableOnDown();
                if (nextSelectable != null)
                {
                    eventSystem.SetSelectedGameObject(nextSelectable.gameObject);
                }
            }
        }
    }
    
    public void SelectPreviousUIElement()
    {
        if (eventSystem != null && eventSystem.currentSelectedGameObject != null)
        {
            var selectable = eventSystem.currentSelectedGameObject.GetComponent<Selectable>();
            if (selectable != null)
            {
                var previousSelectable = selectable.FindSelectableOnUp();
                if (previousSelectable != null)
                {
                    eventSystem.SetSelectedGameObject(previousSelectable.gameObject);
                }
            }
        }
    }
    
    public void SelectLeftUIElement()
    {
        if (eventSystem != null && eventSystem.currentSelectedGameObject != null)
        {
            var selectable = eventSystem.currentSelectedGameObject.GetComponent<Selectable>();
            if (selectable != null)
            {
                var leftSelectable = selectable.FindSelectableOnLeft();
                if (leftSelectable != null)
                {
                    eventSystem.SetSelectedGameObject(leftSelectable.gameObject);
                }
            }
        }
    }
    
    public void SelectRightUIElement()
    {
        if (eventSystem != null && eventSystem.currentSelectedGameObject != null)
        {
            var selectable = eventSystem.currentSelectedGameObject.GetComponent<Selectable>();
            if (selectable != null)
            {
                var rightSelectable = selectable.FindSelectableOnRight();
                if (rightSelectable != null)
                {
                    eventSystem.SetSelectedGameObject(rightSelectable.gameObject);
                }
            }
        }
    }
    
    public void ClickSelectedUIElement()
    {
        if (eventSystem != null && eventSystem.currentSelectedGameObject != null)
        {
            var button = eventSystem.currentSelectedGameObject.GetComponent<Button>();
            if (button != null && button.interactable)
            {
                button.onClick.Invoke();
            }
        }
    }
    
    public Vector2 GetPointerPosition()
    {
        return pointerPosition;
    }
    
    public bool IsPointerOverUI()
    {
        if (eventSystem != null)
        {
            return eventSystem.IsPointerOverGameObject();
        }
        return false;
    }
    
    public void SetUINavigationEnabled(bool enabled)
    {
        enableUINavigation = enabled;
    }
    
    public void SetTouchUIEnabled(bool enabled)
    {
        enableTouchUI = enabled;
    }
    
    public void SetUITouchThreshold(float threshold)
    {
        uiTouchThreshold = threshold;
    }
    
    void OnDestroy()
    {
        // Unsubscribe from input events
        if (navigateAction != null)
        {
            navigateAction.performed -= OnNavigatePerformed;
            navigateAction.canceled -= OnNavigateCanceled;
        }
        
        if (submitAction != null)
        {
            submitAction.performed -= OnSubmitPerformed;
            submitAction.canceled -= OnSubmitCanceled;
        }
        
        if (cancelAction != null)
        {
            cancelAction.performed -= OnCancelPerformed;
            cancelAction.canceled -= OnCancelCanceled;
        }
        
        if (pointAction != null)
        {
            pointAction.performed -= OnPointPerformed;
        }
        
        if (clickAction != null)
        {
            clickAction.performed -= OnClickPerformed;
            clickAction.canceled -= OnClickCanceled;
        }
    }
}