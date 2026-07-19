using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 게임 시작 시 ShelfArea 밑의 CupClickSelectable들을 찾아서,
/// CupVisualData에 등록된 스프라이트를 자동으로 적용합니다.
///
/// 인스펙터 설정:
/// - Visual Data: 위에서 만든 CupVisualData 에셋 연결
/// - Shelf Area: Hierarchy의 ShelfArea 오브젝트 연결
///
/// 나중에 진짜 아트 에셋이 나오면 CupVisualData 안의 스프라이트만 바꾸면
/// 이 스크립트가 자동으로 반영해줍니다. 씬의 개별 오브젝트를 손댈 필요 없음.
/// </summary>
public class CupVisualApplier : MonoBehaviour
{
    public CupVisualData visualData;
    public Transform shelfArea;

    private void Start()
    {
        ApplyShelfSprites();
    }

    private void ApplyShelfSprites()
    {
        if (visualData == null || shelfArea == null)
        {
            Debug.LogWarning("CupVisualApplier: Visual Data 또는 Shelf Area가 연결되지 않았습니다.");
            return;
        }

        var selectables = shelfArea.GetComponentsInChildren<CupClickSelectable>();

        foreach (var selectable in selectables)
        {
            var entry = visualData.GetEntry(selectable.cupSize);
            if (entry == null || entry.shelfStackSprite == null)
                continue;

            var image = selectable.GetComponent<Image>();
            if (image != null)
                image.sprite = entry.shelfStackSprite;
        }
    }
}
