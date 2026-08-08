using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 화면②에서 받은 "어떤 맛을 담았는지" 데이터(OrderSession.FilledFlavorIds)만 가지고
/// 화면③ 포장 씬에서 직접 완성된 컵 이미지를 합성해서 보여줌.
/// 화면 캡처 없이, 맛의 색상(FlavorData.colorHex)만으로 표현하기 때문에
/// 화면②와 똑같은 아트 에셋을 따로 준비할 필요가 없음.
///
/// - 잇코/니코(1~2개 스쿱): 슬롯들을 작은 원으로 배치
/// - 산코~록코(3~6개 스쿱): 부채꼴로 나눠서 색칠 (ContainerVisual.cs의 웨지 방식과 동일한 원리)
///
/// 인스펙터 설정:
/// - Container Base Image: 빈 용기 베이스 (CupVisualData의 tableCupSprite가 적용될 Image)
/// - Flavor Slot Images: 맛 색을 입힐 원형 Image 6개. 전부 같은 흰색 원형 스프라이트 사용,
///   Container Base Image와 같은 크기로 중앙에 겹쳐서 배치해두면 됨.
/// </summary>
public class FilledCupVisualComposer : MonoBehaviour
{
    [Header("베이스 컵")]
    public Image containerBaseImage;

    [Header("맛 슬롯 (최대 6개, 흰색 원형 스프라이트에 색만 입힘)")]
    public Image[] flavorSlotImages;

    [Header("스쿱/부채꼴 부모 분리")]
    [Tooltip("스쿱 스타일(잇코/니코)일 때 슬롯들이 들어갈 부모. 원근 눌림 없이 그대로.")]
    public Transform scoopSlotsParent;
    [Tooltip("부채꼴 스타일(산코~록코)일 때 슬롯들이 들어갈 부모. Scale Y를 줄여서 타원처럼 보이게 하는 그 오브젝트(FlavorDiscRoot).")]
    public Transform wedgeSlotsParent;

    [Header("사이즈별 이미지 데이터")]
    public CupVisualData visualData;

    [Header("맛별 실제 아트 데이터")]
    public FlavorVisualData flavorVisualData;

    [Header("비주얼 다듬기")]
    [Tooltip("부채꼴 조각 사이 틈 (도 단위). 0이면 완전히 붙어있음")]
    public float wedgeGapDegrees = 5f;
    [Tooltip("스쿱(잇코/니코)이 살짝 어긋나 보이게 하는 오프셋 거리(px)")]
    public float scoopOffsetDistance = 14f;

    [Header("에디터 미리보기 (Play 안 눌러도 확인용)")]
    [Tooltip("체크하면 아래 설정으로 미리보기가 표시됨. 값 바꿀 때마다 자동 갱신됨")]
    public bool livePreview = false;
    public CupSize previewSize = CupSize.Sanko;
    [Range(1, 6)] public int previewFlavorCount = 3;

    /// <summary>스쿱 1개일 때 오프셋 (중앙 그대로)</summary>
    private static readonly Vector2[] ScoopOffsetsForOne = { Vector2.zero };

    /// <summary>스쿱 2개일 때 오프셋 (좌우로 평행하게 벌어지게)</summary>
    private static readonly Vector2[] ScoopOffsetDirectionsForTwo =
    {
        new Vector2(-1f, 0f),
        new Vector2(1f, 0f)
    };

    /// <summary>포장 모드 진입 시 호출. 실제 데이터(CraftResultSession)로 합성.</summary>
    public void Compose()
    {
        int containerIndex = CraftResultSession.Instance.ContainerIndex;
        List<string> flavorIds = CraftResultSession.Instance.FlavorIds;

        if (containerIndex <= 0)
        {
            Debug.LogWarning("FilledCupVisualComposer: CraftResultSession.ContainerIndex가 비어있어 합성할 수 없습니다. " +
                              "화면②에서 CraftResultSession.Instance.SetResult()를 호출했는지 확인하세요.");
            return;
        }

        CupSize size = (CupSize)(containerIndex - 1); // 1~6 -> Ikko(0)~Rokko(5)
        ComposeWithData(size, flavorIds);
    }

