using UnityEngine;

public class IngredientSlot : MonoBehaviour
{
    public string flavorId;

    private void OnMouseDown()
    {
        GameManager.Instance.AddFlavor(flavorId);
    }
}