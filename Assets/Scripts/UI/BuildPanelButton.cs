using UnityEngine;
using UnityEngine.EventSystems;

public class BuildPanelButton : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    public Machine.Type type;

    public void OnPointerClick(PointerEventData eventData) {
        BuildPanel.Instance.select(type);
    }

    public void OnPointerEnter(PointerEventData eventData) {
        BuildPanel.Instance.showMachineInfo(type);
    }
}
