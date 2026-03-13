using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class MainMenuManager : MonoBehaviourPunCallbacks
{
    const string k_PlayerNameKey = "PlayerName";

    [SerializeField] string gameVersion = "1";
    [SerializeField] string roomName = "BlahRoom";

    [Header("Username")]
    [SerializeField] GameObject usernamePromptPanel;
    [SerializeField] TMP_InputField usernameInput;
    [SerializeField] GameObject creditsPanel;

    [Header("Options")]
    [SerializeField] OptionsMenuManager optionsMenuManager;

    void Start()
    {
        usernamePromptPanel?.SetActive(false);
        creditsPanel?.SetActive(false);
    }

    public void OnSinglePlayerPressed()
    {
        AudioManager.Instance?.PlaySfx(AudioManager.SfxClip.StartGameSound);
        SceneManager.LoadScene("03_Game");
    }

    public void OnCreditsPressed() 
    {
        AudioManager.Instance?.PlaySfx(AudioManager.SfxClip.ButtonClick);
        creditsPanel?.SetActive(true);
    }

    public void OnCloseCreditsPressed()
    {
        AudioManager.Instance?.PlaySfx(AudioManager.SfxClip.ButtonClick);
        creditsPanel?.SetActive(false);
    }

    public void OnCloseOptionsPressed()
    {
        AudioManager.Instance?.PlaySfx(AudioManager.SfxClip.ButtonClick);
        optionsMenuManager?.CloseOptions();
    }

    public void OnOnlinePressed()
    {
        AudioManager.Instance?.PlaySfx(AudioManager.SfxClip.ButtonClick);
        var storedName = PlayerPrefs.GetString(k_PlayerNameKey, string.Empty).Trim();
        if (string.IsNullOrEmpty(storedName))
        {
            ShowUsernamePrompt();
            return;
        }

        StartOnlineSession(storedName);
    }

    void ShowUsernamePrompt()
    {
        if (usernamePromptPanel != null)
            usernamePromptPanel.SetActive(true);

        if (usernameInput != null)
            usernameInput.text = string.Empty;
    }

    public void OnUsernameSubmit()
    {
        if (usernameInput == null) return;

        AudioManager.Instance?.PlaySfx(AudioManager.SfxClip.ButtonClick);
        var name = usernameInput.text.Trim();
        if (string.IsNullOrEmpty(name))
            return;

        PlayerPrefs.SetString(k_PlayerNameKey, name);
        PlayerPrefs.Save();

        if (usernamePromptPanel != null)
            usernamePromptPanel.SetActive(false);

        StartOnlineSession(name);
    }

    void StartOnlineSession(string playerName)
    {
        PhotonNetwork.NickName = playerName;
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.GameVersion = gameVersion;
        PhotonNetwork.ConnectUsingSettings();
    }

    public void OnOptionsPressed()
    {
        AudioManager.Instance?.PlaySfx(AudioManager.SfxClip.ButtonClick);
        optionsMenuManager?.OpenOptions();
    }

    public void OnQuitPressed()
    {
        AudioManager.Instance?.PlaySfx(AudioManager.SfxClip.ButtonClick);
        Application.Quit();
    }

    public override void OnConnectedToMaster()
    {
        SceneManager.LoadScene("01_Lobby");
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined room: " + PhotonNetwork.CurrentRoom.Name);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogWarning($"Create room failed: {message}");
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogWarning($"Join room failed: {message}");
    }
}
