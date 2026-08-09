using UnityEngine;
using UnityEngine.EventSystems;

public class IngredientSlot : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public string flavorId;
    public string flavorName; // 표시할 이름 (영어 또는 나중에 한글로 교체)

    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        GameManager.Instance.AddFlavor(flavorId, rectTransform);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        TooltipManager.Instance.ShowTooltip(flavorName);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipManager.Instance.HideTooltip();
    }
}