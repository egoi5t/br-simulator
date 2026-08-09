using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 쇼핑백 무더기(선반)에 붙이는 스크립트. 컵 선반의 CupClickSelectable과 같은 패턴.
/// 클릭하면 포장 완료 처리가 아니라, 책상(아이스크림 옆)에 "실제 쇼핑백"을 소환함.
/// 포장 완료는 그 소환된 쇼핑백 위로 아이스크림을 드래그해야(DraggableIceCream.cs) 처리됨.
/// </summary>
public class BagClickable : MonoBehaviour, IPointerClickHandler
{
    private PackagingSceneController controller;

    private void Awake()
    {
        controller = FindObjectOfType<PackagingSceneController>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (controller == null)
        {
            Debug.LogWarning("BagClickable: PackagingSceneController를 씬에서 찾지 못했습니다.");
            return;
        }

        // 1차(용기 선택) 화면에서는 아직 완성된 아이스크림이 없어서 쇼핑백을 소환하면 안 됨
        if (CraftResultSession.Instance.ContainerIndex <= 0)
        {
            if (WarningPopupEffect.Instance != null)
                WarningPopupEffect.Instance.PlayWarningAtMouse("아직 포장할 게 없어요!");
            Debug.LogWarning("BagClickable: 아직 화면②에서 완성된 아이스크림이 없어 쇼핑백을 소환할 수 없습니다.");
            return;
        }

        if (SfxManager.Instance != null) SfxManager.Instance.PlayBagSelect();
        controller.SpawnBagNextToIceCream();
    }
}