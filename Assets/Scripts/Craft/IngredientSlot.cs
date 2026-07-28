using UnityEngine;
using UnityEngine.EventSystems;

public class IngredientSlot : MonoBehaviour, IPointerClickHandler
{
    public string flavorId;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!ToolManager.Instance.HasToolEquipped())
        {
            Debug.Log("먼저 도구를 선택해주세요!");
            return;
        }

        GameManager.Instance.AddFlavor(flavorId);
    }
}