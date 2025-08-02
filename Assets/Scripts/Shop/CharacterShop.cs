using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class CharacterShop : MonoBehaviour
{
    [System.Serializable]
    public class Character
    {
        public string characterName;
        public string characterDescription;
        public Sprite characterSprite;
        public GameObject characterPrefab;
        public int unlockCost = 1000;
        public bool isUnlocked = false;
        public bool isDefault = false;
        public CharacterStats stats;
        public Color characterColor = Color.white;
        public AudioClip characterSound;
    }
    
    [System.Serializable]
    public class CharacterStats
    {
        public float moveSpeed = 8f;
        public float jumpForce = 16f;
        public float doubleJumpForce = 12f;
        public int maxHealth = 3;
        public float acceleration = 50f;
        public float deceleration = 50f;
        public float airControl = 0.5f;
    }
    
    [Header("Shop UI")]
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private Transform characterContainer;
    [SerializeField] private GameObject characterButtonPrefab;
    [SerializeField] private Button closeShopButton;
    [SerializeField] private Button buyButton;
    [SerializeField] private Button selectButton;
    
    [Header("Character Display")]
    [SerializeField] private Image selectedCharacterImage;
    [SerializeField] private TextMeshProUGUI characterNameText;
    [SerializeField] private TextMeshProUGUI characterDescriptionText;
    [SerializeField] private TextMeshProUGUI characterStatsText;
    [SerializeField] private TextMeshProUGUI unlockCostText;
    [SerializeField] private GameObject lockedOverlay;
    [SerializeField] private GameObject selectedOverlay;
    
    [Header("Currency Display")]
    [SerializeField] private TextMeshProUGUI coinsText;
    [SerializeField] private TextMeshProUGUI gemsText;
    
    [Header("Character List")]
    [SerializeField] private List<Character> characters = new List<Character>();
    [SerializeField] private int currentCharacterIndex = 0;
    [SerializeField] private int selectedCharacterIndex = 0;
    
    [Header("Audio")]
    [SerializeField] private AudioClip purchaseSound;
    [SerializeField] private AudioClip selectSound;
    [SerializeField] private AudioClip errorSound;
    
    // Components
    private AudioSource audioSource;
    private List<GameObject> characterButtons = new List<GameObject>();
    
    // Events
    public System.Action<Character> OnCharacterUnlocked;
    public System.Action<Character> OnCharacterSelected;
    
    void Start()
    {
        InitializeShop();
        SetupEventListeners();
        LoadCharacterProgress();
        CreateCharacterButtons();
        UpdateDisplay();
    }
    
    void InitializeShop()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Set default character as unlocked
        if (characters.Count > 0)
        {
            Character defaultChar = characters.Find(c => c.isDefault);
            if (defaultChar != null)
            {
                defaultChar.isUnlocked = true;
                selectedCharacterIndex = characters.IndexOf(defaultChar);
            }
            else
            {
                characters[0].isUnlocked = true;
                selectedCharacterIndex = 0;
            }
        }
    }
    
    void SetupEventListeners()
    {
        if (closeShopButton) closeShopButton.onClick.AddListener(CloseShop);
        if (buyButton) buyButton.onClick.AddListener(BuySelectedCharacter);
        if (selectButton) selectButton.onClick.AddListener(SelectCurrentCharacter);
    }
    
    void CreateCharacterButtons()
    {
        // Clear existing buttons
        foreach (GameObject button in characterButtons)
        {
            Destroy(button);
        }
        characterButtons.Clear();
        
        // Create character buttons
        for (int i = 0; i < characters.Count; i++)
        {
            Character character = characters[i];
            GameObject characterButton = Instantiate(characterButtonPrefab, characterContainer);
            
            SetupCharacterButton(characterButton, character, i);
            characterButtons.Add(characterButton);
        }
    }
    
    void SetupCharacterButton(GameObject button, Character character, int index)
    {
        // Get button components
        Button buttonComponent = button.GetComponent<Button>();
        Image characterImage = button.transform.Find("CharacterImage")?.GetComponent<Image>();
        TextMeshProUGUI nameText = button.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
        GameObject lockIcon = button.transform.Find("LockIcon")?.gameObject;
        GameObject selectedIcon = button.transform.Find("SelectedIcon")?.gameObject;
        TextMeshProUGUI costText = button.transform.Find("CostText")?.GetComponent<TextMeshProUGUI>();
        
        // Set character image
        if (characterImage && character.characterSprite)
        {
            characterImage.sprite = character.characterSprite;
            characterImage.color = character.characterColor;
        }
        
        // Set character name
        if (nameText)
        {
            nameText.text = character.characterName;
        }
        
        // Set lock status
        if (lockIcon)
        {
            lockIcon.SetActive(!character.isUnlocked);
        }
        
        // Set selected status
        if (selectedIcon)
        {
            selectedIcon.SetActive(index == selectedCharacterIndex);
        }
        
        // Set cost text
        if (costText)
        {
            costText.text = character.isUnlocked ? "" : $"{character.unlockCost}";
        }
        
        // Setup button click
        if (buttonComponent)
        {
            int buttonIndex = index; // Capture index for lambda
            buttonComponent.onClick.AddListener(() => SelectCharacterInShop(buttonIndex));
        }
    }
    
    public void SelectCharacterInShop(int characterIndex)
    {
        if (characterIndex < 0 || characterIndex >= characters.Count) return;
        
        currentCharacterIndex = characterIndex;
        UpdateDisplay();
        
        // Play select sound
        if (audioSource && selectSound)
        {
            audioSource.PlayOneShot(selectSound);
        }
    }
    
    void UpdateDisplay()
    {
        if (currentCharacterIndex < 0 || currentCharacterIndex >= characters.Count) return;
        
        Character character = characters[currentCharacterIndex];
        
        // Update character image
        if (selectedCharacterImage && character.characterSprite)
        {
            selectedCharacterImage.sprite = character.characterSprite;
            selectedCharacterImage.color = character.characterColor;
        }
        
        // Update character info
        if (characterNameText)
        {
            characterNameText.text = character.characterName;
        }
        
        if (characterDescriptionText)
        {
            characterDescriptionText.text = character.characterDescription;
        }
        
        // Update stats display
        if (characterStatsText && character.stats != null)
        {
            characterStatsText.text = $"Speed: {character.stats.moveSpeed}\n" +
                                     $"Jump: {character.stats.jumpForce}\n" +
                                     $"Health: {character.stats.maxHealth}\n" +
                                     $"Air Control: {character.stats.airControl:F1}";
        }
        
        // Update unlock cost
        if (unlockCostText)
        {
            unlockCostText.text = character.isUnlocked ? "Unlocked!" : $"Cost: {character.unlockCost}";
        }
        
        // Update overlays
        if (lockedOverlay)
        {
            lockedOverlay.SetActive(!character.isUnlocked);
        }
        
        if (selectedOverlay)
        {
            selectedOverlay.SetActive(currentCharacterIndex == selectedCharacterIndex);
        }
        
        // Update buttons
        if (buyButton)
        {
            buyButton.gameObject.SetActive(!character.isUnlocked);
            buyButton.interactable = CanAffordCharacter(character);
        }
        
        if (selectButton)
        {
            selectButton.gameObject.SetActive(character.isUnlocked);
            selectButton.interactable = currentCharacterIndex != selectedCharacterIndex;
        }
        
        // Update currency display
        UpdateCurrencyDisplay();
    }
    
    void UpdateCurrencyDisplay()
    {
        if (GameManager.Instance == null) return;
        
        if (coinsText)
        {
            coinsText.text = $"Coins: {GameManager.Instance.GetCoinsCollected()}";
        }
        
        // You can add gems system here
        if (gemsText)
        {
            gemsText.text = "Gems: 0"; // Placeholder
        }
    }
    
    bool CanAffordCharacter(Character character)
    {
        if (GameManager.Instance == null) return false;
        
        return GameManager.Instance.GetCoinsCollected() >= character.unlockCost;
    }
    
    public void BuySelectedCharacter()
    {
        if (currentCharacterIndex < 0 || currentCharacterIndex >= characters.Count) return;
        
        Character character = characters[currentCharacterIndex];
        
        if (character.isUnlocked)
        {
            PlayErrorSound();
            return;
        }
        
        if (!CanAffordCharacter(character))
        {
            PlayErrorSound();
            return;
        }
        
        // Deduct coins
        if (GameManager.Instance)
        {
            GameManager.Instance.AddCoins(-character.unlockCost);
        }
        
        // Unlock character
        character.isUnlocked = true;
        
        // Play purchase sound
        if (audioSource && purchaseSound)
        {
            audioSource.PlayOneShot(purchaseSound);
        }
        
        // Trigger event
        OnCharacterUnlocked?.Invoke(character);
        
        // Update display
        UpdateDisplay();
        CreateCharacterButtons(); // Refresh buttons
        
        // Save progress
        SaveCharacterProgress();
    }
    
    public void SelectCurrentCharacter()
    {
        if (currentCharacterIndex < 0 || currentCharacterIndex >= characters.Count) return;
        
        Character character = characters[currentCharacterIndex];
        
        if (!character.isUnlocked)
        {
            PlayErrorSound();
            return;
        }
        
        selectedCharacterIndex = currentCharacterIndex;
        
        // Play select sound
        if (audioSource && selectSound)
        {
            audioSource.PlayOneShot(selectSound);
        }
        
        // Trigger event
        OnCharacterSelected?.Invoke(character);
        
        // Update display
        UpdateDisplay();
        CreateCharacterButtons(); // Refresh buttons
        
        // Save progress
        SaveCharacterProgress();
        
        // Apply character to current player
        ApplyCharacterToPlayer(character);
    }
    
    void ApplyCharacterToPlayer(Character character)
    {
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player == null) return;
        
        // Apply character stats
        if (character.stats != null)
        {
            // Update player controller stats
            // Note: You'll need to add public setters to PlayerController for this
            // player.SetMoveSpeed(character.stats.moveSpeed);
            // player.SetJumpForce(character.stats.jumpForce);
            // etc.
        }
        
        // Update player appearance
        SpriteRenderer playerSprite = player.GetComponent<SpriteRenderer>();
        if (playerSprite && character.characterSprite)
        {
            playerSprite.sprite = character.characterSprite;
            playerSprite.color = character.characterColor;
        }
        
        // Update player health
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth && character.stats != null)
        {
            playerHealth.SetMaxHealth(character.stats.maxHealth);
        }
    }
    
    public void ShowShop()
    {
        if (shopPanel) shopPanel.SetActive(true);
        UpdateDisplay();
    }
    
    public void CloseShop()
    {
        if (shopPanel) shopPanel.SetActive(false);
    }
    
    public Character GetSelectedCharacter()
    {
        if (selectedCharacterIndex >= 0 && selectedCharacterIndex < characters.Count)
        {
            return characters[selectedCharacterIndex];
        }
        return null;
    }
    
    public Character GetCharacterByIndex(int index)
    {
        if (index >= 0 && index < characters.Count)
        {
            return characters[index];
        }
        return null;
    }
    
    public List<Character> GetAllCharacters()
    {
        return characters;
    }
    
    public int GetUnlockedCharacterCount()
    {
        return characters.Count(c => c.isUnlocked);
    }
    
    public int GetTotalCharacterCount()
    {
        return characters.Count;
    }
    
    void PlayErrorSound()
    {
        if (audioSource && errorSound)
        {
            audioSource.PlayOneShot(errorSound);
        }
    }
    
    void SaveCharacterProgress()
    {
        // Save unlocked characters
        for (int i = 0; i < characters.Count; i++)
        {
            PlayerPrefs.SetInt($"Character_{i}_Unlocked", characters[i].isUnlocked ? 1 : 0);
        }
        
        // Save selected character
        PlayerPrefs.SetInt("SelectedCharacter", selectedCharacterIndex);
        PlayerPrefs.Save();
    }
    
    void LoadCharacterProgress()
    {
        // Load unlocked characters
        for (int i = 0; i < characters.Count; i++)
        {
            if (PlayerPrefs.HasKey($"Character_{i}_Unlocked"))
            {
                characters[i].isUnlocked = PlayerPrefs.GetInt($"Character_{i}_Unlocked") == 1;
            }
        }
        
        // Load selected character
        if (PlayerPrefs.HasKey("SelectedCharacter"))
        {
            selectedCharacterIndex = PlayerPrefs.GetInt("SelectedCharacter");
            if (selectedCharacterIndex >= characters.Count)
            {
                selectedCharacterIndex = 0;
            }
        }
    }
    
    public void ResetCharacterProgress()
    {
        // Reset all characters to locked except default
        foreach (Character character in characters)
        {
            character.isUnlocked = character.isDefault;
        }
        
        // Reset selected character to default
        Character defaultChar = characters.Find(c => c.isDefault);
        selectedCharacterIndex = defaultChar != null ? characters.IndexOf(defaultChar) : 0;
        
        // Clear saved progress
        for (int i = 0; i < characters.Count; i++)
        {
            PlayerPrefs.DeleteKey($"Character_{i}_Unlocked");
        }
        PlayerPrefs.DeleteKey("SelectedCharacter");
        PlayerPrefs.Save();
        
        // Update display
        UpdateDisplay();
        CreateCharacterButtons();
    }
    
    public void UnlockAllCharacters()
    {
        foreach (Character character in characters)
        {
            character.isUnlocked = true;
        }
        
        SaveCharacterProgress();
        UpdateDisplay();
        CreateCharacterButtons();
    }
}