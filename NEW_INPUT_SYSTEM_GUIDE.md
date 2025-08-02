# New Input System & TextMeshPro Integration Guide

This guide covers the integration of Unity's new Input System and TextMeshPro throughout the 2D mobile platformer game.

## 🎮 New Input System Setup

### Overview
The new Input System provides better cross-platform support, enhanced touch handling, and more flexible input binding compared to the legacy Input Manager.

### 1. Project Setup

#### Enable New Input System
1. Go to **Edit > Project Settings > Player**
2. Under **Other Settings**, find **Active Input Handling**
3. Set to **"Both"** (for compatibility) or **"Input System Package (New)"**
4. Click **"Yes"** when prompted to restart Unity

#### Install Input System Package
1. Go to **Window > Package Manager**
2. Search for **"Input System"**
3. Install the latest version (1.4.4 or newer)
4. Restart Unity when prompted

### 2. Input Actions Asset

#### Create Input Actions
1. Right-click in Project window
2. Select **Create > Input Actions**
3. Name it **"PlayerInputActions"**
4. Double-click to open the Input Actions editor

#### Configure Player Actions
1. **Add Action Map**: Click **"+"** and name it **"Player"**
2. **Add Actions**:
   - **Move** (Value, Vector2)
   - **Jump** (Button)
   - **Pause** (Button)

#### Configure UI Actions
1. **Add Action Map**: Click **"+"** and name it **"UI"**
2. **Add Actions**:
   - **Navigate** (Pass Through, Vector2)
   - **Submit** (Button)
   - **Cancel** (Button)
   - **Point** (Pass Through, Vector2)
   - **Click** (Pass Through, Button)

#### Set Up Bindings

##### Player Actions
- **Move**: WASD, Arrow Keys, Gamepad Left Stick
- **Jump**: Space, Gamepad South Button
- **Pause**: Escape, Gamepad Start Button

##### UI Actions
- **Navigate**: Arrow Keys, WASD, Gamepad Left Stick
- **Submit**: Enter, Gamepad South Button
- **Cancel**: Escape, Gamepad East Button
- **Point**: Mouse Position, Touch Position
- **Click**: Mouse Left Button, Touch Press

### 3. Generate C# Class
1. In Input Actions editor, click **"Generate C# Class"**
2. Enable **"Generate C# Class"**
3. Set **"C# Class File"** to `Assets/Scripts/Input/PlayerInputActions.cs`
4. Click **"Apply"**

## 📱 Player Input Handler

### Overview
The `PlayerInputHandler` script manages all player input using the new Input System, including touch controls for mobile devices.

### Setup Instructions

#### 1. Add to Player
1. Select the Player GameObject
2. Add the `PlayerInputHandler` script
3. Add a `PlayerInput` component
4. Assign the **PlayerInputActions** asset to the PlayerInput component

#### 2. Configure Settings
```csharp
[Header("Input Settings")]
[SerializeField] private float swipeThreshold = 50f;
[SerializeField] private float tapThreshold = 0.3f;
[SerializeField] private float doubleTapThreshold = 0.5f;

[Header("Touch Settings")]
[SerializeField] private bool enableTouchInput = true;
[SerializeField] private bool enableKeyboardInput = true;
```

#### 3. Input Events
The handler provides these events:
- `OnMoveInput(Vector2)`: Movement input
- `OnJumpPressed()`: Jump button pressed
- `OnJumpReleased()`: Jump button released
- `OnDoubleJumpPressed()`: Double jump detected
- `OnPausePressed()`: Pause button pressed
- `OnSwipeDetected(Vector2)`: Swipe gesture detected
- `OnTapDetected()`: Tap gesture detected

### Touch Controls

#### Swipe Detection
- **Horizontal Swipe**: Move left/right
- **Vertical Swipe Up**: Jump
- **Tap**: Jump (if grounded)
- **Double Tap**: Double jump (if power-up active)

#### Configuration
```csharp
// Adjust sensitivity
inputHandler.SetSwipeThreshold(50f);
inputHandler.SetTapThreshold(0.3f);
inputHandler.SetDoubleTapThreshold(0.5f);

// Enable/disable input types
inputHandler.SetTouchInputEnabled(true);
inputHandler.SetKeyboardInputEnabled(true);
```

## 🖥️ UI Input Handler

### Overview
The `UIInputHandler` script manages UI navigation and interaction using the new Input System.

### Setup Instructions

#### 1. Add to UI Manager
1. Select the UI Manager GameObject
2. Add the `UIInputHandler` script
3. Add a `PlayerInput` component
4. Assign the **PlayerInputActions** asset

