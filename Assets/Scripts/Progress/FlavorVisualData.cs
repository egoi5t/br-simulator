using UnityEngine;

/// <summary>
/// 맛 ID별로 필요한 실제 아트 리소스를 한 곳에 모아두는 데이터 에셋.
/// CupVisualData랑 같은 패턴 - AssetDatabase.LoadAssetAtPath 같은 에디터 전용 방식 대신
/// Inspector에서 직접 스프라이트를 연결해두는 방식이라 빌드에서도 안전하게 작동함.
///
/// 사용법:
/// 1. Project 창 우클릭 → Create → BRSimulator → Flavor Visual Data
/// 2. Entries 배열 크기를 맛 개수만큼 설정
/// 3. 각 Entry에 Flavor Id(예: "FLV-001") + Scoop Sprite(잇코/니코용 원형 이미지)
///    + Disc Sprite(산코~록코용 부채꼴 채우기용 원판 이미지) 연결
/// </summary>
[CreateAssetMenu(fileName = "FlavorVisualData", menuName = "BRSimulator/Flavor Visual Data")]
public class FlavorVisualData : ScriptableObject
{
    [System.Serializable]
    public class Entry
    {
        public string flavorId;

        [Tooltip("잇코/니코(1~2개 스쿱)에 쓸 원형 스쿱 이미지. Art/IceCreamCups/ikko_niko_flavor_overlays 폴더의 pos1_FLV-... 계열")]
        public Sprite scoopSprite;

        [Tooltip("산코~록코(3~6개)에 쓸 원판 이미지. 부채꼴로 잘라서 채워짐. disc_FLV-... 계열")]
        public Sprite discSprite;
    }

    public Entry[] entries;

    /// <summary>해당 맛 ID의 데이터를 찾아서 반환. 못 찾으면 null.</summary>
    public Entry GetEntry(string flavorId)
    {
        if (entries == null) return null;

        foreach (var entry in entries)
        {
            if (entry.flavorId == flavorId)
                return entry;
        }

        return null;
    }
}
