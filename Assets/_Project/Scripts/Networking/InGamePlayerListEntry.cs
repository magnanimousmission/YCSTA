using TMPro;
using UnityEngine;

public class InGamePlayerListEntry : MonoBehaviour
{
    [SerializeField] private TMP_Text playerNameText;

    private void Awake()
    {
        if (playerNameText == null)
            playerNameText = GetComponentInChildren<TMP_Text>(true);
    }

    public void SetPlayerName(string playerName)
    {
        if (playerNameText == null)
            return;

        playerNameText.text = playerName;
    }
}
