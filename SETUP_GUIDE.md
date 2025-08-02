# 2D Mobile Platformer Game - Setup Guide

This guide will help you set up and configure the 2D mobile platformer game in Unity.

## Prerequisites

- Unity 2022.3 LTS or newer
- Basic knowledge of Unity Editor
- Mobile device or emulator for testing

## Project Setup

### 1. Create New Unity Project

1. Open Unity Hub
2. Click "New Project"
3. Select "2D Core" template
4. Name your project "MobilePlatformer"
5. Choose location and click "Create"

### 2. Import Scripts

1. Create the following folder structure in your Assets folder:
   ```
   Assets/
   ├── Scripts/
   │   ├── Player/
   │   ├── Game/
   │   ├── Enemies/
   │   ├── Collectibles/
   │   └── Utils/
   ├── Prefabs/
   ├── Scenes/
   ├── Materials/
   ├── Audio/
   └── Sprites/
   ```

2. Copy all the provided scripts into their respective folders

### 3. Configure Project Settings

#### Physics 2D Settings
1. Go to Edit > Project Settings > Physics 2D
2. Set Default Material to "Bouncy" or create a custom physics material
3. Configure collision matrix for your layers

#### Input Settings
1. Go to Edit > Project Settings > Input Manager
2. Ensure the following axes are configured:
   - Horizontal (for testing)
   - Jump (Space key for testing)

#### Quality Settings
1. Go to Edit > Project Settings > Quality
2. Set quality levels appropriate for mobile devices
3. Disable anti-aliasing for better performance

### 4. Set Up Layers

Create the following layers:
- Player (Layer 8)
- Ground (Layer 9)
- Enemy (Layer 10)
- Coin (Layer 11)
- PowerUp (Layer 12)
- Hazard (Layer 13)

## Scene Setup

### 1. Create Main Scene

1. Create a new scene called "Level1"
2. Save it in Assets/Scenes/

### 2. Set Up Camera

1. Select the Main Camera
2. Add the `CameraFollow` script
3. Configure settings:
   - Follow Speed: 5
   - Offset: (0, 2, -10)
   - Enable Bounds: true
   - Min X: -20, Max X: 20
   - Min Y: -10, Max Y: 10

### 3. Create Player

1. Create an empty GameObject named "Player"
2. Add the following components:
   - Sprite Renderer
   - Rigidbody 2D (set to Dynamic)
   - Box Collider 2D
   - PlayerController script
   - PlayerHealth script
   - Audio Source

3. Configure PlayerController:
   - Move Speed: 8
   - Jump Force: 16
   - Ground Layer: Ground
   - Swipe Threshold: 50

4. Configure PlayerHealth:
   - Max Health: 3
   - Invincibility Duration: 2
   - Damage Layers: Enemy, Hazard

5. Set the Player tag to "Player"
6. Set the Player layer to "Player"

### 4. Create Ground

1. Create a GameObject named "Ground"
2. Add Sprite Renderer and Box Collider 2D
3. Set layer to "Ground"
4. Scale to create platforms
5. Duplicate for multiple platforms

### 5. Create Game Manager

1. Create an empty GameObject named "GameManager"
2. Add the `GameManager` script
3. Configure settings:
   - Current Level: 1
   - Total Levels: 10
   - Player Lives: 3
   - Enable Vibration: true
   - Enable Sound: true

### 6. Create UI Manager

1. Create a Canvas (UI > Canvas)
2. Set Canvas Scaler to "Scale With Screen Size"
3. Reference resolution: 1920x1080
4. Create UI Manager GameObject
5. Add the `UIManager` script
6. Set up UI elements (see UI Setup section)

### 7. Create Level Manager

1. Create an empty GameObject named "LevelManager"
2. Add the `LevelManager` script
3. Configure level settings:
   - Level Number: 1
   - Time Limit: 300
   - Target Score: 1000
   - Use Checkpoints: true

