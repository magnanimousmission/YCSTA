using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuButtonSelectionIcon : MonoBehaviour
{
    public RectTransform Arrow;
    public Button[] Buttons;

    [SerializeField] private Vector3 arrowOffset = new Vector3(-180f, 0f, 0f);

    private int _currentIndex;

    private void Start()
    {
        if (Arrow != null)
            Arrow.gameObject.SetActive(false);

        BindRelays();
    }

    private void Update()
    {
        if (Buttons == null || Buttons.Length == 0)
            return;

        HandleKeyboardFallback();

        if (TryGetSelectedButtonIndex(out int selectedIndex))
            SetCurrentIndex(selectedIndex);
    }

    public void OnButtonFocusFromRelay(int index)
    {
        SetCurrentIndex(index);
    }

    private void HandleKeyboardFallback()
    {
        if (!Input.GetKeyDown(KeyCode.DownArrow) && !Input.GetKeyDown(KeyCode.UpArrow))
            return;

        int direction = Input.GetKeyDown(KeyCode.DownArrow) ? 1 : -1;
        int nextIndex = _currentIndex;

        for (int i = 0; i < Buttons.Length; i++)
        {
            nextIndex = (nextIndex + direction + Buttons.Length) % Buttons.Length;
            if (IsButtonUsable(nextIndex))
            {
                SetCurrentIndex(nextIndex);

                if (EventSystem.current != null && Buttons[nextIndex] != null)
                    EventSystem.current.SetSelectedGameObject(Buttons[nextIndex].gameObject);

                return;
            }
        }
    }

    private bool TryGetSelectedButtonIndex(out int index)
    {
        index = -1;

        if (EventSystem.current == null)
            return false;

        GameObject selected = EventSystem.current.currentSelectedGameObject;
        if (selected == null)
            return false;

        Button selectedButton = selected.GetComponentInParent<Button>();
        if (selectedButton == null)
            return false;

        for (int i = 0; i < Buttons.Length; i++)
        {
            if (Buttons[i] == selectedButton && IsButtonUsable(i))
            {
                index = i;
                return true;
            }
        }

        return false;
    }

    private void BindRelays()
    {
        if (Buttons == null)
            return;

        for (int i = 0; i < Buttons.Length; i++)
        {
            Button button = Buttons[i];
            if (button == null)
                continue;

            MenuButtonSelectionIconRelay relay = button.GetComponent<MenuButtonSelectionIconRelay>();
            if (relay == null)
                relay = button.gameObject.AddComponent<MenuButtonSelectionIconRelay>();

            relay.Owner = this;
            relay.Index = i;
        }
    }

    private bool IsButtonUsable(int index)
    {
        if (index < 0 || index >= Buttons.Length)
            return false;

        Button button = Buttons[index];
        if (button == null)
            return false;

        return button.gameObject.activeInHierarchy && button.interactable;
    }

    private void SetCurrentIndex(int index)
    {
        if (!IsButtonUsable(index))
            return;

        _currentIndex = index;

        if (Arrow != null && !Arrow.gameObject.activeSelf)
            Arrow.gameObject.SetActive(true);

        MoveArrow();
    }

    private void MoveArrow()
    {
        if (Arrow == null || !IsButtonUsable(_currentIndex))
            return;

        Arrow.position = Buttons[_currentIndex].transform.position + arrowOffset;
    }
}
