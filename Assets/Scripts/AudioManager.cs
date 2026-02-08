using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource bgmSource;
    public AudioSource sfxSource;

    [Header("Settings")]
    [Range(0, 1)] public float bgmVolume = 0.5f;
    [Range(0, 1)] public float sfxVolume = 0.8f;

    private bool isMuted = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadSettings();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ToggleMute()
    {
        isMuted = !isMuted;
        ApplySettings();
        SaveSettings();
    }

    public void SetBGMVolume(float volume)
    {
        bgmVolume = volume;
        ApplySettings();
        SaveSettings();
    }

    public bool IsMuted() => isMuted;

    private void ApplySettings()
    {
        if (bgmSource != null)
        {
            bgmSource.mute = isMuted;
            bgmSource.volume = bgmVolume;
            if (!isMuted && !bgmSource.isPlaying) bgmSource.Play();
        }
        if (sfxSource != null)
        {
            sfxSource.mute = isMuted;
            sfxSource.volume = sfxVolume;
        }
    }

    private void SaveSettings()
    {
        PlayerPrefs.SetInt("SoundMuted", isMuted ? 1 : 0);
        PlayerPrefs.SetFloat("BGMVolume", bgmVolume);
        PlayerPrefs.Save();
    }

    private void LoadSettings()
    {
        isMuted = PlayerPrefs.GetInt("SoundMuted", 0) == 1;
        bgmVolume = PlayerPrefs.GetFloat("BGMVolume", 0.5f);
        ApplySettings();
    }

    public void PlaySFX(AudioClip clip)
    {
        if (isMuted || clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip);
    }
}
