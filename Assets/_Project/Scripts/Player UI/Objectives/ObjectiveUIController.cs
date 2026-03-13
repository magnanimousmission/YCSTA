using System.Collections;
using TMPro;
using UnityEngine;

public class ObjectiveUIController : MonoBehaviour
{
    [Header("Main Panel (fades in → out)")]
    [SerializeField] private CanvasGroup mainPanel;
    [SerializeField] private TextMeshProUGUI mainDetailText;
 
    [Header("Sub Panel (persistent)")]
    [SerializeField] private CanvasGroup subPanel;
    [SerializeField] private TextMeshProUGUI subDetailText;

    [Header("Fade Settings")]
    [Tooltip("Seconds for the main objective to fade IN")]
    [SerializeField] private float mainFadeInDuration = 0.5f;

    [Tooltip("Seconds for the main objective to fade OUT after its display window")]
    [SerializeField] private float mainFadeOutDuration = 1f;

    [Tooltip("Seconds to cross-fade sub-objective text when it changes")]
    [SerializeField] private float subCrossFadeDuration = 0.4f;

    private Coroutine _mainRoutine;
    private Coroutine _subRoutine;
    
    private bool _mainBannerFinished;
    private bool _mainBannerPlaying;
    
    private void Awake()
    {
        SetAlpha(mainPanel, 0f);
        SetAlpha(subPanel,  0f);
    }

    private void OnEnable()
    {
        ObjectiveManager.OnMainObjectiveSet    += HandleMainObjectiveSet;
        ObjectiveManager.OnSubObjectiveChanged  += HandleSubObjectiveChanged;
        ObjectiveManager.OnObjectiveComplete    += HandleObjectiveComplete;
        RestoreFromManager();
    }

    private void OnDisable()
    {        
        if (_mainRoutine != null) { StopCoroutine(_mainRoutine); _mainRoutine = null; }
        if (_subRoutine  != null) { StopCoroutine(_subRoutine);  _subRoutine  = null; }
        
        ObjectiveManager.OnMainObjectiveSet    -= HandleMainObjectiveSet;
        ObjectiveManager.OnSubObjectiveChanged  -= HandleSubObjectiveChanged;
        ObjectiveManager.OnObjectiveComplete    -= HandleObjectiveComplete;
    }
    
    private void RestoreFromManager()
    {
        ObjectiveManager manager = ObjectiveManager.Instance;
        if (manager == null || manager.CurrentObjective == null) return;
 
        ObjectiveData data = manager.CurrentObjective;
 
        if (!_mainBannerFinished)
        {

            if (_mainRoutine != null) StopCoroutine(_mainRoutine);
            _mainRoutine = StartCoroutine(ShowMainPanel(data.mainObjectiveText, data.mainDisplayDuration));
        }
        else
        {
            SetAlpha(mainPanel, 0f);
            RestoreSubInstant(data);
        }
    }
    
    private void HandleMainObjectiveSet(ObjectiveData data)
    {
        _mainBannerFinished = false;
        if (_mainRoutine != null) StopCoroutine(_mainRoutine);
        _mainRoutine = StartCoroutine(ShowMainPanel(data.mainObjectiveText, data.mainDisplayDuration));
    }
 
    private void HandleSubObjectiveChanged(string subText)
    {
        if (_mainBannerPlaying) return;
        
        if (_subRoutine != null) StopCoroutine(_subRoutine);
 
        if (string.IsNullOrEmpty(subText))
            _subRoutine = StartCoroutine(FadeOutPanel(subPanel, subCrossFadeDuration));
        else
            _subRoutine = StartCoroutine(CrossFadeSubPanel(subText));
    }
 
    private void HandleObjectiveComplete(ObjectiveData data)
    {
        Debug.Log($"[ObjectiveUI] Complete: {data.mainObjectiveText}");
    }
    
    private IEnumerator ShowMainPanel(string titleText, float holdDuration)
    {
        _mainBannerPlaying = true;
        
        if (_subRoutine != null) { StopCoroutine(_subRoutine); _subRoutine = null; }
        SetAlpha(subPanel, 0f);
 
        mainDetailText.text = titleText;
 
        yield return StartCoroutine(FadeInPanel(mainPanel, mainFadeInDuration));
        yield return new WaitForSeconds(holdDuration);
        yield return StartCoroutine(FadeOutPanel(mainPanel, mainFadeOutDuration));
 
        _mainBannerPlaying = false;
        _mainBannerFinished = true;
        _mainRoutine = null;
        
        ObjectiveManager manager = ObjectiveManager.Instance;
        if (manager?.CurrentObjective != null)
        {
            string sub = manager.CurrentObjective.CurrentSubObjectiveText;
            if (!string.IsNullOrEmpty(sub))
                _subRoutine = StartCoroutine(CrossFadeSubPanel(sub));
        }
    }
    
    private IEnumerator CrossFadeSubPanel(string newText)
    {
        float half = subCrossFadeDuration * 0.5f;
 
        if (subPanel.alpha > 0.01f)
            yield return StartCoroutine(FadeOutPanel(subPanel, half));
 
        subDetailText.text = newText;
        yield return StartCoroutine(FadeInPanel(subPanel, half));
 
        _subRoutine = null;
    }
    
    private void RestoreSubInstant(ObjectiveData data)
    {
        string sub = data.CurrentSubObjectiveText;
        if (!string.IsNullOrEmpty(sub))
        {
            subDetailText.text = sub;
            SetAlpha(subPanel, 1f);
        }
        else
        {
            SetAlpha(subPanel, 0f);
        }
    }
 
    private IEnumerator FadeInPanel(CanvasGroup cg, float duration)
    {
        float t = 0f;
        float start = cg.alpha;
        while (t < duration)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(start, 1f, t / duration);
            yield return null;
        }
        cg.alpha = 1f;
    }
 
    private IEnumerator FadeOutPanel(CanvasGroup cg, float duration)
    {
        float t = 0f;
        float start = cg.alpha;
        while (t < duration)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(start, 0f, t / duration);
            yield return null;
        }
        cg.alpha = 0f;
    }
 
    private static void SetAlpha(CanvasGroup cg, float a)
    {
        if (cg != null) cg.alpha = a;
    }
}