using UnityEngine;

public class IngredientSlot : MonoBehaviour
{
    public string flavorId;
    public string flavorName;

    private void OnMouseDown()
    {
        GameManager.Instance.AddFlavor(flavorId, flavorName);
    }
}