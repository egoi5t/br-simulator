using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 완성된 아이스크림(전체를 감싸는 부모, 예: IceCreamVisualRoot)을 드래그해서,
/// 쇼핑백 무더기 클릭으로 책상에 소환된 쇼핑백 위에 놓으면 포장 완료 처리.
/// 쇼핑백은 고정 오브젝트가 아니라 BagClickable 클릭 시 동적으로 소환되는 것이라서,
/// 드롭존은 PackagingSceneController.ActiveBagDropZone을 그때그때 읽어서 씀.
///
/// 인스펙터 설정은 따로 없음 - 베이스 컵 이미지 + 맛 조각들을 전부 감싸는
/// 부모 오브젝트에 이 스크립트만 붙이면 됨 (그래야 드래그 시 전부 같이 움직임).
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class DraggableIceCream : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private Vector2 originalPosition;
    private Canvas canvas;
    private PackagingSceneController controller;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        controller = FindObjectOfType<PackagingSceneController>();
        originalPosition = rectTransform.anchoredPosition;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        transform.SetAsLastSibling(); // 드래그 중엔 다른 UI 위에 보이도록
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (canvas == null) return;
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        RectTransform dropZone = controller != null ? controller.ActiveBagDropZone : null;

        if (dropZone == null)
        {
            // 아직 쇼핑백을 소환 안 한 상태에서 드래그한 경우
            if (WarningPopupEffect.Instance != null)
                WarningPopupEffect.Instance.PlayWarningAtMouse("먼저 쇼핑백을 소환하세요!");
            rectTransform.anchoredPosition = originalPosition;
            return;
        }

        bool droppedOnBag = RectTransformUtility.RectangleContainsScreenPoint(
            dropZone, eventData.position, eventData.pressEventCamera);

        if (droppedOnBag && controller != null)
        {
            // 놓기 전에 미리 준비됐는지 확인 (성공할지 예측)
            bool willSucceed = controller.IsReadyForBag;

            // 뚜껑 안 덮이는 등 준비 안 됐으면 TryPackageIntoBag 내부에서 알아서 경고 처리
            controller.TryPackageIntoBag(dropZone);

            if (willSucceed)
            {
                // 성공한 경우: 원래 자리로 되돌리지 않고 놓인 자리(쇼핑백 위)에 그대로 둠.
                // 잠시 후 PackagingSceneController가 알아서 숨겨줌 (원래 자리로 순간이동했다가
                // 사라지는 것처럼 보이지 않게 하기 위함)
                return;
            }
        }

        // 실패했거나(뚜껑 미완료 등) 쇼핑백 밖에 놓은 경우만 원래 자리로 되돌림
        rectTransform.anchoredPosition = originalPosition;
    }
}