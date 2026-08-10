using UnityEngine;
using UnityEngine.UI;

public class ContainerVisual : MonoBehaviour
{
    [Header("용기 UI 오브젝트")]
    public Image containerBaseImage;    // 빈 용기 베이스
    public Image[] flavorSlotImages;    // 맛 슬롯(최대 6). 웨지 회전을 위해 용기와 동일 크기·중앙 겹침 권장

    private int currentContainerIndex;  // 1=잇코 … 6=록코 (= 담을 수 있는 맛 개수)
    
// 용기 코드명(폴더/파일)
    private readonly string[] containerCodeNames =
        { "ikko", "niko", "sanko", "yonko", "goko", "rokko" };

    // 2026-08-10: 빌드에서도 뜨도록 Resources.Load 사용.
    // 아트는 Assets/Resources/CraftArt/ 아래로 이동됨. 경로는 Resources 기준 상대경로(확장자 없음).
    private const string basePath = "CraftArt";
    private const string containersFolder = "IceCreamCup";
    private const string scoopFolder = "IceCreamFlvaorTexture/ikko_niko_flavor_overlays";
    private const string discFolder = "IceCreamFlvaorTexture/flavor_discs";

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

        // 2026-07-25: 잇코/니코 = "cup", 산코~록코 = "flat"
        string suffix = (containerIndex <= 2) ? "cup" : "flat";
        string path = $"{basePath}/{containersFolder}/cup_{containerIndex:00}_{code}_{suffix}";

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
            string path = $"{basePath}/{scoopFolder}/{currentContainerIndex:00}_{code}/pos{slotOrder}_{flavorId}_{flavorName}";
            Sprite overlay = LoadSpriteAtPath(path);
            if (overlay == null) { Debug.LogError("스쿱 오버레이를 못 찾았습니다: " + path); return; }

            ResetSlot(targetSlot);                  // Simple 상태로
            targetSlot.sprite = overlay;
            targetSlot.gameObject.SetActive(true);
        }
        else
        {
            // ---- 산코~록코: flavor_disc 1장을 웨지(1/n)로 잘라 슬롯 위치에 회전 배치 ----
            string path = $"{basePath}/{discFolder}/disc_{flavorId}_{flavorName}";
            Sprite disc = LoadSpriteAtPath(path);
            if (disc == null) { Debug.LogError("맛 디스크를 못 찾았습니다: " + path); return; }

            int n = currentContainerIndex;      // 웨지 개수 = 담을 맛 개수
            targetSlot.sprite = disc;
            targetSlot.type = Image.Type.Filled;
            targetSlot.fillMethod = Image.FillMethod.Radial360;
            targetSlot.fillOrigin = (int)Image.Origin360.Top; // 위(12시)에서 시작
            targetSlot.fillClockwise = true;
            targetSlot.fillAmount = 1f / n;     // 한 조각 = 원의 1/n
            
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

    // 2026-08-10: 빌드 대응 - Resources.Load 로 로드(에디터/빌드 모두 동작).
    // Single 스프라이트 모드면 바로 잡히고, Multiple(spriteMode 2) 모드면
    // 그 파일이 품은 하위 스프라이트 중 첫 번째를 사용.
    private Sprite LoadSpriteAtPath(string path)
    {
        Sprite sprite = Resources.Load<Sprite>(path);
        if (sprite != null) return sprite;

        Sprite[] all = Resources.LoadAll<Sprite>(path);
        if (all != null && all.Length > 0) return all[0];

        return null;
    }
}