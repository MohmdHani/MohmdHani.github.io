# Advanced Features Setup Guide

This guide covers the advanced features added to the 2D mobile platformer game: Level Lock/Unlock System, World Map, Checkpoints/Endpoints, and Multi-Character Shop.

## 🎯 Level Lock/Unlock System

### Overview
The level progression system manages which levels and worlds are unlocked based on player progress and star collection.

### Setup Instructions

#### 1. Add LevelProgressManager
1. Create an empty GameObject named "LevelProgressManager"
2. Add the `LevelProgressManager` script
3. Configure settings:
   - **Total Worlds**: 5 (default)
   - **Levels Per World**: 10 (default)
   - **Stars Required to Unlock World**: 15 (default)
   - **Auto Save**: true

#### 2. Configure Worlds
In the LevelProgressManager inspector:
1. Set up each world with:
   - **World Name**: "World 1", "World 2", etc.
   - **World Icon**: Assign sprite for world representation
   - **World Color**: Set unique color for each world
   - **Required Stars**: Stars needed from previous world

#### 3. Level Progression Logic
The system automatically:
- Unlocks levels sequentially within a world
- Unlocks next world when current world is completed
- Requires minimum stars to unlock new worlds
- Saves progress automatically

### Usage
```csharp
// Check if level is unlocked
bool isUnlocked = LevelProgressManager.Instance.IsLevelUnlocked(levelNumber);

// Complete a level
LevelProgressManager.Instance.CompleteLevel(levelNumber, stars, time, score, coins, allCoins, allEnemies);

// Get level data
LevelData levelData = LevelProgressManager.Instance.GetLevelData(levelNumber);
```

## 🗺️ World Map System

### Overview
The world map provides a visual interface for navigating between worlds and selecting levels.

### Setup Instructions

#### 1. Create World Map UI
1. Create a Canvas for the world map
2. Add the `WorldMapManager` script to a GameObject
3. Create UI prefabs:
   - **World Button Prefab**: Button with world icon, name, progress
   - **Level Button Prefab**: Button with level number, stars, completion status

#### 2. UI Hierarchy
```
WorldMapCanvas/
├── WorldMapPanel/
│   ├── WorldContainer (Grid Layout Group)
│   ├── TotalStarsText
│   ├── NavigationButtons
│   └── BackButton
├── LevelSelectionPanel/
│   ├── LevelContainer (Grid Layout Group)
│   ├── WorldTitleText
│   ├── WorldProgressText
│   └── NavigationButtons
└── LevelInfoPanel/
    ├── CharacterImage
    ├── LevelStatsText
    ├── PlayButton
    └── BackButton
```

#### 3. Configure World Map Manager
1. Assign UI references in the inspector
2. Set up visual effects (unlock effects, sounds)
3. Configure navigation buttons

#### 4. World Button Prefab Structure
```
WorldButton/
├── Background (Image)
├── WorldIcon (Image)
├── WorldName (TextMeshPro)
├── ProgressText (TextMeshPro)
├── StarsContainer/
│   ├── Star1 (Image)
│   ├── Star2 (Image)
│   └── Star3 (Image)
└── LockIcon (Image)
```

#### 5. Level Button Prefab Structure
```
LevelButton/
├── Background (Image)
├── LevelNumber (TextMeshPro)
├── StarsContainer/
│   ├── Star1 (Image)
│   ├── Star2 (Image)
│   └── Star3 (Image)
├── CompletionIcon (Image)
├── LockIcon (Image)
└── TimeText (TextMeshPro)
```

### Features
- **World Navigation**: Swipe between worlds
- **Level Selection**: Grid layout of level buttons
- **Progress Display**: Shows completion status and stars
- **Visual Feedback**: Unlock effects and animations
- **Level Info**: Detailed stats for each level

## 🏁 Checkpoint and Endpoint System

### Overview
The checkpoint system allows players to respawn at specific points and tracks level completion.

### Setup Instructions

#### 1. Add CheckpointSystem
1. Create an empty GameObject named "CheckpointSystem"
2. Add the `CheckpointSystem` script
3. Configure settings:
   - **Auto Activate First**: true
   - **Respawn Delay**: 1 second
   - **Activated Color**: Green
   - **Inactive Color**: Gray

#### 2. Create Checkpoints
1. Create empty GameObjects for each checkpoint
2. Position them strategically in your level
3. Add them to the CheckpointSystem's checkpoint list
4. Configure each checkpoint:
   - **Checkpoint Transform**: Assign the GameObject
   - **Is End Point**: Set to true for the final checkpoint
   - **Activation Effect**: Optional particle effect
   - **Activation Sound**: Optional audio clip

#### 3. Checkpoint Visual Setup
The system automatically:
- Adds SpriteRenderer if not present
- Sets up trigger colliders
- Applies visual states (locked/unlocked)
- Handles player detection

#### 4. Endpoint Configuration
1. Set the final checkpoint as "Is End Point"
2. The endpoint triggers level completion
3. Can have special visual effects (yellow color)

### Usage
```csharp
// Activate checkpoint
checkpointSystem.ActivateCheckpoint(checkpointIndex);

// Respawn player
checkpointSystem.RespawnPlayer();

// Check if endpoint reached
if (checkpointSystem.HasEndPoint())
{
    Checkpoint endPoint = checkpointSystem.GetEndPoint();
}
```

### Visual Gizmos
- Green circles: Regular checkpoints
- Yellow circles: Endpoints
- Blue lines: Checkpoint connections
- Numbers: Checkpoint order

## 🛍️ Multi-Character Shop System

### Overview
The character shop allows players to unlock and select different characters with unique stats and abilities.

### Setup Instructions

