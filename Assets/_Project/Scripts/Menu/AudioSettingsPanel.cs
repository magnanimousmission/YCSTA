using UnityEngine;
using UnityEngine.UI;

public class AudioSettingsPanel : MonoBehaviour
{
    private const float DefaultMaster = 1f;
    private const float DefaultMusic = 0.7f;
    private const float DefaultSfx = 1f;
    private const float DefaultAmbient = 0.5f;

    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider ambientSlider;
    [SerializeField] private OptionsMenuManager optionsMenuManager;

    private float _pendingMaster;
    private float _pendingMusic;
    private float _pendingSfx;
    private float _pendingAmbient;

    private void Awake()
    {
        if (optionsMenuManager == null)
            optionsMenuManager = GetComponentInParent<OptionsMenuManager>();
    }

    private void OnEnable()
    {
        masterSlider?.onValueChanged.AddListener(OnMasterChanged);
        musicSlider?.onValueChanged.AddListener(OnMusicChanged);
        sfxSlider?.onValueChanged.AddListener(OnSfxChanged);
        ambientSlider?.onValueChanged.AddListener(OnAmbientChanged);

        RefreshFromAudioManager();
    }

    private void OnDisable()
    {
        masterSlider?.onValueChanged.RemoveListener(OnMasterChanged);
        musicSlider?.onValueChanged.RemoveListener(OnMusicChanged);
        sfxSlider?.onValueChanged.RemoveListener(OnSfxChanged);
        ambientSlider?.onValueChanged.RemoveListener(OnAmbientChanged);
    }

    private void OnMasterChanged(float value)
    {
        _pendingMaster = value;
        AudioManager.Instance?.SetMasterVolume(value, save: false);
    }

    private void OnMusicChanged(float value)
    {
        _pendingMusic = value;
        AudioManager.Instance?.SetMusicVolume(value, save: false);
    }

    private void OnSfxChanged(float value)
    {
        _pendingSfx = value;
        AudioManager.Instance?.SetSfxVolume(value, save: false);
    }

    private void OnAmbientChanged(float value)
    {
        _pendingAmbient = value;
        AudioManager.Instance?.SetAmbientVolume(value, save: false);
    }

    public void RefreshFromAudioManager()
    {
        var audio = AudioManager.Instance;
        if (audio != null)
        {
            _pendingMaster = audio.GetMasterVolume();
            _pendingMusic = audio.GetMusicVolume();
            _pendingSfx = audio.GetSfxVolume();
            _pendingAmbient = audio.GetAmbientVolume();

            SetSliderSilent(masterSlider, _pendingMaster);
            SetSliderSilent(musicSlider, _pendingMusic);
            SetSliderSilent(sfxSlider, _pendingSfx);
            SetSliderSilent(ambientSlider, _pendingAmbient);
            return;
        }

        _pendingMaster = PlayerPrefs.GetFloat("Vol_Master", DefaultMaster);
        _pendingMusic = PlayerPrefs.GetFloat("Vol_Music", DefaultMusic);
        _pendingSfx = PlayerPrefs.GetFloat("Vol_Sfx", DefaultSfx);
        _pendingAmbient = PlayerPrefs.GetFloat("Vol_Ambient", DefaultAmbient);

        SetSliderSilent(masterSlider, _pendingMaster);
        SetSliderSilent(musicSlider, _pendingMusic);
        SetSliderSilent(sfxSlider, _pendingSfx);
        SetSliderSilent(ambientSlider, _pendingAmbient);
    }

    public void SaveChanges()
    {
        var audio = AudioManager.Instance;
        if (audio == null) return;

        audio.SetMasterVolume(_pendingMaster, save: false);
        audio.SetMusicVolume(_pendingMusic, save: false);
        audio.SetSfxVolume(_pendingSfx, save: false);
        audio.SetAmbientVolume(_pendingAmbient, save: false);
        audio.SaveCurrentVolumeSettings();

        optionsMenuManager?.CloseOptions();
    }

    public void ResetToDefaults()
    {
        _pendingMaster = DefaultMaster;
        _pendingMusic = DefaultMusic;
        _pendingSfx = DefaultSfx;
        _pendingAmbient = DefaultAmbient;

        SetSliderSilent(masterSlider, _pendingMaster);
        SetSliderSilent(musicSlider, _pendingMusic);
        SetSliderSilent(sfxSlider, _pendingSfx);
        SetSliderSilent(ambientSlider, _pendingAmbient);

        var audio = AudioManager.Instance;
        if (audio == null) return;
        audio.SetMasterVolume(_pendingMaster, save: false);
        audio.SetMusicVolume(_pendingMusic, save: false);
        audio.SetSfxVolume(_pendingSfx, save: false);
        audio.SetAmbientVolume(_pendingAmbient, save: false);
    }

    private static void SetSliderSilent(Slider slider, float value)
    {
        if (slider == null) return;
        slider.SetValueWithoutNotify(value);
    }
}
