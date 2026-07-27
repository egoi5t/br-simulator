using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 쇼핑백 오브젝트에 붙이는 스크립트.
/// 클릭하면 PackagingSceneController.TryPackageIntoBag()을 호출.
/// 뚜껑과 달리 사이즈 구분이 필요 없어서(쇼핑백은 하나) LidClickable보다 단순함.
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
