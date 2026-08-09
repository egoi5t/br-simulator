using UnityEngine;
using UnityEngine.EventSystems;

// 완성된 아이스크림(쇼핑백) 프리팹에 붙이는 스크립트.
// 프리팹 구조 예시:
// IceCreamBag (RectTransform + CanvasGroup + Image(쇼핑백 스프라이트) + 이 스크립트)
[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(CanvasGroup))]
public class DeliveryBagItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Canvas rootCanvas;
    private Vector2 startAnchoredPos;
    private bool isDelivered = false;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        rootCanvas = GetComponentInParent<Canvas>();
    }

    void OnEnable()
    {
        startAnchoredPos = rectTransform.anchoredPosition;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("[DeliveryBagItem] OnBeginDrag");
        // 드래그 중엔 자기 자신이 레이캐스트를 가려서 드롭존이 감지 못 하는 일이 없도록 잠깐 끔
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        float scale = rootCanvas != null ? rootCanvas.scaleFactor : 1f;
        rectTransform.anchoredPosition += eventData.delta / scale;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log($"[DeliveryBagItem] OnEndDrag, isDelivered={isDelivered}");
        canvasGroup.blocksRaycasts = true;

        if (isDelivered)
        {
            // CustomerDropZone.OnDrop()에서 이미 MarkDelivered()를 호출한 상태 - 손에서 사라짐
            Destroy(gameObject);
            return;
        }

        // 빈 곳에 놓쳤으면 원래 자리로 되돌림
        rectTransform.anchoredPosition = startAnchoredPos;
    }

    // CustomerDropZone이 드롭을 감지했을 때 호출
    public void MarkDelivered()
    {
        isDelivered = true;
    }
}