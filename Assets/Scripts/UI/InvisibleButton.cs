using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InvisibleButton : RaycastTarget, IPointerClickHandler
{
    public Button.ButtonClickedEvent onClick;

    public void OnPointerClick(PointerEventData eventData) {
        onClick.Invoke();
    }
}