    /// <summary>
    /// Play 안 해도 Inspector에서 값 바꾸는 즉시 확인할 수 있는 미리보기.
    /// Preview Size / Preview Flavor Count 기준으로, FlavorVisualData에 등록된
    /// 맛들 중 앞에서부터 필요한 개수만큼 뽑아서 더미로 그려봄.
    /// 우클릭(컴포넌트 톱니 아이콘) → Preview Compose Now 로도 수동 실행 가능.
    /// </summary>
    [ContextMenu("Preview Compose Now")]
    public void ComposePreview()
    {
        if (flavorVisualData == null || flavorVisualData.entries == null || flavorVisualData.entries.Length == 0)
        {
            Debug.LogWarning("FilledCupVisualComposer: 미리보기 하려면 Flavor Visual Data에 최소 1개 이상 등록되어 있어야 합니다.");
            return;
        }

        var dummyFlavorIds = new List<string>();
        int count = Mathf.Min(previewFlavorCount, flavorVisualData.entries.Length);
        for (int i = 0; i < count; i++)
        {
            dummyFlavorIds.Add(flavorVisualData.entries[i].flavorId);
        }

        ComposeWithData(previewSize, dummyFlavorIds);
    }

    /// <summary>Compose()/ComposePreview()가 공통으로 쓰는 실제 합성 로직.</summary>
    private void ComposeWithData(CupSize size, List<string> flavorIds)
    {
        // 베이스 컵 이미지 적용
        var entry = visualData != null ? visualData.GetEntry(size) : null;
        if (entry != null && entry.tableCupSprite != null && containerBaseImage != null)
        {
            containerBaseImage.sprite = entry.tableCupSprite;
        }

        if (flavorIds == null || flavorIds.Count == 0)
        {
            Debug.LogWarning("FilledCupVisualComposer: FlavorIds가 비어있습니다.");
            HideAllSlots();
            return;
        }

        if (flavorSlotImages == null || flavorSlotImages.Length == 0)
        {
            Debug.LogWarning("FilledCupVisualComposer: Flavor Slot Images가 비어있습니다.");
            return;
        }

        int n = flavorIds.Count;
        bool isScoopStyle = (int)size <= 1; // Ikko(0), Niko(1) - 작은 원 스쿱 스타일

        for (int i = 0; i < flavorSlotImages.Length; i++)
        {
            if (flavorSlotImages[i] == null) continue;

            if (i >= n)
            {
                flavorSlotImages[i].gameObject.SetActive(false);
                continue;
            }

            var slot = flavorSlotImages[i];
            slot.gameObject.SetActive(true);

            var flavorEntry = flavorVisualData != null ? flavorVisualData.GetEntry(flavorIds[i]) : null;

            if (isScoopStyle)
            {
                // 스쿱 스타일: 원근 눌림 없는 부모로 옮김 (이미 완성된 그림이라 추가로 누르면 안 됨)
                if (scoopSlotsParent != null && slot.transform.parent != scoopSlotsParent)
                    slot.transform.SetParent(scoopSlotsParent, worldPositionStays: false);

                slot.type = Image.Type.Simple;
                slot.fillAmount = 1f;
                slot.rectTransform.localEulerAngles = Vector3.zero;

                if (flavorEntry != null && flavorEntry.scoopSprite != null)
                {
                    slot.sprite = flavorEntry.scoopSprite;
                    slot.color = Color.white; // 원본 이미지 색 그대로
                }
                else
                {
                    slot.color = GetFallbackColor(flavorIds[i]);
                }

                Vector2 offset = Vector2.zero;
                if (n == 1)
                {
                    offset = ScoopOffsetsForOne[0];
                }
                else if (n == 2 && i < ScoopOffsetDirectionsForTwo.Length)
                {
                    offset = ScoopOffsetDirectionsForTwo[i] * scoopOffsetDistance;
                }
                slot.rectTransform.anchoredPosition = offset;
            }
            else
            {
                // 부채꼴 스타일: 원근 눌림(타원 느낌) 적용된 부모(FlavorDiscRoot)로 옮김
                if (wedgeSlotsParent != null && slot.transform.parent != wedgeSlotsParent)
                    slot.transform.SetParent(wedgeSlotsParent, worldPositionStays: false);

                slot.type = Image.Type.Filled;
                slot.fillMethod = Image.FillMethod.Radial360;
                slot.fillOrigin = (int)Image.Origin360.Top;
                slot.fillClockwise = true;

                if (flavorEntry != null && flavorEntry.discSprite != null)
                {
                    slot.sprite = flavorEntry.discSprite;
                    slot.color = Color.white;
                }
                else
                {
                    slot.color = GetFallbackColor(flavorIds[i]);
                }

                float fullSlice = 1f / n;
                float gapFraction = wedgeGapDegrees / 360f;
                slot.fillAmount = Mathf.Max(0.01f, fullSlice - gapFraction);

                slot.rectTransform.anchoredPosition = Vector2.zero;
                slot.rectTransform.localEulerAngles = new Vector3(0f, 0f, -i * (360f / n));
            }
        }
    }

#if UNITY_EDITOR
    /// <summary>Inspector에서 값이 바뀔 때마다 자동 호출됨 (에디터 전용). Live Preview 켜져 있으면 즉시 다시 그림.</summary>
    private void OnValidate()
    {
        if (!livePreview) return;

        // 컴파일 도중이나 값 초기화 시점에 바로 실행하면 에러 날 수 있어서 다음 프레임으로 미룸
        UnityEditor.EditorApplication.delayCall += () =>
        {
            if (this != null && livePreview) ComposePreview();
        };
    }
#endif

