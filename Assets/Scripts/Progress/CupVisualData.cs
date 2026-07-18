using UnityEngine;

/// <summary>
/// 컵 사이즈(잇코~록코)별로 필요한 비주얼 리소스를 한 곳에 모아두는 데이터 에셋.
///
/// 사용법:
/// 1. Project 창 우클릭 → Create → BRSimulator → Cup Visual Data
/// 2. 생성된 에셋 선택 → Entries 배열 크기를 6으로 설정
/// 3. Element 0~5에 각각 Size(Ikko~Rokko)를 지정하고,
///    지금은 더미 이미지/프리팹을 넣어두었다가
///    나중에 실제 에셋이 나오면 이 파일에서만 갈아끼우면 됩니다.
///    (씬이나 프리팹을 따로 건드릴 필요 없음)
/// </summary>
[CreateAssetMenu(fileName = "CupVisualData", menuName = "BRSimulator/Cup Visual Data")]
public class CupVisualData : ScriptableObject
{
    [System.Serializable]
    public class Entry
    {
        public CupSize size;

        [Tooltip("선반의 CupStack에 적용할 '탑' 이미지 (1개~6개 쌓인 모양)")]
        public Sprite shelfStackSprite;

        [Tooltip("테이블에 등장할 때 TableCup 프리팹에 적용할 완성 용기 이미지")]
        public Sprite tableCupSprite;
    }

    public Entry[] entries = new Entry[6];

    /// <summary>해당 사이즈에 맞는 데이터를 찾아서 반환. 못 찾으면 null.</summary>
    public Entry GetEntry(CupSize size)
    {
        foreach (var entry in entries)
        {
            if (entry.size == size)
                return entry;
        }

        Debug.LogWarning($"CupVisualData: {size}에 해당하는 Entry를 찾지 못했습니다.");
        return null;
    }
}
