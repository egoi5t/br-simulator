using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 게임 시작 시 LidArea 밑의 LidClickable들을 찾아서,
/// 각자의 lidSize에 맞는 스프라이트를 CupVisualData에서 가져와 적용.
/// CupVisualApplier와 구조 동일 (선반 컵 버전의 뚜껑 버전).
/// </summary>
public class LidVisualApplier : MonoBehaviour
{
    public CupVisualData visualData;
    public Transform lidArea;

    private void Start()
    {
        ApplyLidSprites();
    }

    private void ApplyLidSprites()
    {
        if (visualData == null || lidArea == null)
        {
            Debug.LogWarning("LidVisualApplier: Visual Data 또는 Lid Area가 연결되지 않았습니다.");
            return;
        }

        var lids = lidArea.GetComponentsInChildren<LidClickable>();

        foreach (var lid in lids)
        {
            var entry = visualData.GetEntry(lid.lidSize);
            if (entry == null || entry.lidStackSprite == null)
                continue;

            var image = lid.GetComponent<Image>();
            if (image != null)
                image.sprite = entry.lidStackSprite;
        }
    }
}