    /// <summary>합성된 맛 비주얼(스쿱/부채꼴)을 전부 숨김. 뚜껑 덮일 때 외부에서 호출.</summary>
    public void HideAllSlots()
    {
        foreach (var slot in flavorSlotImages)
        {
            if (slot != null) slot.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// FlavorVisualData에서 해당 맛의 이미지를 못 찾았을 때만 쓰는 비상용 색상.
    /// 정상적으로는 안 쓰이는 게 맞고, 이 색이 보이면 FlavorVisualData에 해당 맛이
    /// 등록 안 됐다는 뜻이니 확인 필요.
    /// </summary>
    private static readonly Dictionary<string, Color32> flavorColors = new Dictionary<string, Color32>
    {
        { "FLV-001", new Color32(140, 198, 140, 255) }, // Mint Chocolate Chip
        { "FLV-002", new Color32(120, 80, 60, 255) },   // Puss in Boots (초코 베이스 추정)
        { "FLV-003", new Color32(200, 190, 230, 255) }, // Shooting Star
        { "FLV-004", new Color32(210, 170, 130, 255) }, // Almond Bon Bon
        { "FLV-005", new Color32(245, 230, 200, 255) }, // New York Cheesecake
        { "FLV-006", new Color32(200, 60, 90, 255) },   // Cherry Jubilee
        { "FLV-007", new Color32(230, 130, 150, 255) }, // Very Berry Strawberry
        { "FLV-008", new Color32(255, 200, 200, 255) }, // Rainbow Sherbet
        { "FLV-009", new Color32(90, 60, 50, 255) },    // Chocolate Mousse
        { "FLV-010", new Color32(230, 220, 210, 255) }, // Cookies 'n Cream
        { "FLV-011", new Color32(210, 150, 220, 255) }, // Love Potion #31
        { "FLV-012", new Color32(250, 190, 220, 255) }, // Cotton Candy Wonderland
        { "FLV-013", new Color32(150, 110, 80, 255) },  // Jamoca Almond Fudge
        { "FLV-014", new Color32(150, 190, 140, 255) }, // Green Tea
        { "FLV-015", new Color32(170, 190, 130, 255) }, // Pistachio Almond
        { "FLV-016", new Color32(220, 180, 170, 255) }, // Pralines 'n Cream
        { "FLV-017", new Color32(220, 130, 140, 255) }, // Twinberry Cheesecake
        { "FLV-018", new Color32(250, 245, 230, 255) }, // Vanilla
        { "FLV-019", new Color32(100, 65, 45, 255) },   // Chocolate
        { "FLV-020", new Color32(240, 240, 225, 255) }, // 31 Yogurt
    };

    private Color GetFallbackColor(string flavorId)
    {
        if (flavorColors.TryGetValue(flavorId, out var color))
            return color;

        int hash = flavorId.GetHashCode();
        float h = Mathf.Abs(hash % 360) / 360f;
        return Color.HSVToRGB(h, 0.45f, 0.9f);
    }
}