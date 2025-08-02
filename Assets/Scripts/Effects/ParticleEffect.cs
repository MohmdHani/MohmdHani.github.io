using UnityEngine;

public class ParticleEffect : MonoBehaviour
{
    [Header("Effect Settings")]
    public float lifetime = 2f;
    public bool autoDestroy = true;
    public bool playOnStart = true;
    
    [Header("Particle Systems")]
    public ParticleSystem[] particleSystems;
    
    [Header("Audio")]
    public AudioClip effectSound;
    public float soundVolume = 1f;
    
    private float timer = 0f;
    
    void Start()
    {
        if (playOnStart)
        {
            PlayEffect();
        }
    }
    
    void Update()
    {
        if (autoDestroy)
        {
            timer += Time.deltaTime;
            if (timer >= lifetime)
            {
                Destroy(gameObject);
            }
        }
    }
    
    public void PlayEffect()
    {
        // Play all particle systems
        foreach (ParticleSystem ps in particleSystems)
        {
            if (ps != null)
            {
                ps.Play();
            }
        }
        
        // Play sound effect
        if (effectSound != null)
        {
            AudioSource.PlayClipAtPoint(effectSound, transform.position, soundVolume);
        }
    }
    
    public void StopEffect()
    {
        foreach (ParticleSystem ps in particleSystems)
        {
            if (ps != null)
            {
                ps.Stop();
            }
        }
    }
    
    public void SetColor(Color color)
    {
        foreach (ParticleSystem ps in particleSystems)
        {
            if (ps != null)
            {
                var main = ps.main;
                main.startColor = color;
            }
        }
    }
    
    public void SetScale(float scale)
    {
        transform.localScale = Vector3.one * scale;
    }
    
    public static ParticleEffect CreateEffect(GameObject effectPrefab, Vector3 position, Quaternion rotation = default)
    {
        if (effectPrefab != null)
        {
            GameObject instance = Instantiate(effectPrefab, position, rotation);
            ParticleEffect effect = instance.GetComponent<ParticleEffect>();
            if (effect != null)
            {
                effect.PlayEffect();
            }
            return effect;
        }
        return null;
    }
}