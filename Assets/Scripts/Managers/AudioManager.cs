using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;
    [Range(0f, 1f)]
    public float volume = 1f;
    [Range(0.1f, 3f)]
    public float pitch = 1f;
    public bool loop = false;
    
    [HideInInspector]
    public AudioSource source;
}

public class AudioManager : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;
    
    [Header("Sound Effects")]
    public Sound[] sounds;
    
    [Header("Background Music")]
    public AudioClip mainMenuMusic;
    public AudioClip gameplayMusic;
    public AudioClip victoryMusic;
    
    [Header("Settings")]
    [Range(0f, 1f)]
    public float masterVolume = 1f;
    [Range(0f, 1f)]
    public float musicVolume = 0.7f;
    [Range(0f, 1f)]
    public float sfxVolume = 1f;
    
    private Dictionary<string, Sound> soundDictionary;
    
    public static AudioManager Instance { get; private set; }
    
    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudio();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void InitializeAudio()
    {
        // Create audio sources if they don't exist
        if (musicSource == null)
        {
            GameObject musicObj = new GameObject("MusicSource");
            musicObj.transform.SetParent(transform);
            musicSource = musicObj.AddComponent<AudioSource>();
            musicSource.loop = true;
        }
        
        if (sfxSource == null)
        {
            GameObject sfxObj = new GameObject("SFXSource");
            sfxObj.transform.SetParent(transform);
            sfxSource = sfxObj.AddComponent<AudioSource>();
        }
        
        // Initialize sound dictionary
        soundDictionary = new Dictionary<string, Sound>();
        
        foreach (Sound sound in sounds)
        {
            sound.source = sfxSource.gameObject.AddComponent<AudioSource>();
            sound.source.clip = sound.clip;
            sound.source.volume = sound.volume * sfxVolume * masterVolume;
            sound.source.pitch = sound.pitch;
            sound.source.loop = sound.loop;
            
            soundDictionary[sound.name] = sound;
        }
        
        // Set initial volumes
        UpdateVolumes();
    }
    
    public void PlaySound(string soundName)
    {
        if (soundDictionary.ContainsKey(soundName))
        {
            Sound sound = soundDictionary[soundName];
            sound.source.Play();
        }
        else
        {
            Debug.LogWarning("Sound: " + soundName + " not found!");
        }
    }
    
    public void StopSound(string soundName)
    {
        if (soundDictionary.ContainsKey(soundName))
        {
            Sound sound = soundDictionary[soundName];
            sound.source.Stop();
        }
    }
    
    public void PlayMusic(AudioClip musicClip)
    {
        if (musicSource != null && musicClip != null)
        {
            musicSource.clip = musicClip;
            musicSource.volume = musicVolume * masterVolume;
            musicSource.Play();
        }
    }
    
    public void StopMusic()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
        }
    }
    
    public void PauseMusic()
    {
        if (musicSource != null)
        {
            musicSource.Pause();
        }
    }
    
    public void ResumeMusic()
    {
        if (musicSource != null)
        {
            musicSource.UnPause();
        }
    }
    
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        UpdateVolumes();
    }
    
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        UpdateVolumes();
    }
    
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        UpdateVolumes();
    }
    
    void UpdateVolumes()
    {
        // Update music volume
        if (musicSource != null)
        {
            musicSource.volume = musicVolume * masterVolume;
        }
        
        // Update all sound effect volumes
        foreach (Sound sound in sounds)
        {
            if (sound.source != null)
            {
                sound.source.volume = sound.volume * sfxVolume * masterVolume;
            }
        }
    }
    
    // Convenience methods for common sounds
    public void PlayJumpSound()
    {
        PlaySound("jump");
    }
    
    public void PlayCoinCollectSound()
    {
        PlaySound("coin_collect");
    }
    
    public void PlayPlayerHurtSound()
    {
        PlaySound("player_hurt");
    }
    
    public void PlayButtonClickSound()
    {
        PlaySound("button_click");
    }
    
    public void PlayLevelCompleteSound()
    {
        PlaySound("level_complete");
    }
    
    public void PlayGameOverSound()
    {
        PlaySound("game_over");
    }
    
    // Music methods
    public void PlayMainMenuMusic()
    {
        PlayMusic(mainMenuMusic);
    }
    
    public void PlayGameplayMusic()
    {
        PlayMusic(gameplayMusic);
    }
    
    public void PlayVictoryMusic()
    {
        PlayMusic(victoryMusic);
    }
    
    void OnApplicationPause(bool pauseStatus)
    {
        // Pause/resume music when app goes to background
        if (pauseStatus)
        {
            PauseMusic();
        }
        else
        {
            ResumeMusic();
        }
    }
}