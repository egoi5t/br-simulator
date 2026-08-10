using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
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
    [Tooltip("이 씬이 주문의 시작점이면 체크 (화면① 역할을 겸하는 테스트 씬일 때). 실제 통합 시엔 화면①에서 이미 시작했을 것이므로 체크 해제")]
    public bool startTimerHere = true;
    [Tooltip("제한 시간(초). Start Timer Here가 체크됐을 때만 이 값으로 타이머가 시작됨")]
    public float orderTimeLimit = 30f;
    public TMP_Text timerText;
    private bool timerRunning = true;

    [Header("테스트용 - 화면① 미연동 시 주문 시뮬레이션")]
    [Tooltip("체크하면 이 씬 시작 시 더미 CustomerOrder를 만들어 OrderSession에 주입 (정확도 평가 테스트용)")]
    public bool simulateOrder = true;
    public CupSize debugOrderedSize = CupSize.Goko;

    [Header("다음 버튼")]
    public Button nextButton;

    private GameObject currentCupOnTable;
    private CupSize? selectedCup = null;

    private void Start()
    {
        if (startTimerHere)
        {
            OrderSession.Instance.StartOrderTimer(orderTimeLimit);
            if (PersistentTimerUI.Instance != null)
                PersistentTimerUI.Instance.ShowTimer();
        }

        if (simulateOrder && OrderSession.Instance.CurrentOrder == null)
        {
            // 화면①이 아직 없어서 OrderSession에 주문이 없는 경우, 테스트용 더미 주문 생성
            var dummyOrder = new CustomerOrder
            {
                scoopCount = (int)debugOrderedSize + 1, // Ikko(0)~Rokko(5) -> 1~6
                paper = "테스트 주문"
            };
            OrderSession.Instance.SetOrder(dummyOrder, new Dictionary<string, FlavorData>());
            Debug.Log($"[테스트 모드] 더미 주문 생성 (scoopCount={dummyOrder.scoopCount}, {debugOrderedSize}). " +
                      "실제 화면①과 연동되면 Simulate Order 체크 해제하세요.");
        }

        nextButton.interactable = true; // 항상 클릭 가능. 준비 안 됐으면 클릭 시 경고 처리
        nextButton.onClick.AddListener(OnNextButtonPressed);
        UpdateTimerDisplay();

        // 판정용 스냅샷: CurrentOrder가 나중에(포장 시점) null이 되어버리므로
        // 지금(주문 정보가 살아있는 시점) 미리 떠둠
        OrderSession.Instance.SnapshotOrderedCupSize = OrderSession.Instance.GetOrderedCupSize();
        OrderSession.Instance.SnapshotOrderedFlavorIds = OrderSession.Instance.CurrentOrder != null
            ? new List<string>(OrderSession.Instance.CurrentOrder.flavorIds)
            : null;
    }

    private void Update()
    {
        if (!timerRunning) return;

        if (OrderSession.Instance.IsTimeUp())
        {
            timerRunning = false;
            OnTimeUp();
        }

        UpdateTimerDisplay();
    }

    private void UpdateTimerDisplay()
    {
        int seconds = Mathf.CeilToInt(OrderSession.Instance.GetRemainingTime());
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
        // GetComponentInChildren을 써서 Image가 루트든 자식이든 상관없이 찾음
        var entry = visualData != null ? visualData.GetEntry(size) : null;
        if (entry != null && entry.tableCupSprite != null)
        {
            var image = currentCupOnTable.GetComponentInChildren<Image>();
            if (image != null)
                image.sprite = entry.tableCupSprite;
            else
                Debug.LogWarning("TableCup 프리팹에서 Image 컴포넌트를 찾지 못했습니다.");
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
            OrderSession.Instance.RegisterComplaint();
            if (WarningPopupEffect.Instance != null)
                WarningPopupEffect.Instance.PlayWarningAtMouse("시간이 초과됐어요!");
            Debug.Log("시간 초과: 용기 선택 실패 -> complainCounter++");
        }
    }

    [Header("씬 전환")]
    [Tooltip("1차(용기 선택) 완료 후 이동할 화면②(제작) 씬 이름. Build Profiles에 등록된 이름과 정확히 일치해야 함")]
    public string craftSceneName = "CraftScene";

    private void OnNextButtonPressed()
    {
        if (selectedCup == null)
        {
            if (WarningPopupEffect.Instance != null)
                WarningPopupEffect.Instance.PlayWarningAtMouse("컵을 먼저 골라주세요!");
            Debug.LogWarning("[다음 버튼] selectedCup이 비어있어서 경고 표시함");
            return;
        }

        timerRunning = false;

        // 선택한 컵 정보를 다음 씬에서도 쓸 수 있게 저장
        OrderSession.Instance.SetSelectedCup(selectedCup.Value);

        Debug.Log($"선택된 컵: {selectedCup} -> {craftSceneName}로 전환");

        SceneManager.LoadScene(craftSceneName);
    }
}