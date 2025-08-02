# 2D Mobile Platformer Game

A complete 2D mobile platformer game built in Unity with touch controls, physics-based movement, and mobile-optimized features.

## Features

- **Touch Controls**: Swipe to move, tap to jump
- **Physics-Based Movement**: Smooth character movement with acceleration and deceleration
- **Mobile Optimized**: Touch-friendly UI and responsive design
- **Level System**: Multiple levels with increasing difficulty
- **Collectibles**: Coins and power-ups to collect
- **Enemies**: Basic AI enemies that patrol platforms
- **Mobile UI**: Pause menu, level selection, and settings
- **Sound System**: Background music and sound effects

## Project Structure

```
Assets/
├── Scripts/
│   ├── Player/
│   │   ├── PlayerController.cs
│   │   ├── PlayerHealth.cs
│   │   └── PlayerAnimation.cs
│   ├── Game/
│   │   ├── GameManager.cs
│   │   ├── LevelManager.cs
│   │   └── UIManager.cs
│   ├── Enemies/
│   │   ├── EnemyController.cs
│   │   └── EnemyPatrol.cs
│   ├── Collectibles/
│   │   ├── Coin.cs
│   │   └── PowerUp.cs
│   └── Utils/
│       ├── TouchInput.cs
│       └── CameraFollow.cs
├── Prefabs/
├── Scenes/
└── Materials/
```

## Setup Instructions

1. Create a new Unity 2D project
2. Import the scripts into your Assets/Scripts folder
3. Set up the scene hierarchy as described in the documentation
4. Configure the mobile build settings
5. Test on device or emulator

## Controls

- **Swipe Left/Right**: Move character
- **Tap**: Jump
- **Double Tap**: Double jump (if power-up is active)
- **Swipe Up**: Special ability (if available)

## Mobile Build Settings

- Target Platform: Android/iOS
- Graphics API: OpenGL ES 3.0
- Scripting Backend: IL2CPP
- Target Architectures: ARM64

## Performance Optimization

- Object pooling for frequently spawned objects
- Efficient sprite rendering
- Optimized physics calculations
- Mobile-friendly texture compression