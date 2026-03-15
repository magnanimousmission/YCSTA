using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class ExtractionUIController : MonoBehaviourPunCallbacks
{
    [SerializeField] private string mainMenuSceneName = "01_Lobby";
    
    [Header("Panels")]
    [SerializeField] private GameObject deathPanel;
    [SerializeField] private CanvasGroup extractionPanel;
    [SerializeField] private CanvasGroup extractionDetailPanel;
    [SerializeField] private CanvasGroup quitButtonPanel;

    [Header("Fade Settings")]
    [SerializeField] private float fadeInDuration = 0.8f;
    [SerializeField] private float delayBetweenPanels = 0.4f;

    private PlayerCore _localPlayer;
    private bool _isReturningToMainMenu;
    
    private CursorLockMode _previousLockMode;
    private bool _previousCursorVisible;
    
    private void Awake()
    {
        deathPanel.SetActive(false);
    }

    public void ActivateExtractionUI()
    {
        deathPanel.SetActive(true);
        UnlockCursor();

        SetAlpha(extractionPanel, 0f);
        SetAlpha(extractionDetailPanel, 0f);
        SetAlpha(quitButtonPanel, 0f);

        PlaySequence();
    }
    
    private void PlaySequence()
    {
        float delay = 0f;

        FadeIn(extractionPanel, delay);
        delay += fadeInDuration + delayBetweenPanels;

        FadeIn(extractionDetailPanel, delay);
        delay += fadeInDuration + delayBetweenPanels;
        
        FadeIn(quitButtonPanel, delay);
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

    private void FadeIn(CanvasGroup group, float delay)
    {
        group.DOFade(1f, fadeInDuration)
            .SetDelay(delay)
            .SetUpdate(true);
    }

    private void SetAlpha(CanvasGroup group, float alpha)
    {
        if (group != null) group.alpha = alpha;
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

        AudioManager.Instance?.PlaySfx(AudioManager.SfxClip.PlayerLeaveRoomSound);

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
        RestoreCursor();
        LoadingScreen.Show("Leaving Room...");
        var asyncOp = SceneManager.LoadSceneAsync(mainMenuSceneName);
        asyncOp.completed += _ =>
        {
            _isReturningToMainMenu = false;
            LoadingScreen.Hide();
        };
    }
}