using UnityEngine;

public class IngredientSlot : MonoBehaviour
{
    public string ingredientName;
    private SpriteRenderer sr;

    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    private void OnMouseDown()
    {
        GameManager.Instance.AddIngredient(ingredientName, sr.color);
    }
}