using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro 안 쓰면 이 줄 지우고 Text로 교체

/// <summary>
/// 화면③ "용기 선택 → 꺼내기" 씬의 메인 컨트롤러.
/// - 45초 타이머
/// - 컵 클릭 선택 시 테이블에 등장 애니메이션
/// - 선택 후 "제작하러 가기" 버튼 활성화
///
/// 인스펙터 설정 순서:
/// 1. 빈 오브젝트에 이 스크립트 부착 (예: SceneController)
/// 2. Table Spawn Point: 컵이 등장할 위치의 RectTransform 연결
/// 3. Table Cup Prefab: 테이블에 등장할 TableCup 프리팹 1개만 연결 (사이즈 상관없이 재사용)
/// 4. Visual Data: CupVisualData 에셋 연결 (사이즈별 스프라이트 정보)
/// 5. Timer Text: 상단 타이머 텍스트 연결
/// 6. Next Button: "제작하러 가기" 버튼 연결
///
/// 선반의 각 컵에는 이 스크립트가 아니라 CupClickSelectable.cs를 붙이세요.
/// </summary>
public class CupSelectionSceneController : MonoBehaviour
{
    [Header("테이블 표시")]
    public RectTransform tableSpawnPoint;
    [Tooltip("사이즈 상관없이 재사용할 프리팹 1개. 사이즈별 이미지는 Visual Data에서 가져옴")]
    public GameObject tableCupPrefab;
    [Tooltip("사이즈별 스프라이트가 담긴 데이터 에셋")]
    public CupVisualData visualData;
    public float dropAnimDuration = 0.3f;

    [Header("타이머")]
    public float timeLimit = 45f;
    public TMP_Text timerText;
    private float remainingTime;
    private bool timerRunning = true;

    [Header("다음 버튼")]
    public Button nextButton;

    private GameObject currentCupOnTable;
    private CupSize? selectedCup = null;

    private void Start()
    {
        remainingTime = timeLimit;
        nextButton.interactable = false;
        nextButton.onClick.AddListener(OnNextButtonPressed);
        UpdateTimerDisplay();
    }

    private void Update()
    {
        if (!timerRunning) return;

        remainingTime -= Time.deltaTime;
        if (remainingTime <= 0f)
        {
            remainingTime = 0f;
            timerRunning = false;
            OnTimeUp();
        }

        UpdateTimerDisplay();
    }

    private void UpdateTimerDisplay()
    {
        int seconds = Mathf.CeilToInt(remainingTime);
        int m = seconds / 60;
        int s = seconds % 60;
        timerText.text = $"{m}:{s:00}";
    }

    /// <summary>CupClickSelectable이 클릭 선택 시 호출</summary>
    public void SelectCup(CupSize size)
    {
        selectedCup = size;

        if (currentCupOnTable != null)
            Destroy(currentCupOnTable);

        currentCupOnTable = Instantiate(tableCupPrefab, tableSpawnPoint);
        currentCupOnTable.transform.localPosition = Vector3.zero;

        // 사이즈에 맞는 스프라이트를 데이터에서 찾아서 적용
        var entry = visualData != null ? visualData.GetEntry(size) : null;
        if (entry != null && entry.tableCupSprite != null)
        {
            var image = currentCupOnTable.GetComponent<Image>();
            if (image != null)
                image.sprite = entry.tableCupSprite;
        }

        StartCoroutine(DropAnimation(currentCupOnTable.GetComponent<RectTransform>()));

        nextButton.interactable = true;
    }

    private IEnumerator DropAnimation(RectTransform cupRect)
    {
        Vector2 endPos = cupRect.anchoredPosition;
        Vector2 startPos = endPos + Vector2.up * 200f;
        float elapsed = 0f;

        cupRect.anchoredPosition = startPos;

        while (elapsed < dropAnimDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / dropAnimDuration;
            cupRect.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            yield return null;
        }

        cupRect.anchoredPosition = endPos;
    }

    private void OnTimeUp()
    {
        if (selectedCup == null)
        {
            // 시간 내 컵을 선택하지 못한 경우 -> complainCounter 연동 지점
            Debug.Log("시간 초과: 용기 선택 실패 (complainCounter++ 연동 필요)");
            // ScoreManager.Instance.AddComplaint();
        }
    }

    private void OnNextButtonPressed()
    {
        if (selectedCup == null) return;

        timerRunning = false;
        Debug.Log($"선택된 컵: {selectedCup} -> 제작 단계로 전환");

        // TODO: 씬 전환 또는 화면 전환 로직
        // 예) SceneManager.LoadScene("CupFillingScene");
        // 또는 다른 팀원의 ②제작·조작 화면으로 전환하는 이벤트 호출
    }
}
