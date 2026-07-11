using UnityEngine;

public class ContainerSelector : MonoBehaviour
{
    public string containerType; 

    private void OnMouseDown()
    {
        GameManager.Instance.SelectContainer(containerType, GetComponent<SpriteRenderer>());
    }
}