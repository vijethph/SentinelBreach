using System.Collections;
using System.Collections.Generic;
using UnityEngine;





public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else Destroy(gameObject);
    }

    [Header("SFX Clips")]
    public AudioClip sfxLaser;
    public AudioClip sfxHit;
    public AudioClip sfxEMP;
    public AudioClip sfxTimeSlow;
    public AudioClip sfxShard;
    public AudioClip sfxDash;
    public AudioClip sfxExfil;
    public AudioClip sfxDeath;

    [Header("Background Music")]
    public AudioClip bgmClip;

    [Header("Volume")]
    [Range(0f, 1f)] public float sfxVolume  = 0.8f;
    [Range(0f, 1f)] public float musicVolume = 0.4f;

    private AudioSource sfxSource;
    private AudioSource musicSource;

    void Start()
    {
        
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;

        
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop        = true;
        musicSource.playOnAwake = false;
        musicSource.volume      = musicVolume;

        if (bgmClip != null)
        {
            musicSource.clip = bgmClip;
            musicSource.Play();
        }
    }

    

    public void PlayLaser()    => Play(sfxLaser);
    public void PlayHit()      => Play(sfxHit);
    public void PlayEMP()      => Play(sfxEMP);
    public void PlayTimeSlow() => Play(sfxTimeSlow);
    public void PlayShard()    => Play(sfxShard);
    public void PlayDash()     => Play(sfxDash);
    public void PlayExfil()    => Play(sfxExfil);
    public void PlayDeath()    => Play(sfxDeath);

    void Play(AudioClip clip)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip, sfxVolume);
    }

    
    public void IncreaseMusicPitch()
    {
        if (musicSource != null)
            musicSource.pitch = Mathf.Clamp(musicSource.pitch + 0.1f, 1f, 1.5f);
    }
}
