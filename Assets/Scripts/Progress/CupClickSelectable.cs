using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 선반의 컵 6개 각각에 붙이는 스크립트.
/// 클릭(터치)하면 바로 해당 사이즈가 선택된 것으로 처리.
/// DraggableCupItem.cs를 대체합니다 - 이 파일을 쓰면 그 파일은 삭제하거나 안 붙이면 됩니다.
/// </summary>
public class CupClickSelectable : MonoBehaviour, IPointerClickHandler
{
    [Tooltip("이 컵 아이콘이 어떤 사이즈인지 인스펙터에서 지정")]
    public CupSize cupSize;

    private CupSelectionSceneController controller;

    private void Awake()
    {
        controller = FindObjectOfType<CupSelectionSceneController>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (SfxManager.Instance != null) SfxManager.Instance.PlayCupSelect();
        controller.SelectCup(cupSize);
    }
}