using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class ContainerVisual : MonoBehaviour
{
    [Header("연결할 UI 오브젝트")]
    public Image containerBaseImage;
    public Image[] flavorSlotImages;

    private int currentContainerIndex;
    private readonly string[] containerCodeNames =
        { "01_ikko", "02_niko", "03_sanko", "04_yonko", "05_goko", "06_rokko" };

    private const string basePath = "Assets/Art/IceCreamCups";

    private void Awake()
    {
        //시작 시점엔 아직 용기 정보가 없으니 다 꺼둠
        containerBaseImage.gameObject.SetActive(false);

        foreach (var slot in flavorSlotImages)
        {
            if (slot != null)
            {
                slot.gameObject.SetActive(false);
            }
        }
    }

    public void SetContainer(int containerIndex)
    {
        containerBaseImage.gameObject.SetActive(true); // 용기 정보 받으면 다시 켬

        currentContainerIndex = containerIndex;
        string code = containerCodeNames[containerIndex - 1];

        string path = $"{basePath}/01_containers_empty/cup_{code}_top.png";
        Sprite baseSprite = LoadSpriteAtPath(path);

        if (baseSprite == null)
        {
            Debug.LogError("용기 베이스 이미지를 못 찾았습니다: " + path);
        }
        containerBaseImage.sprite = baseSprite;

        foreach (var slot in flavorSlotImages)
        {
            slot.gameObject.SetActive(false);
        }
    }

    public void ApplyFlavor(int slotOrder, string flavorId, string flavorName)
    {
        string containerCode = containerCodeNames[currentContainerIndex - 1];
        string prefix = (currentContainerIndex <= 2) ? "pos" : "slot";
        string path = $"{basePath}/02_flavor_overlays/{containerCode}/{prefix}{slotOrder}_{flavorId}_{flavorName}.png";

        Sprite overlaySprite = LoadSpriteAtPath(path);
        if (overlaySprite == null)
        {
            Debug.LogError("맛 이미지를 못 찾았습니다: " + path);
            return;
        }

        Image targetSlot = flavorSlotImages[slotOrder - 1];
        targetSlot.sprite = overlaySprite;
        targetSlot.gameObject.SetActive(true);
    }

    public void ResetVisual()
    {
        foreach (var slot in flavorSlotImages)
        {
            slot.gameObject.SetActive(false);
        }
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