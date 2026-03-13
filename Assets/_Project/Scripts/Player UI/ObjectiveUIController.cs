using TMPro;
using UnityEngine;

/// <summary>
/// Displays the current objective in the UI.
/// </summary>
public class ObjectiveUIController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI objectiveText;
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private bool fadeInOutOnChange = true;
    [SerializeField] private float fadeDuration = 0.3f;

    private ObjectiveManager _objectiveManager;
    private float _fadeTimer;
    private bool _isFading;

    private void OnEnable()
    {
        if (_objectiveManager == null)
            _objectiveManager = ObjectiveManager.Instance;

        if (_objectiveManager != null)
        {
            _objectiveManager.OnObjectiveChanged += HandleObjectiveChanged;
            _objectiveManager.OnObjectiveCompleted += HandleObjectiveCompleted;
            RefreshDisplay();
        }
    }

    private void OnDisable()
    {
        if (_objectiveManager != null)
        {
            _objectiveManager.OnObjectiveChanged -= HandleObjectiveChanged;
            _objectiveManager.OnObjectiveCompleted -= HandleObjectiveCompleted;
        }
    }

    private void Update()
    {
        if (_isFading)
        {
            _fadeTimer += Time.deltaTime;
            if (_fadeTimer >= fadeDuration)
            {
                _isFading = false;
                if (canvasGroup != null)
                    canvasGroup.alpha = 1f;
            }
            else if (canvasGroup != null)
            {
                float progress = _fadeTimer / fadeDuration;
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, progress);
            }
        }
    }

    private void HandleObjectiveChanged(Objective objective)
    {
        if (fadeInOutOnChange)
        {
            _fadeTimer = 0f;
            _isFading = true;
            if (canvasGroup != null)
                canvasGroup.alpha = 0f;
        }

        RefreshDisplay();
    }

    private void HandleObjectiveCompleted(Objective objective)
    {
        // Optional: Show completion feedback
        // You could add a special effect, sound, or animation here
    }

    private void RefreshDisplay()
    {
        if (_objectiveManager?.CurrentObjective == null)
        {
            if (objectiveText != null)
                objectiveText.text = "All objectives complete!";
            if (progressText != null)
                progressText.text = "";
            return;
        }

        if (objectiveText != null)
            objectiveText.text = _objectiveManager.CurrentObjective.Description;

        if (progressText != null)
        {
            int current = _objectiveManager.GetCurrentObjectiveIndex() + 1;
            int total = _objectiveManager.GetObjectiveCount();
            progressText.text = $"({current}/{total})";
        }
    }
}
