using UnityEngine;
using TMPro;

public class LoadingScreenUI : MonoBehaviour
{
    [SerializeField] CanvasGroup canvasGroup;
    [SerializeField] TMP_Text messageText;

    void Reset()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        messageText = GetComponentInChildren<TMP_Text>();
    }

    public void SetMessage(string message)
    {
        if (messageText != null)
            messageText.text = message;
    }

    public void SetVisible(bool visible)
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.interactable = visible;
            canvasGroup.blocksRaycasts = visible;
            return;
        }

        gameObject.SetActive(visible);
    }
}