#### 1. Create Character Shop
1. Create an empty GameObject named "CharacterShop"
2. Add the `CharacterShop` script
3. Configure shop settings and UI references

#### 2. Character Configuration
For each character, set up:
- **Character Name**: "Hero", "Ninja", "Robot", etc.
- **Character Description**: Brief description of abilities
- **Character Sprite**: Visual representation
- **Character Prefab**: Optional prefab for instantiation
- **Unlock Cost**: Coins required to unlock
- **Is Default**: Set to true for starting character
- **Character Stats**: Movement, health, abilities
- **Character Color**: Tint color for the character

#### 3. Character Stats Configuration
Configure each character's stats:
- **Move Speed**: Horizontal movement speed
- **Jump Force**: Initial jump velocity
- **Double Jump Force**: Second jump velocity
- **Max Health**: Maximum health points
- **Acceleration**: How quickly speed changes
- **Deceleration**: How quickly speed decreases
- **Air Control**: Movement control while airborne

#### 4. Shop UI Setup
```
ShopPanel/
├── CharacterContainer (Grid Layout Group)
├── CharacterDisplay/
│   ├── SelectedCharacterImage
│   ├── CharacterNameText
│   ├── CharacterDescriptionText
│   ├── CharacterStatsText
│   └── UnlockCostText
├── CurrencyDisplay/
│   ├── CoinsText
│   └── GemsText
├── ActionButtons/
│   ├── BuyButton
│   ├── SelectButton
│   └── CloseButton
└── Overlays/
    ├── LockedOverlay
    └── SelectedOverlay
```

#### 5. Character Button Prefab
```
CharacterButton/
├── Background (Image)
├── CharacterImage (Image)
├── NameText (TextMeshPro)
├── CostText (TextMeshPro)
├── LockIcon (Image)
└── SelectedIcon (Image)
```

### Character Examples

#### Default Character (Hero)
- **Name**: "Hero"
- **Cost**: 0 (unlocked by default)
- **Stats**: Balanced movement and health
- **Color**: Blue

#### Fast Character (Ninja)
- **Name**: "Ninja"
- **Cost**: 500 coins
- **Stats**: High speed, low health
- **Color**: Black

#### Strong Character (Robot)
- **Name**: "Robot"
- **Cost**: 1000 coins
- **Stats**: Low speed, high health
- **Color**: Red

#### Special Character (Mage)
- **Name**: "Mage"
- **Cost**: 1500 coins
- **Stats**: Medium speed, special abilities
- **Color**: Purple

### Usage
```csharp
// Show shop
characterShop.ShowShop();

// Buy character
characterShop.BuySelectedCharacter();

// Select character
characterShop.SelectCurrentCharacter();

// Get selected character
Character selected = characterShop.GetSelectedCharacter();
```

## 🔧 Integration with Existing Systems

### 1. Update GameManager
The GameManager now integrates with:
- LevelProgressManager for level completion
- CharacterShop for character selection
- CheckpointSystem for respawning

### 2. Update LevelManager
The LevelManager works with:
- CheckpointSystem for checkpoint tracking
- LevelProgressManager for progress saving
- World map navigation

### 3. Update UIManager
The UIManager includes:
- World map navigation buttons
- Shop access button
- Character selection UI

## 🎮 Game Flow

### 1. Main Menu
- Play button → World Map
- Shop button → Character Shop
- Settings button → Settings Menu

### 2. World Map
- Select world → Level selection
- Select level → Level info
- Play level → Game scene

### 3. In-Game
- Reach checkpoint → Save progress
- Reach endpoint → Level complete
- Die → Respawn at checkpoint

### 4. Level Complete
- Show stars earned
- Unlock next level/world
- Return to world map

## 💾 Save System

### Progress Data Saved
- **Level Progress**: Unlocked levels, completion status, stars
- **World Progress**: Unlocked worlds, completion status
- **Character Progress**: Unlocked characters, selected character
- **Checkpoint Progress**: Current checkpoint position
- **Game Progress**: Score, coins, lives

### Save Keys
- `LevelProgress`: Level and world data
- `Character_0_Unlocked`: Character unlock status
- `SelectedCharacter`: Currently selected character
- `CurrentCheckpoint`: Last checkpoint reached

## 🎨 Visual Polish

### 1. Unlock Effects
- Particle effects for level/world unlocks
- Sound effects for achievements
- Screen shake for important events

### 2. UI Animations
- Button press animations
- Panel transitions
- Progress bar animations

### 3. Character Visuals
- Unique sprites for each character
- Color variations
- Special effects for abilities

## 🚀 Performance Optimization

### 1. UI Optimization
- Object pooling for buttons
- Efficient sprite rendering
- Minimal draw calls

### 2. Save Optimization
- Compressed save data
- Incremental saves
- Background saving

### 3. Memory Management
- Proper cleanup of UI elements
- Efficient data structures
- Minimal allocations

## 🧪 Testing

### 1. Level Progression
- Test level unlocking logic
- Verify star requirements
- Check world progression

### 2. Checkpoint System
- Test checkpoint activation
- Verify respawn functionality
- Check endpoint detection

### 3. Character Shop
- Test character unlocking
- Verify stat application
- Check save/load functionality

### 4. World Map
- Test navigation between worlds
- Verify level selection
- Check progress display

## 🔄 Future Enhancements

### 1. Additional Features
- Achievement system
- Daily challenges
- Leaderboards
- Multiplayer support

### 2. Character Abilities
- Special moves
- Power-ups
- Unique mechanics

### 3. Level Editor
- Custom level creation
- Level sharing
- Community content

This advanced feature set provides a complete progression system that keeps players engaged with unlockable content, clear goals, and meaningful rewards.