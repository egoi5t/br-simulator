using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

// 손님 프리팹 안에 함께 두는 투명 UI (Image, Raycast Target 켜짐, Color Alpha 0).
// CustomerRoot의 월드 스페이스 캔버스(말풍선이랑 같은 캔버스여도 됨) 하위에 두면 됨.
// CustomerView가 스폰 시점에 이 드롭존의 콜백을 연결해줌 - 인스펙터에서 직접 연결할 필요 없음.
public class CustomerDropZone : MonoBehaviour, IDropHandler
{
    private UnityAction onDelivered;

    public void SetDeliveryCallback(UnityAction callback)
    {
        onDelivered = callback;
    }

    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log("[CustomerDropZone] OnDrop 호출됨");

        GameObject dropped = eventData.pointerDrag;
        if (dropped == null)
        {
            Debug.LogWarning("[CustomerDropZone] eventData.pointerDrag가 null입니다.");
            return;
        }

        DeliveryBagItem draggable = dropped.GetComponent<DeliveryBagItem>();
        if (draggable == null)
        {
            Debug.LogWarning($"[CustomerDropZone] 드롭된 오브젝트({dropped.name})에 DeliveryBagItem이 없습니다.");
            return; // 아이스크림이 아닌 다른 걸 떨어뜨린 경우 무시
        }

        draggable.MarkDelivered();
        onDelivered?.Invoke();
    }
}