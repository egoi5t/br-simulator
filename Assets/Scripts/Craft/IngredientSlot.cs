using UnityEngine;
using UnityEngine.EventSystems;

public class IngredientSlot : MonoBehaviour, IPointerClickHandler
{
    public string flavorId;

    public void OnPointerClick(PointerEventData eventData)
    {
        GameManager.Instance.AddFlavor(flavorId);
    }
}