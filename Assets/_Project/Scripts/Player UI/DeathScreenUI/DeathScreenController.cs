using Photon.Pun;
using Photon.Realtime;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CanvasGroup))]
public class DeathScreenController : MonoBehaviourPunCallbacks
{
    [Header("References")]
    [SerializeField] private GameObject deathPanel;
    [SerializeField] private string mainMenuSceneName = "01_Lobby";
    [SerializeField] private GameObject spectateButton;
    
    private PlayerCore _playerCore;
    private AuraDeathTimer _auraDeathTimer;
    private PlayerDeathController _playerDeathController;
    private CursorLockMode _previousLockMode;
    private bool _previousCursorVisible;
    private bool _isReturningToMainMenu;

    [HideInInspector] public bool deathPanelActive = false;
    
    private void Awake()
    {
        deathPanel.SetActive(false);
    }

    public void Bind(AuraDeathTimer timer)
    {
        _auraDeathTimer = timer;
        _auraDeathTimer.OnTimerExpired += ActivateDeathPanel;
    }

    public void Bind(PlayerCore playerCore)
    {
        _playerCore = playerCore;
        _playerCore.OnOxygenDepleted += ActivateDeathPanel;
    }

    private void OnDisable()
    {
        if (_auraDeathTimer != null)
            _auraDeathTimer.OnTimerExpired -= ActivateDeathPanel;

        if (_playerCore != null)
            _playerCore.OnOxygenDepleted -= ActivateDeathPanel;

        if (_playerDeathController != null)
            _playerDeathController.OnNoPlayersLeftToSpectate -= QuitButtonClicked;
    }

    private void ActivateDeathPanel()
    {
        if (deathPanelActive)
            return;

        deathPanelActive = true;
        deathPanel.SetActive(true);
        UnlockCursor();
        
        _playerDeathController = _playerCore.GetComponent<PlayerDeathController>();
        _playerDeathController?.BroadcastDisablePlayerPresence();
        
        if (spectateButton != null)
            spectateButton.SetActive(SpectatorController.AnyLivingPlayersExist());

        if (_playerDeathController != null)
            _playerDeathController.OnNoPlayersLeftToSpectate += QuitButtonClicked;
        
        
        //down state
        //res state
        //death state
        
    }
    

    private void UnlockCursor()
    {
        _previousLockMode = Cursor.lockState;
        _previousCursorVisible = Cursor.visible;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void RestoreCursor()
    {
        Cursor.lockState = _previousLockMode;
        Cursor.visible = _previousCursorVisible;
    }

    public void SpectateButtonClicked()
    {
        if (!SpectatorController.AnyLivingPlayersExist())
        {
            QuitButtonClicked();
            return;
        }
        
        RestoreCursor();
        deathPanel.SetActive(false);
        deathPanelActive = false;
        _playerDeathController?.Spectate();
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

        if (PhotonNetwork.IsConnected)
        {
            LoadingScreen.Show("Disconnecting...");
            PhotonNetwork.Disconnect();
            return;
        }
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

        if (PhotonNetwork.IsConnected)
        {
            LoadingScreen.Show("Disconnecting...");
            PhotonNetwork.Disconnect();
            return;
        }

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