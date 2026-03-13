using UnityEngine;
using TMPro;

public class LoadingScreen : MonoBehaviour
{
    public static LoadingScreen Instance { get; private set; }

    [Header("Loading UI")]
    public GameObject loadingScreenPrefab;

    LoadingScreenUI _activeScreen;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public static void Show(string message)
    {
        if (Instance == null)
        {
            Instance = FindObjectOfType<LoadingScreen>();
            if (Instance == null)
                return;
        }

        Instance.ShowInternal(message);
    }

    public static void Hide()
    {
        if (Instance == null)
            Instance = FindObjectOfType<LoadingScreen>();

        if (Instance == null)
            return;

        Instance.HideInternal();
    }

    void ShowInternal(string message)
    {
        if (_activeScreen == null)
        {
            if (loadingScreenPrefab == null)
                return;

            var go = Instantiate(loadingScreenPrefab);
            _activeScreen = go.GetComponent<LoadingScreenUI>();

            DontDestroyOnLoad(go);
        }

        _activeScreen?.SetMessage(message);
        _activeScreen?.SetVisible(true);
    }

    void HideInternal()
    {
        if (_activeScreen == null)
            return;

        _activeScreen.SetVisible(false);
    }
}
