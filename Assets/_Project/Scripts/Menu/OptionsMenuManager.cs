using UnityEngine;
using UnityEngine.UI;

public class OptionsMenuManager : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject optionsRoot;

    [Header("Panels")]
    [SerializeField] private GameObject graphicsPanel;
    [SerializeField] private GameObject audioPanel;
    [SerializeField] private GameObject controlsPanel;
    [SerializeField] private GameObject gameplayPanel;

    [Header("Tab Buttons")]
    [SerializeField] private Button graphicsButton;
    [SerializeField] private Button audioButton;
    [SerializeField] private Button controlsButton;
    [SerializeField] private Button gameplayButton;

    [Header("Tab Colors")]
    [SerializeField] private Color selectedColor   = new Color(1f, 1f, 1f, 1f);
    [SerializeField] private Color deselectedColor = new Color(0.7f, 0.7f, 0.7f, 1f);

    [Header("Settings")]
    [SerializeField] private DefaultPanel defaultPanel = DefaultPanel.Graphics;

    private GameObject _activePanel;
    private Button _activeButton;

    private enum DefaultPanel { Graphics, Audio, Controls, Gameplay }

    private void Awake()
    {
        optionsRoot?.SetActive(false);
    }

    public void OpenOptions()
    {
        optionsRoot?.SetActive(true);
        var (panel, button) = GetDefault();
        ShowPanel(panel, button);
    }

    private (GameObject panel, Button button) GetDefault() => defaultPanel switch
    {
        DefaultPanel.Audio    => (audioPanel,    audioButton),
        DefaultPanel.Controls => (controlsPanel, controlsButton),
        DefaultPanel.Gameplay => (gameplayPanel, gameplayButton),
        _                     => (graphicsPanel, graphicsButton),
    };

    public void CloseOptions()
    {
        AudioManager.Instance?.PlaySfx(AudioManager.SfxClip.ButtonClick);
        HideAllPanels();
        optionsRoot?.SetActive(false);
    }

    public void ShowGraphics()  => ShowPanel(graphicsPanel,  graphicsButton);
    public void ShowAudio()     => ShowPanel(audioPanel,     audioButton);
    public void ShowControls()  => ShowPanel(controlsPanel,  controlsButton);
    public void ShowGameplay()  => ShowPanel(gameplayPanel,  gameplayButton);

    public void OnSaveChangesPressed()
    {
        AudioManager.Instance?.PlaySfx(AudioManager.SfxClip.ButtonClick);
        audioPanel?.GetComponent<AudioSettingsPanel>()?.SaveChanges();
        CloseOptions();
    }

    public void OnResetDefaultsPressed()
    {
        AudioManager.Instance?.PlaySfx(AudioManager.SfxClip.ButtonClick);
        audioPanel?.GetComponent<AudioSettingsPanel>()?.ResetToDefaults();
    }

    void ShowPanel(GameObject panel, Button button)
    {
        AudioManager.Instance?.PlaySfx(AudioManager.SfxClip.ButtonClick);
        if (_activePanel == panel) return;
        
        HideAllPanels();
        _activePanel = panel;
        _activePanel?.SetActive(true);

        if (_activePanel == audioPanel)
            _activePanel.GetComponent<AudioSettingsPanel>()?.RefreshFromAudioManager();

        _activeButton = button;
        SetButtonColor(_activeButton, selectedColor);
    }

    void HideAllPanels()
    {
        graphicsPanel?.SetActive(false);
        audioPanel?.SetActive(false);
        controlsPanel?.SetActive(false);
        gameplayPanel?.SetActive(false);
        _activePanel = null;

        SetButtonColor(graphicsButton,  deselectedColor);
        SetButtonColor(audioButton,     deselectedColor);
        SetButtonColor(controlsButton,  deselectedColor);
        SetButtonColor(gameplayButton,  deselectedColor);
        _activeButton = null;
    }

    void SetButtonColor(Button btn, Color color)
    {
        if (btn == null) return;
        var colors = btn.colors;
        colors.normalColor = color;
        btn.colors = colors;
    }
}
