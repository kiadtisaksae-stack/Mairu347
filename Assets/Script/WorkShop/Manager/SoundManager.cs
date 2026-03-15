using UnityEngine;

public sealed class SoundManager : MonoBehaviour
{
    // ---- Singleton ----
    private static SoundManager _instance;
    public static SoundManager Instance
    {
        get
        {
            if (_instance == null)
                Debug.LogError("SoundManager instance is null! Is it in the scene?");
            return _instance;
        }
    }

    [Header("Audio Sources")]
    public AudioSource musicSource; // สำหรับเพลงประกอบ (Looping)
    public AudioSource sfxSource;   // สำหรับเอฟเฟกต์เสียง (Non-Looping)

    [Header("Default Audio Clips")]
    public AudioClip defaultButtonClick;
    public AudioClip defaultBackgroundMusic;

    [Header("Volume")]
    [Range(0f, 1f)] public float musicVolume = 0.5f;
    [Range(0f, 1f)] public float sfxVolume = 0.5f;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);

            if (musicSource == null) musicSource = gameObject.AddComponent<AudioSource>();
            if (sfxSource == null) sfxSource = gameObject.AddComponent<AudioSource>();

            musicSource.loop = true;
            musicSource.volume = musicVolume;
            sfxSource.volume = sfxVolume;

            PlayMusic(defaultBackgroundMusic);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ------------------- Music -------------------

    public void PlayMusic(AudioClip clip)
    {
        if (clip == null || musicSource == null) return;
        musicSource.clip = clip;
        musicSource.Play();
    }

    public void StopMusic()
    {
        if (musicSource != null) musicSource.Stop();
    }

    // ------------------- SFX -------------------

    /// <summary>PlayOneShot — เล่นทับซ้อนกันได้</summary>
    public void PlaySFX(AudioClip clip)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip, sfxVolume);
    }

    public void PlayButtonClick()
    {
        PlaySFX(defaultButtonClick);
    }

    // ------------------- Volume -------------------

    /// <summary>ตั้งระดับเสียงเพลงประกอบ (0.0 ถึง 1.0)</summary>
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (musicSource != null) musicSource.volume = musicVolume;
    }

    /// <summary>ตั้งระดับเสียงเอฟเฟกต์ (0.0 ถึง 1.0)</summary>
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        if (sfxSource != null) sfxSource.volume = sfxVolume;
    }
}