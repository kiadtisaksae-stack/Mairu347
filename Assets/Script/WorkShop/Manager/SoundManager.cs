using UnityEngine;
using System.Collections.Generic;

// กำหนดให้เป็น sealed เพื่อป้องกันการสืบทอด
public sealed class SoundManager : MonoBehaviour
{
    // 1. Singleton Instance
    private static SoundManager _instance;

    // 2. Public Static Property (Global Access Point)
    public static SoundManager Instance
    {
        get
        {
            if (_instance == null)
            {
                Debug.LogError("SoundManager instance is null! Is it in the scene?");
            }
            return _instance;
        }
    }

    [Header("Audio Sources")]
    // Audio Source สำหรับเพลงประกอบ (Looping)
    public AudioSource musicSource;
    // Audio Source สำหรับเอฟเฟกต์เสียง (Non-Looping)
    public AudioSource sfxSource;

    [Header("Default Audio Clips")]
    public AudioClip defaultButtonClick;
    public AudioClip defaultBackgroundMusic;

    // 3. Singleton Initialization
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);

            // ตรวจสอบและเพิ่ม Audio Source หากยังไม่มี
            if (musicSource == null) musicSource = gameObject.AddComponent<AudioSource>();
            if (sfxSource == null) sfxSource = gameObject.AddComponent<AudioSource>();

            // ตั้งค่าพื้นฐาน
            musicSource.loop = true; // เพลงประกอบมักจะวนซ้ำ

            PlayMusic(defaultBackgroundMusic);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ------------------- Music Controls -------------------

    /// <summary>
    /// เล่นเพลงประกอบใหม่
    /// </summary>
    public void PlayMusic(AudioClip clip)
    {
        if (clip == null || musicSource == null) return;

        musicSource.clip = clip;
        musicSource.Play();
    }

    public void StopMusic()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
        }
    }

    // ------------------- SFX Controls -------------------

    /// <summary>
    /// เล่นเอฟเฟกต์เสียงแบบครั้งเดียวจบ (One-Shot)
    /// </summary>
    public void PlaySFX(AudioClip clip)
    {
        if (clip == null || sfxSource == null) return;

        // ใช้ PlayOneShot เพื่อให้เล่นหลายเสียงทับซ้อนกันได้
        sfxSource.PlayOneShot(clip);
    }

    // ------------------- Volume Controls -------------------

    /// <summary>
    /// กำหนดระดับเสียงหลักของเพลงประกอบ (0.0 ถึง 1.0)
    /// </summary>
    public void SetMusicVolume(float volume)
    {
        if (musicSource != null)
        {
            musicSource.volume = volume;
        }
    }

    /// <summary>
    /// กำหนดระดับเสียงหลักของเอฟเฟกต์เสียง (0.0 ถึง 1.0)
    /// </summary>
    public void SetSFXVolume(float volume)
    {
        if (sfxSource != null)
        {
            sfxSource.volume = volume;
        }
    }
}