using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

// =============================================================
//  ContainerVisual  —  용기 + 맛 시각화 (화면② 담기)
//
//  [변경 이력]
//  2026-07-21 (기획 변경: 탑다운 리소스 축소)
//    - 기존 02_flavor_overlays(용기6×20맛×슬롯 = 420장) 방식 폐기.
//    - 새 구조(Assets/Art/CraftTopDown)로 경로 전면 교체:
//        · 잇코/니코(스쿱): containers/cup_0N_이름_open.png + scoop_overlays/…/posK_…
//        · 산코~록코(웨지): containers/cup_0N_이름_flat.png + flavor_discs/disc_…
//    - 웨지 용기는 맛별 웨지 이미지를 따로 두지 않고,
//      flavor_disc(꽉 찬 원형) 1장을 Image의 Filled(Radial360)로 잘라
//      슬롯 개수(n)만큼 1/n 조각씩 회전 배치한다. → 리소스 20장으로 축소.
//    - CupSelection(최보광) 쪽 리소스는 건드리지 않음(옛 IceCreamCups 일부 유지).
//
//  ※ 웨지 정렬 주의: flavorSlotImages 각 Image는 용기와 같은 크기로
//     '중앙에 겹쳐서'(pivot 0.5,0.5) 배치해야 회전이 맞습니다.
// =============================================================

public class ContainerVisual : MonoBehaviour
{
    [Header("용기 UI 오브젝트")]
    public Image containerBaseImage;      // 빈 용기 베이스
    public Image[] flavorSlotImages;      // 맛 슬롯(최대 6). 웨지 회전을 위해 용기와 동일 크기·중앙 겹침 권장

    private int currentContainerIndex;    // 1=잇코 … 6=록코 (= 담을 수 있는 맛 개수)

    // 용기 코드명(폴더/파일)
    private readonly string[] containerCodeNames =
        { "ikko", "niko", "sanko", "yonko", "goko", "rokko" };

    // 2026-07-21: 새 아트 루트
    private const string basePath = "Assets/Art/CraftTopDown";

    private void Awake()
    {
        containerBaseImage.gameObject.SetActive(false);
        foreach (var slot in flavorSlotImages)
            if (slot != null) slot.gameObject.SetActive(false);
    }

    public void SetContainer(int containerIndex)
    {
        currentContainerIndex = containerIndex;
        string code = containerCodeNames[containerIndex - 1];

        // 2026-07-21: 잇코/니코 = _open(열린 컵), 산코~록코 = _flat(탑다운 웨지)
        string suffix = (containerIndex <= 2) ? "open" : "flat";
        string path = $"{basePath}/containers/cup_{containerIndex:00}_{code}_{suffix}.png";

        Sprite baseSprite = LoadSpriteAtPath(path);
        if (baseSprite == null)
            Debug.LogError("용기 베이스 이미지를 못 찾았습니다: " + path);

        containerBaseImage.sprite = baseSprite;
        containerBaseImage.gameObject.SetActive(true);

        // 모든 슬롯 초기화(직전 주문의 Filled/회전 상태 제거)
        foreach (var slot in flavorSlotImages)
        {
            if (slot == null) continue;
            ResetSlot(slot);
            slot.gameObject.SetActive(false);
        }
    }

    public void ApplyFlavor(int slotOrder, string flavorId, string flavorName)
    {
        string code = containerCodeNames[currentContainerIndex - 1];
        Image targetSlot = flavorSlotImages[slotOrder - 1];

        if (currentContainerIndex <= 2)
        {
            // ---- 잇코/니코: 스쿱(볼) 오버레이를 그대로 얹기 ----
            string path = $"{basePath}/scoop_overlays/{currentContainerIndex:00}_{code}/pos{slotOrder}_{flavorId}_{flavorName}.png";
            Sprite overlay = LoadSpriteAtPath(path);
            if (overlay == null) { Debug.LogError("스쿱 오버레이를 못 찾았습니다: " + path); return; }

            ResetSlot(targetSlot);                 // Simple 상태로
            targetSlot.sprite = overlay;
            targetSlot.gameObject.SetActive(true);
        }
        else
        {
            // ---- 산코~록코: flavor_disc 1장을 웨지(1/n)로 잘라 슬롯 위치에 회전 배치 ----
            string path = $"{basePath}/flavor_discs/disc_{flavorId}_{flavorName}.png";
            Sprite disc = LoadSpriteAtPath(path);
            if (disc == null) { Debug.LogError("맛 디스크를 못 찾았습니다: " + path); return; }

            int n = currentContainerIndex;         // 웨지 개수 = 담을 맛 개수
            targetSlot.sprite = disc;
            targetSlot.type = Image.Type.Filled;
            targetSlot.fillMethod = Image.FillMethod.Radial360;
            targetSlot.fillOrigin = (int)Image.Origin360.Top; // 위(12시)에서 시작
            targetSlot.fillClockwise = true;
            targetSlot.fillAmount = 1f / n;        // 한 조각 = 원의 1/n

            // slotOrder 번째 조각 위치로 회전(시계방향). 부호가 반대로 보이면 -를 +로.
            float angle = -(slotOrder - 1) * (360f / n);
            targetSlot.rectTransform.localEulerAngles = new Vector3(0f, 0f, angle);

            targetSlot.gameObject.SetActive(true);
        }
    }

    public void ResetVisual()
    {
        foreach (var slot in flavorSlotImages)
        {
            if (slot == null) continue;
            ResetSlot(slot);
            slot.gameObject.SetActive(false);
        }
    }

    // 슬롯을 기본(Simple·회전 0·풀 채움) 상태로 되돌림
    private void ResetSlot(Image slot)
    {
        slot.type = Image.Type.Simple;
        slot.fillAmount = 1f;
        slot.rectTransform.localEulerAngles = Vector3.zero;
    }

    private Sprite LoadSpriteAtPath(string path)
    {
#if UNITY_EDITOR
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
#else
        return null;
#endif
    }
}
