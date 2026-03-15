using System;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject pauseMenuUI;
    [SerializeField] private float pauseCooldown = 0f;
    [SerializeField] private string mainMenuSceneName = "01_Lobby";
    [SerializeField] private DeathScreenController deathScreenController;
    
    private bool _isPaused;
    private bool _isOnCooldown;
    private float _cooldownTimer;
    private CursorLockMode _previousLockMode;
    private bool _previousCursorVisible;
    private bool _isReturningToMainMenu;
    
    public static event Action<bool> OnPauseToggled;

    private void Awake()
    {
        pauseMenuUI.SetActive(false);
    }

    void Update()
    {
        if (_isOnCooldown)
        {
            _cooldownTimer -= Time.unscaledDeltaTime;
            if (_cooldownTimer <= 0f)
                _isOnCooldown = false;
        }

        if (Input.GetKeyDown(KeyCode.Escape) && !_isOnCooldown && !deathScreenController.deathPanelActive)
            TogglePause();
    }

    void TogglePause()
    {
        _isPaused = !_isPaused;
        pauseMenuUI.SetActive(_isPaused);
        SetCursorState(_isPaused);
        OnPauseToggled?.Invoke(_isPaused);

        _isOnCooldown = true;
        _cooldownTimer = pauseCooldown;
    }

    void SetCursorState(bool isPaused)
    {
        if (isPaused)
        {
            _previousLockMode = Cursor.lockState;
            _previousCursorVisible = Cursor.visible;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = _previousLockMode;
            Cursor.visible = _previousCursorVisible;
        }
    }

    public void ResumeButtonClicked()
    {
        TogglePause();
    }

    public void QuitButtonClicked()
    {
        if (_isReturningToMainMenu)
            return;
        
        _isReturningToMainMenu = true;
        
        if (PhotonNetwork.InRoom)
        {
            LoadingScreen.Show("Leaving room...");
            PhotonNetwork.LeaveRoom();
            return;
        }

        LoadMainMenu();
    }
    
    public override void OnDisconnected(DisconnectCause cause)
    {
        if (!_isReturningToMainMenu) return;
        LoadMainMenu();
    }
    
    public override void OnLeftRoom()
    {
        if (!_isReturningToMainMenu)
            return;

        AudioManager.Instance?.PlaySfx(AudioManager.SfxClip.PlayerLeaveRoomSound);

        LoadMainMenu();
    }
    
    private void LoadMainMenu()
    {
        LoadingScreen.Show("Leaving Room...");
        var asyncOp = SceneManager.LoadSceneAsync(mainMenuSceneName);
        asyncOp.completed += _ =>
        {
            _isReturningToMainMenu = false;
            LoadingScreen.Hide();
        };
    }
}