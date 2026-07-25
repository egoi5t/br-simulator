using UnityEngine;
using UnityEngine.UI;

public class ToolManager : MonoBehaviour
{
    public static ToolManager Instance;

    public int currentTool = 0;

    [Header("마우스를 따라다닐 도구 아이콘 (Canvas 자식으로 미리 배치)")]
    public RectTransform followingToolIcon; // 화면에 항상 있고, sprite만 바꿔치기
    public Image followingToolIconImage;

    [Header("도구 아이콘 스프라이트")]
    public Sprite scoopSprite;
    public Sprite spadeSprite;

    [Header("도구함 안의 아이콘 Image (투명도로 숨김/표시)")]
    public Image scoopIconImage;
    public Image spadeIconImage;

    private Canvas parentCanvas;

    private void Awake()
    {
        Instance = this;
        parentCanvas = followingToolIcon.GetComponentInParent<Canvas>();
        followingToolIcon.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (currentTool != 0)
        {
            // 마우스 화면 좌표를 Canvas 좌표로 변환해서 따라다니게
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentCanvas.transform as RectTransform,
                Input.mousePosition,
                parentCanvas.worldCamera,
                out localPoint);

            followingToolIcon.anchoredPosition = localPoint;
        }
    }

    public void SelectTool(int tool)
    {
        if (currentTool == tool)
        {
            PutDownTool();
            return;
        }

        currentTool = tool;
        Cursor.visible = false; // 실제 커서 숨김
        followingToolIcon.gameObject.SetActive(true);

        switch (tool)
        {
            case 1: // Scoop
                followingToolIconImage.sprite = scoopSprite;
                SetIconVisible(scoopIconImage, false);
                SetIconVisible(spadeIconImage, true);
                break;
            case 2: // Spade
                followingToolIconImage.sprite = spadeSprite;
                SetIconVisible(spadeIconImage, false);
                SetIconVisible(scoopIconImage, true);
                break;
        }

        Debug.Log("도구 선택됨: " + tool);
    }

    private void PutDownTool()
    {
        currentTool = 0;
        Cursor.visible = true; // 실제 커서 다시 보이게
        followingToolIcon.gameObject.SetActive(false);

        SetIconVisible(scoopIconImage, true);
        SetIconVisible(spadeIconImage, true);

        Debug.Log("도구 내려놓음");
    }

    private void SetIconVisible(Image icon, bool visible)
    {
        Color c = icon.color;
        c.a = visible ? 1f : 0f;
        icon.color = c;
    }

    public bool HasToolEquipped()
    {
        return currentTool != 0;
    }
}