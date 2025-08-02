using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Menu Panels")]
    public GameObject mainMenuPanel;
    public GameObject settingsPanel;
    public GameObject creditsPanel;
    
    [Header("Main Menu Buttons")]
    public Button playButton;
    public Button settingsButton;
    public Button creditsButton;
    public Button quitButton;
    
    [Header("Settings")]
    public Slider masterVolumeSlider;
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;
    public Toggle mobileControlsToggle;
    public Button settingsBackButton;
    
    [Header("Credits")]
    public Button creditsBackButton;
    
    [Header("Audio")]
    public AudioClip buttonClickSound;
    public AudioClip menuMusic;
    
    void Start()
    {
        SetupButtons();
        SetupSettings();
        ShowMainMenu();
        
        // Play menu music
        if (menuMusic != null)
        {
            AudioManager.Instance?.PlayMusic(menuMusic);
        }
    }
    
    void SetupButtons()
    {
        // Main menu buttons
        if (playButton != null)
            playButton.onClick.AddListener(StartGame);
        
        if (settingsButton != null)
            settingsButton.onClick.AddListener(ShowSettings);
        
        if (creditsButton != null)
            creditsButton.onClick.AddListener(ShowCredits);
        
        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);
        
        // Settings back button
        if (settingsBackButton != null)
            settingsBackButton.onClick.AddListener(ShowMainMenu);
        
        // Credits back button
        if (creditsBackButton != null)
            creditsBackButton.onClick.AddListener(ShowMainMenu);
    }
    
    void SetupSettings()
    {
        // Volume sliders
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.value = PlayerPrefs.GetFloat("MasterVolume", 1f);
            masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        }
        
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume", 0.7f);
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        }
        
        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        }
        
        // Mobile controls toggle
        if (mobileControlsToggle != null)
        {
            mobileControlsToggle.isOn = PlayerPrefs.GetInt("MobileControls", 1) == 1;
            mobileControlsToggle.onValueChanged.AddListener(OnMobileControlsChanged);
        }
    }
    
    public void StartGame()
    {
        PlayButtonSound();
        
        // Reset game state
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetGame();
        }
        
        // Load first level
        SceneManager.LoadScene("Level1");
    }
    
    public void ShowSettings()
    {
        PlayButtonSound();
        ShowPanel(settingsPanel);
    }
    
    public void ShowCredits()
    {
        PlayButtonSound();
        ShowPanel(creditsPanel);
    }
    
    public void ShowMainMenu()
    {
        PlayButtonSound();
        ShowPanel(mainMenuPanel);
    }
    
    public void QuitGame()
    {
        PlayButtonSound();
        
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    
    void ShowPanel(GameObject panel)
    {
        // Hide all panels
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(false);
        
        // Show target panel
        if (panel != null) panel.SetActive(true);
    }
    
    void OnMasterVolumeChanged(float value)
    {
        AudioManager.Instance?.SetMasterVolume(value);
        PlayerPrefs.SetFloat("MasterVolume", value);
        PlayerPrefs.Save();
    }
    
    void OnMusicVolumeChanged(float value)
    {
        AudioManager.Instance?.SetMusicVolume(value);
        PlayerPrefs.SetFloat("MusicVolume", value);
        PlayerPrefs.Save();
    }
    
    void OnSFXVolumeChanged(float value)
    {
        AudioManager.Instance?.SetSFXVolume(value);
        PlayerPrefs.SetFloat("SFXVolume", value);
        PlayerPrefs.Save();
    }
    
    void OnMobileControlsChanged(bool value)
    {
        PlayerPrefs.SetInt("MobileControls", value ? 1 : 0);
        PlayerPrefs.Save();
    }
    
    void PlayButtonSound()
    {
        if (buttonClickSound != null)
        {
            AudioManager.Instance?.PlaySound("button_click");
        }
    }
    
    void Update()
    {
        // Handle back button on mobile
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (settingsPanel != null && settingsPanel.activeSelf)
            {
                ShowMainMenu();
            }
            else if (creditsPanel != null && creditsPanel.activeSelf)
            {
                ShowMainMenu();
            }
        }
    }
}