## UI Setup

### 1. HUD Elements

Create the following UI elements under the Canvas:

#### Score Text
- Type: TextMeshPro - Text
- Position: Top-left
- Text: "Score: 0"

#### Lives Text
- Type: TextMeshPro - Text
- Position: Top-left (below score)
- Text: "Lives: 3"

#### Coins Text
- Type: TextMeshPro - Text
- Position: Top-left (below lives)
- Text: "Coins: 0"

#### Level Text
- Type: TextMeshPro - Text
- Position: Top-center
- Text: "Level 1"

#### Time Text
- Type: TextMeshPro - Text
- Position: Top-right
- Text: "00:00"

### 2. Mobile Controls

#### Pause Button
- Type: Button
- Position: Top-right
- Icon: Pause symbol

#### Jump Button
- Type: Button
- Position: Bottom-right
- Size: 100x100
- Icon: Up arrow

#### Left/Right Buttons
- Type: Button
- Position: Bottom-left
- Size: 80x80 each
- Icons: Left/Right arrows

### 3. Menus

#### Pause Menu
- Panel with semi-transparent background
- Resume Button
- Restart Button
- Main Menu Button
- Settings Button

#### Game Over Menu
- Panel with dark background
- "Game Over" text
- Final Score text
- Retry Button
- Main Menu Button

#### Level Complete Menu
- Panel with semi-transparent background
- "Level Complete!" text
- Level Score text
- Level Time text
- Next Level Button
- Main Menu Button

## Mobile Build Configuration

### 1. Build Settings

1. Go to File > Build Settings
2. Switch Platform to Android or iOS
3. Add your scenes to build:
   - MainMenu
   - Level1
   - Level2 (etc.)

### 2. Player Settings

#### Android Settings
1. Go to Edit > Project Settings > Player
2. Set Company Name and Product Name
3. Set Default Orientation to "Portrait"
4. Set Target API Level to "Automatic"
5. Set Scripting Backend to "IL2CPP"
6. Set Target Architectures to "ARM64"

#### iOS Settings
1. Set Bundle Identifier
2. Set Target Device to "iPhone + iPad"
3. Set Scripting Backend to "IL2CPP"
4. Set Target Architectures to "ARM64"

### 3. Quality Settings for Mobile

1. Go to Edit > Project Settings > Quality
2. Set quality levels:
   - Low: For older devices
   - Medium: For mid-range devices
   - High: For newer devices

## Testing

### 1. Editor Testing

1. Press Play in Unity Editor
2. Test with keyboard controls:
   - WASD or Arrow Keys: Move
   - Space: Jump
   - Escape: Pause

### 2. Mobile Testing

1. Build and run on device
2. Test touch controls:
   - Swipe left/right: Move
   - Tap: Jump
   - Double tap: Double jump (if power-up active)

### 3. Performance Testing

1. Use Unity Profiler
2. Monitor frame rate
3. Check memory usage
4. Test on different devices

## Troubleshooting

### Common Issues

1. **Player falls through ground**
   - Check ground layer settings
   - Ensure ground has collider
   - Verify player rigidbody settings

2. **Touch controls not working**
   - Check Input.multiTouchEnabled
   - Verify touch input handling in PlayerController
   - Test on actual device

3. **Performance issues**
   - Reduce particle effects
   - Optimize sprite sizes
   - Use object pooling for frequent spawns

4. **Build errors**
   - Check API compatibility
   - Verify all required assets are included
   - Test with clean build

## Next Steps

1. Create additional levels
2. Add more enemy types
3. Implement power-ups
4. Add sound effects and music
5. Create main menu scene
6. Add level selection
7. Implement save system
8. Add achievements

## Performance Tips

1. Use sprite atlases
2. Implement object pooling
3. Optimize physics calculations
4. Use LOD for complex objects
5. Minimize draw calls
6. Use efficient collision detection
7. Optimize audio compression
8. Test on target devices regularly