#### 2. Configure UI Input Module
1. Find the EventSystem in your scene
2. Replace `StandaloneInputModule` with `InputSystemUIInputModule`
3. Assign the **PlayerInputActions** asset to the UI Input Module

#### 3. UI Navigation
```csharp
// Navigate UI elements
uiInputHandler.SelectNextUIElement();
uiInputHandler.SelectPreviousUIElement();
uiInputHandler.SelectLeftUIElement();
uiInputHandler.SelectRightUIElement();

// Click selected element
uiInputHandler.ClickSelectedUIElement();

// Check pointer over UI
bool isOverUI = uiInputHandler.IsPointerOverUI();
```

## 📝 TextMeshPro Integration

### Overview
TextMeshPro provides superior text rendering with better quality, performance, and features compared to legacy UI Text.

### 1. Project Setup

#### Install TextMeshPro
1. Go to **Window > Package Manager**
2. Search for **"TextMeshPro"**
3. Install the latest version (3.0.6 or newer)
4. Import TMP Essentials when prompted

#### Import TMP Essentials
1. Go to **Window > TextMeshPro > Import TMP Essential Resources**
2. Click **"Import"** to add default fonts and materials

### 2. Replace Legacy Text

#### UI Text Replacement
1. Select all UI Text components
2. Right-click and choose **"Convert to TextMeshPro"**
3. Adjust font size and settings as needed

#### World Space Text
1. Create 3D Text: **GameObject > 3D Object > Text - TextMeshPro**
2. Create UI Text: **GameObject > UI > Text - TextMeshPro**

### 3. TextMeshPro Features

#### Rich Text
```csharp
// Bold text
textMesh.text = "<b>Bold Text</b>";

// Colored text
textMesh.text = "<color=red>Red Text</color>";

// Size variation
textMesh.text = "<size=24>Large Text</size>";

// Multiple effects
textMesh.text = "<b><color=yellow><size=32>Important!</size></color></b>";
```

#### Dynamic Font Assets
```csharp
// Load font asset
TMP_FontAsset fontAsset = Resources.Load<TMP_FontAsset>("Fonts/MyFont");
textMesh.font = fontAsset;

// Create font asset at runtime
TMP_FontAsset runtimeFont = TMP_FontAsset.CreateFontAsset(font);
```

#### Text Effects
```csharp
// Enable text effects
textMesh.enableAutoSizing = true;
textMesh.fontSizeMin = 12f;
textMesh.fontSizeMax = 48f;

// Text animation
textMesh.text = "Animated Text";
textMesh.ForceMeshUpdate();
```

### 4. TextMeshPro in Scripts

#### Update All Scripts
Replace all `Text` components with `TextMeshProUGUI`:

```csharp
// Old way
using UnityEngine.UI;
public Text scoreText;

// New way
using TMPro;
public TextMeshProUGUI scoreText;
```

#### Text Updates
```csharp
// Set text
scoreText.text = "Score: " + score.ToString("N0");

// Rich text
scoreText.text = $"<color=yellow>Score:</color> <b>{score:N0}</b>";

// Animated text
scoreText.text = "Level Complete!";
scoreText.transform.DOScale(1.2f, 0.3f).SetEase(Ease.OutBack);
```

## 🔧 Integration with Existing Systems

### 1. Update PlayerController

#### Remove Legacy Input
```csharp
// Remove these lines
// horizontalInput = Input.GetAxisRaw("Horizontal");
// if (Input.GetKeyDown(KeyCode.Space)) { ... }
```

#### Add New Input Handler
```csharp
private PlayerInputHandler inputHandler;

void Start()
{
    inputHandler = GetComponent<PlayerInputHandler>();
    inputHandler.OnMoveInput += OnMoveInput;
    inputHandler.OnJumpPressed += OnJumpPressed;
    inputHandler.OnDoubleJumpPressed += OnDoubleJumpPressed;
}

void OnMoveInput(Vector2 moveInput)
{
    horizontalInput = moveInput.x;
}

void OnJumpPressed()
{
    if (isGrounded)
        Jump();
    else if (canDoubleJump && !hasDoubleJumped)
        DoubleJump();
}
```

### 2. Update UIManager

#### Replace Text Components
```csharp
// Old
public Text scoreText;
public Text livesText;

// New
public TextMeshProUGUI scoreText;
public TextMeshProUGUI livesText;
```

#### Add UI Input Handler
```csharp
private UIInputHandler uiInputHandler;

void Start()
{
    uiInputHandler = GetComponent<UIInputHandler>();
    uiInputHandler.OnSubmitPressed += OnSubmitPressed;
    uiInputHandler.OnCancelPressed += OnCancelPressed;
}
```

### 3. Update GameManager

