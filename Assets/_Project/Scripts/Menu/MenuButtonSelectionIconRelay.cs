using UnityEngine;
using UnityEngine.EventSystems;

public class MenuButtonSelectionIconRelay : MonoBehaviour, IPointerEnterHandler, ISelectHandler
{
    public MenuButtonSelectionIcon Owner;
    public int Index;

    public void OnPointerEnter(PointerEventData eventData)
    {
        Owner?.OnButtonFocusFromRelay(Index);
    }

    public void OnSelect(BaseEventData eventData)
    {
        Owner?.OnButtonFocusFromRelay(Index);
    }
}
