// SoundManager.cs - Manages audio and atmospheric sounds
using UnityEngine;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;
    public AudioSource ambientSource;
    
    [Header("Audio Clips")]
    public List<AudioClip> backgroundMusic = new List<AudioClip>();
    public List<AudioClip> hauntingSounds = new List<AudioClip>();
    public List<AudioClip> ambientSounds = new List<AudioClip>();
    
    [Header("Settings")]
    public float musicVolume = 0.7f;
    public float sfxVolume = 1f;
    public float ambientVolume = 0.5f;
    
    private static SoundManager instance;
    public static SoundManager Instance => instance;
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudioSources();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void InitializeAudioSources()
    {
        if (musicSource != null)
        {
            musicSource.volume = musicVolume;
            musicSource.loop = true;
        }
        
        if (sfxSource != null)
            sfxSource.volume = sfxVolume;
            
        if (ambientSource != null)
        {
            ambientSource.volume = ambientVolume;
            ambientSource.loop = true;
        }
    }
    
    public void PlayBackgroundMusic(int index = -1)
    {
        if (backgroundMusic.Count == 0 || musicSource == null) return;
        
        if (index < 0)
            index = Random.Range(0, backgroundMusic.Count);
            
        index = Mathf.Clamp(index, 0, backgroundMusic.Count - 1);
        
        musicSource.clip = backgroundMusic[index];
        musicSource.Play();
    }
    
    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip != null && sfxSource != null)
            sfxSource.PlayOneShot(clip, volume);
    }
    
    public void PlayHauntingSound()
    {
        if (hauntingSounds.Count > 0)
        {
            AudioClip clip = hauntingSounds[Random.Range(0, hauntingSounds.Count)];
            PlaySFX(clip);
        }
    }
    
    public void PlayAmbient(int index = -1)
    {
        if (ambientSounds.Count == 0 || ambientSource == null) return;
        
        if (index < 0)
            index = Random.Range(0, ambientSounds.Count);
            
        index = Mathf.Clamp(index, 0, ambientSounds.Count - 1);
        
        ambientSource.clip = ambientSounds[index];
        ambientSource.Play();
    }
    
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (musicSource != null)
            musicSource.volume = musicVolume;
    }
    
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        if (sfxSource != null)
            sfxSource.volume = sfxVolume;
    }
    
    public void SetAmbientVolume(float volume)
    {
        ambientVolume = Mathf.Clamp01(volume);
        if (ambientSource != null)
            ambientSource.volume = ambientVolume;
    }
}