using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource ambientSource;

    [Header("Ambience Clips")]
    [SerializeField] private List<AudioClip> ambienceClips = new List<AudioClip>();

    [Header("Sound Clips")]
    [SerializeField] private List<AudioClip> soundClips = new List<AudioClip>();

    public enum SfxClip
    {
        ButtonClick = 0,
        DeathScreenSound = 1,
        PlayerJoinRoomSound = 2,
        PlayerLeaveRoomSound = 3,
        StartGameSound = 4,
    }

    [Header("Scene Music")]
    [SerializeField] private List<string> gameplaySceneNames = new List<string> { "03_Game", "04_OnlineGame" };

    [Header("Volume")]
    [Range(0, 1)] [SerializeField] private float masterVolume  = 1f;
    [Range(0, 1)] [SerializeField] private float musicVolume   = 0.7f;
    [Range(0, 1)] [SerializeField] private float sfxVolume     = 1f;
    [Range(0, 1)] [SerializeField] private float ambientVolume = 0.5f;

    private const string k_MasterVol  = "Vol_Master";
    private const string k_MusicVol   = "Vol_Music";
    private const string k_SfxVol     = "Vol_Sfx";
    private const string k_AmbientVol = "Vol_Ambient";

    private Coroutine musicFadeCoroutine;

    private const int MainMenuClipIndex = 0;
    private const int GameplayClipIndex = 1;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        CreateSourcesIfNeeded();
        LoadVolumeSettings();
        ApplyVolumeSettings();
    }

    private void Start()
    {
        UpdateMusicForScene(SceneManager.GetActiveScene());
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        UpdateMusicForScene(scene);
    }

    private void UpdateMusicForScene(Scene scene)
    {
        bool isGameplayScene = gameplaySceneNames != null && gameplaySceneNames.Contains(scene.name);
        int targetIndex = isGameplayScene ? GameplayClipIndex : MainMenuClipIndex;
        AudioClip targetClip = GetAmbienceClip(targetIndex);

        if (targetClip == null)
            return;

        if (musicSource != null && musicSource.clip == targetClip && musicSource.isPlaying)
            return;

        PlayMusic(targetClip, loop: true);
    }

    private AudioClip GetAmbienceClip(int index)
    {
        if (ambienceClips == null || index < 0 || index >= ambienceClips.Count)
            return null;

        return ambienceClips[index];
    }

    private void CreateSourcesIfNeeded()
    {
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
        }

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
        }

        if (ambientSource == null)
        {
            ambientSource = gameObject.AddComponent<AudioSource>();
            ambientSource.loop = true;
            ambientSource.playOnAwake = false;
        }
    }

    private void ApplyVolumeSettings()
    {
        if (musicSource != null)   musicSource.volume   = masterVolume * musicVolume;
        if (sfxSource != null)     sfxSource.volume     = masterVolume * sfxVolume;
        if (ambientSource != null) ambientSource.volume = masterVolume * ambientVolume;
    }

    private void LoadVolumeSettings()
    {
        masterVolume  = PlayerPrefs.GetFloat(k_MasterVol,  masterVolume);
        musicVolume   = PlayerPrefs.GetFloat(k_MusicVol,   musicVolume);
        sfxVolume     = PlayerPrefs.GetFloat(k_SfxVol,     sfxVolume);
        ambientVolume = PlayerPrefs.GetFloat(k_AmbientVol, ambientVolume);
    }

    private void SaveVolumeSettings()
    {
        PlayerPrefs.SetFloat(k_MasterVol,  masterVolume);
        PlayerPrefs.SetFloat(k_MusicVol,   musicVolume);
        PlayerPrefs.SetFloat(k_SfxVol,     sfxVolume);
        PlayerPrefs.SetFloat(k_AmbientVol, ambientVolume);
        PlayerPrefs.Save();
    }

    public void SaveCurrentVolumeSettings()
    {
        SaveVolumeSettings();
    }

    public float GetMasterVolume()  => masterVolume;
    public float GetMusicVolume()   => musicVolume;
    public float GetSfxVolume()     => sfxVolume;
    public float GetAmbientVolume() => ambientVolume;

    public void SetMasterVolume(float volume, bool save = true)
    {
        masterVolume = Mathf.Clamp01(volume);
        ApplyVolumeSettings();
        if (save)
            SaveVolumeSettings();
    }

    public void SetMusicVolume(float volume, bool save = true)
    {
        musicVolume = Mathf.Clamp01(volume);
        ApplyVolumeSettings();
        if (save)
            SaveVolumeSettings();
    }

    public void SetSfxVolume(float volume, bool save = true)
    {
        sfxVolume = Mathf.Clamp01(volume);
        ApplyVolumeSettings();
        if (save)
            SaveVolumeSettings();
    }

    public void SetAmbientVolume(float volume, bool save = true)
    {
        ambientVolume = Mathf.Clamp01(volume);
        ApplyVolumeSettings();
        if (save)
            SaveVolumeSettings();
    }

    public void PlayMusic(AudioClip clip, bool loop = true, float fadeTime = 0.5f)
    {
        if (clip == null)
            return;

        if (musicSource == null)
            CreateSourcesIfNeeded();

        musicSource.loop = loop;

        if (fadeTime > 0f && musicSource.isPlaying)
        {
            if (musicFadeCoroutine != null)
                StopCoroutine(musicFadeCoroutine);

            musicFadeCoroutine = StartCoroutine(FadeMusic(clip, fadeTime));
        }
        else
        {
            musicSource.clip = clip;
            musicSource.Play();
        }
    }

    public void PlayMusic(string resourcePath, bool loop = true, float fadeTime = 0.5f)
    {
        AudioClip clip = Resources.Load<AudioClip>(resourcePath);
        if (clip == null)
            return;

        PlayMusic(clip, loop, fadeTime);
    }

    public void StopMusic(float fadeTime = 0.5f)
    {
        if (musicSource == null || !musicSource.isPlaying)
            return;

        if (fadeTime > 0f)
        {
            if (musicFadeCoroutine != null)
                StopCoroutine(musicFadeCoroutine);

            musicFadeCoroutine = StartCoroutine(FadeOutMusic(fadeTime));
        }
        else
        {
            musicSource.Stop();
        }
    }

    public void StopBackgroundAudio(float musicFadeTime = 0f)
    {
        StopMusic(musicFadeTime);

        if (ambientSource == null)
            return;

        ambientSource.Stop();
    }

    public void PlaySfx(SfxClip sfx, float volumeScale = 1f)
    {
        int index = (int)sfx;
        if (index < 0 || index >= soundClips.Count) return;
        PlaySfx(soundClips[index], volumeScale);
    }

    public float GetSfxClipLength(SfxClip sfx)
    {
        int index = (int)sfx;
        if (index < 0 || index >= soundClips.Count || soundClips[index] == null) return 0f;
        return soundClips[index].length;
    }

    public void PlaySfx(AudioClip clip, float volumeScale = 1f)
    {
        if (clip == null || sfxSource == null)
            return;

        sfxSource.PlayOneShot(clip, volumeScale * masterVolume);
    }    

    public void PlaySfx(string resourcePath, float volumeScale = 1f)
    {
        AudioClip clip = Resources.Load<AudioClip>(resourcePath);
        if (clip == null)
            return;

        PlaySfx(clip, volumeScale);
    }

    private IEnumerator FadeMusic(AudioClip nextClip, float duration)
    {
        float startVolume = musicSource.volume;
        float targetVolume = 0f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            musicSource.volume = Mathf.Lerp(startVolume, targetVolume, t);
            yield return null;
        }

        musicSource.clip = nextClip;
        musicSource.Play();
        elapsed = 0f;

        float endVolume = masterVolume * musicVolume;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            musicSource.volume = Mathf.Lerp(0f, endVolume, t);
            yield return null;
        }

        musicSource.volume = endVolume;
        musicFadeCoroutine = null;
    }

    private IEnumerator FadeOutMusic(float duration)
    {
        float startVolume = musicSource.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            musicSource.volume = Mathf.Lerp(startVolume, 0f, t);
            yield return null;
        }

        musicSource.Stop();
        musicSource.volume = masterVolume * musicVolume;
        musicFadeCoroutine = null;
    }
}
