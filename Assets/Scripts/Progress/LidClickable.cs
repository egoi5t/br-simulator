using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 뚜껑 스택 6개(잇코~록코) 각각에 붙이는 스크립트.
/// CupClickSelectable과 구조 동일 - 자기 사이즈를 갖고 있다가 클릭되면
/// 컨트롤러에 "이 사이즈 뚜껑을 골랐다"고 알림.
/// 정답/오답 판정은 PackagingSceneController.SelectLid()에서 처리.
/// </summary>
public class LidClickable : MonoBehaviour, IPointerClickHandler
{
    [Tooltip("이 뚜껑 아이콘이 어떤 사이즈용인지 인스펙터에서 지정")]
    public CupSize lidSize;

    private PackagingSceneController controller;

    private void Awake()
    {
        controller = FindObjectOfType<PackagingSceneController>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (controller == null)
        {
            Debug.LogWarning("LidClickable: PackagingSceneController를 씬에서 찾지 못했습니다.");
            return;
        }

        controller.SelectLid(lidSize, transform as RectTransform);
    }
}