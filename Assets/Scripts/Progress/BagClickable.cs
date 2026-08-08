using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 쇼핑백 오브젝트에 붙이는 스크립트.
/// 클릭하면 PackagingSceneController.TryPackageIntoBag()을 호출해서 포장을 완료함
/// (뚜껑 덮힌 → 쇼핑백 담긴 이미지로 교체, 평가 시스템 실행).
/// ⚠️ 다음 씬으로 넘어가는 건 여기서 하지 않음 - "체크아웃" 버튼을 눌러야 넘어감.
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

        controller.TryPackageIntoBag(transform as RectTransform);
    }
}