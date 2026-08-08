using UnityEngine;
using TMPro;

public class TooltipManager : MonoBehaviour
{
    public static TooltipManager Instance;

    [Header("툴팁 UI")]
    public GameObject tooltipRoot; // TooltipBackground
    public TMP_Text tooltipText;
    public RectTransform tooltipRect;

    [Header("마우스 기준 오프셋")]
    public Vector2 offset = new Vector2(20, 20);

    private void Awake()
    {
        Instance = this;
        tooltipRoot.SetActive(false);
    }

    private void Update()
    {
        if (tooltipRoot.activeSelf)
        {
            tooltipRect.position = (Vector2)Input.mousePosition + offset;
        }
    }

    public void ShowTooltip(string text)
    {
        tooltipText.text = text;
        tooltipRoot.SetActive(true);
    }

    public void HideTooltip()
    {
        tooltipRoot.SetActive(false);
    }
}