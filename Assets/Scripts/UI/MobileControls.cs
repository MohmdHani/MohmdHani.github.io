using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MobileControls : MonoBehaviour
{
    [Header("Control Areas")]
    public RectTransform leftControlArea;
    public RectTransform rightControlArea;
    public RectTransform jumpButton;
    
    [Header("Visual Feedback")]
    public Image leftArrow;
    public Image rightArrow;
    public Image jumpIcon;
    public Color pressedColor = new Color(1f, 1f, 1f, 0.5f);
    public Color normalColor = new Color(1f, 1f, 1f, 1f);
    
    [Header("Settings")]
    public bool showControls = true;
    public float buttonScale = 1.2f;
    
    private bool isLeftPressed = false;
    private bool isRightPressed = false;
    private bool isJumpPressed = false;
    private PlayerController playerController;
    
    void Start()
    {
        playerController = FindObjectOfType<PlayerController>();
        SetupControlAreas();
        UpdateControlVisibility();
    }
    
    void SetupControlAreas()
    {
        // Setup left control area
        if (leftControlArea != null)
        {
            EventTrigger leftTrigger = leftControlArea.gameObject.GetComponent<EventTrigger>();
            if (leftTrigger == null)
                leftTrigger = leftControlArea.gameObject.AddComponent<EventTrigger>();
            
            AddEventTrigger(leftTrigger, EventTriggerType.PointerDown, (data) => OnLeftPressed());
            AddEventTrigger(leftTrigger, EventTriggerType.PointerUp, (data) => OnLeftReleased());
        }
        
        // Setup right control area
        if (rightControlArea != null)
        {
            EventTrigger rightTrigger = rightControlArea.gameObject.GetComponent<EventTrigger>();
            if (rightTrigger == null)
                rightTrigger = rightControlArea.gameObject.AddComponent<EventTrigger>();
            
            AddEventTrigger(rightTrigger, EventTriggerType.PointerDown, (data) => OnRightPressed());
            AddEventTrigger(rightTrigger, EventTriggerType.PointerUp, (data) => OnRightReleased());
        }
        
        // Setup jump button
        if (jumpButton != null)
        {
            EventTrigger jumpTrigger = jumpButton.gameObject.GetComponent<EventTrigger>();
            if (jumpTrigger == null)
                jumpTrigger = jumpButton.gameObject.AddComponent<EventTrigger>();
            
            AddEventTrigger(jumpTrigger, EventTriggerType.PointerDown, (data) => OnJumpPressed());
            AddEventTrigger(jumpTrigger, EventTriggerType.PointerUp, (data) => OnJumpReleased());
        }
    }
    
    void AddEventTrigger(EventTrigger trigger, EventTriggerType eventType, System.Action<BaseEventData> action)
    {
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = eventType;
        entry.callback.AddListener((data) => action(data));
        trigger.triggers.Add(entry);
    }
    
    void OnLeftPressed()
    {
        isLeftPressed = true;
        UpdateVisualFeedback();
        AudioManager.Instance?.PlaySound("button_click");
    }
    
    void OnLeftReleased()
    {
        isLeftPressed = false;
        UpdateVisualFeedback();
    }
    
    void OnRightPressed()
    {
        isRightPressed = true;
        UpdateVisualFeedback();
        AudioManager.Instance?.PlaySound("button_click");
    }
    
    void OnRightReleased()
    {
        isRightPressed = false;
        UpdateVisualFeedback();
    }
    
    void OnJumpPressed()
    {
        isJumpPressed = true;
        UpdateVisualFeedback();
        if (playerController != null)
        {
            // Trigger jump through reflection or public method
            playerController.SendMessage("Jump", SendMessageOptions.DontRequireReceiver);
        }
        AudioManager.Instance?.PlaySound("button_click");
    }
    
    void OnJumpReleased()
    {
        isJumpPressed = false;
        UpdateVisualFeedback();
    }
    
    void UpdateVisualFeedback()
    {
        // Update left arrow
        if (leftArrow != null)
        {
            leftArrow.color = isLeftPressed ? pressedColor : normalColor;
            leftArrow.transform.localScale = isLeftPressed ? Vector3.one * buttonScale : Vector3.one;
        }
        
        // Update right arrow
        if (rightArrow != null)
        {
            rightArrow.color = isRightPressed ? pressedColor : normalColor;
            rightArrow.transform.localScale = isRightPressed ? Vector3.one * buttonScale : Vector3.one;
        }
        
        // Update jump icon
        if (jumpIcon != null)
        {
            jumpIcon.color = isJumpPressed ? pressedColor : normalColor;
            jumpIcon.transform.localScale = isJumpPressed ? Vector3.one * buttonScale : Vector3.one;
        }
    }
    
    void UpdateControlVisibility()
    {
        bool shouldShow = showControls && Application.isMobilePlatform;
        
        if (leftControlArea != null)
            leftControlArea.gameObject.SetActive(shouldShow);
        
        if (rightControlArea != null)
            rightControlArea.gameObject.SetActive(shouldShow);
        
        if (jumpButton != null)
            jumpButton.gameObject.SetActive(shouldShow);
    }
    
    public bool IsLeftPressed() => isLeftPressed;
    public bool IsRightPressed() => isRightPressed;
    public bool IsJumpPressed() => isJumpPressed;
    
    public void SetShowControls(bool show)
    {
        showControls = show;
        UpdateControlVisibility();
    }
    
    public void ToggleControls()
    {
        SetShowControls(!showControls);
    }
    
    void Update()
    {
        // Handle keyboard input for testing on PC
        if (!Application.isMobilePlatform)
        {
            isLeftPressed = Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow);
            isRightPressed = Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow);
            
            if (Input.GetKeyDown(KeyCode.Space))
            {
                OnJumpPressed();
            }
            else if (Input.GetKeyUp(KeyCode.Space))
            {
                OnJumpReleased();
            }
            
            UpdateVisualFeedback();
        }
    }
    
    void OnApplicationPause(bool pauseStatus)
    {
        // Reset all controls when app is paused
        if (pauseStatus)
        {
            isLeftPressed = false;
            isRightPressed = false;
            isJumpPressed = false;
            UpdateVisualFeedback();
        }
    }
}