#### Pause Input
```csharp
private PlayerInputHandler inputHandler;

void Start()
{
    inputHandler = FindObjectOfType<PlayerInputHandler>();
    if (inputHandler != null)
    {
        inputHandler.OnPausePressed += TogglePause;
    }
}
```

## 📱 Mobile Optimization

### 1. Touch Input Optimization

#### Enhanced Touch Support
```csharp
void Awake()
{
    // Enable enhanced touch support
    EnhancedTouchSupport.Enable();
}
```

#### Touch Sensitivity
```csharp
// Adjust for mobile devices
inputHandler.SetSwipeThreshold(75f); // Larger threshold for mobile
inputHandler.SetTapThreshold(0.4f);  // Longer tap time
```

### 2. UI Touch Optimization

#### Touch-Friendly UI
```csharp
// Larger buttons for mobile
Button button = GetComponent<Button>();
RectTransform rect = button.GetComponent<RectTransform>();
rect.sizeDelta = new Vector2(100f, 100f); // Minimum 44x44 points
```

#### Touch Feedback
```csharp
// Visual feedback for touch
button.onClick.AddListener(() => {
    StartCoroutine(AnimateButtonPress(button));
});

IEnumerator AnimateButtonPress(Button button)
{
    Vector3 originalScale = button.transform.localScale;
    button.transform.localScale = originalScale * 0.9f;
    yield return new WaitForSeconds(0.1f);
    button.transform.localScale = originalScale;
}
```

## 🎨 Visual Enhancements

### 1. TextMeshPro Styling

#### Font Assets
1. Create custom font assets for different styles
2. Use different fonts for titles, body text, and UI elements
3. Implement font fallbacks for missing characters

#### Text Effects
```csharp
// Gradient text
textMesh.text = "<gradient=\"red_to_blue\">Gradient Text</gradient>";

// Animated text
textMesh.text = "<wiggle>Wiggly Text</wiggle>";

// Custom effects
textMesh.text = "<rainbow>Rainbow Text</rainbow>";
```

### 2. Input Visual Feedback

#### Button Animations
```csharp
// Scale animation on press
public void OnButtonPressed(Button button)
{
    button.transform.DOScale(0.95f, 0.1f).SetEase(Ease.OutQuad);
}

public void OnButtonReleased(Button button)
{
    button.transform.DOScale(1f, 0.1f).SetEase(Ease.OutBack);
}
```

#### Touch Indicators
```csharp
// Show touch position
public void OnTouchMoved(Vector2 position)
{
    touchIndicator.transform.position = position;
    touchIndicator.SetActive(true);
}
```

## 🧪 Testing

### 1. Input Testing

#### Editor Testing
- Test keyboard input in Play mode
- Verify gamepad input with connected controller
- Test touch input using Unity Remote or device

#### Mobile Testing
- Build and test on actual device
- Verify touch sensitivity and responsiveness
- Test UI navigation with touch

### 2. TextMeshPro Testing

#### Font Rendering
- Test with different font sizes
- Verify rich text rendering
- Check performance with large text volumes

#### Cross-Platform Testing
- Test on different devices and resolutions
- Verify text scaling and positioning
- Check font fallbacks

## 🔄 Migration Checklist

### Input System Migration
- [ ] Enable new Input System in Project Settings
- [ ] Install Input System package
- [ ] Create Input Actions asset
- [ ] Generate C# class
- [ ] Add PlayerInputHandler to Player
- [ ] Add UIInputHandler to UI Manager
- [ ] Update EventSystem with InputSystemUIInputModule
- [ ] Remove legacy input code
- [ ] Test all input methods

### TextMeshPro Migration
- [ ] Install TextMeshPro package
- [ ] Import TMP Essential Resources
- [ ] Convert all UI Text to TextMeshPro
- [ ] Update script references
- [ ] Configure font assets
- [ ] Test text rendering
- [ ] Optimize for mobile

### Integration Testing
- [ ] Test player movement with new input
- [ ] Test UI navigation
- [ ] Test touch controls on mobile
- [ ] Verify text rendering quality
- [ ] Check performance impact
- [ ] Test cross-platform compatibility

## 🚀 Performance Tips

### Input System
- Use `InputAction.Enable()` and `Disable()` to control input
- Subscribe/unsubscribe from events properly
- Use `InputAction.ReadValue()` for continuous input
- Optimize touch detection for mobile

### TextMeshPro
- Use font atlases for better performance
- Limit rich text usage for large text volumes
- Use object pooling for dynamic text
- Optimize font asset sizes

This integration provides a modern, cross-platform input system with superior text rendering, making the game more responsive and visually appealing across all devices.