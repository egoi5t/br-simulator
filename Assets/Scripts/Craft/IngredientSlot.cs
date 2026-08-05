using UnityEngine;
using UnityEngine.EventSystems;

public class IngredientSlot : MonoBehaviour, IPointerClickHandler
{
    public string flavorId;
    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        GameManager.Instance.AddFlavor(flavorId, rectTransform);
    }
}