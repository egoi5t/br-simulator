using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// 하루 일과가 끝났을 때(오늘 목표 손님 수 도달) 보여주는 정산 화면.
/// 화면①(김정민)이 손님에게 전달까지 끝낸 뒤, OrderSession.Instance.IsTodayComplete()가
/// true면 이 씬으로 전환해줘야 함.
///
/// 흐름:
/// 1. Start()에서 OrderSession.CalculateDaySettlement()로 계산만 하고 화면에 표시
///    (아직 데이터는 안 바뀜 - 확인 버튼 누르기 전까지는 취소해도 안전한 상태)
/// 2. "계속하기" 버튼 클릭 -> ApplyDaySettlement() + AdvanceDay() 실행해서 실제로 반영
/// 3. 판정 순서:
///    a) bossCounter가 3 초과 -> 며칠째든 상관없이 즉시 배드 엔딩(해고)
///    b) 4일차까지 다 끝남 -> 목표 금액 달성했으면 해피 엔딩, 못했으면 배드 엔딩
///    c) 둘 다 아니면 -> 다음 날 화면①(주문)로 이동
///
/// 인스펙터 설정:
/// - Day Label: "1일차 정산" 같은 제목 텍스트
/// - Base Salary Text / Complain Deduction Text / Boss Deduction Text / Final Salary Text
/// - Tip Total Text / Day Total Text / Cumulative Text (누적/목표)
/// - Continue Button
/// - Happy Ending Scene Name / Bad Ending Scene Name / Next Order Scene Name
/// </summary>
public class DailySettlementSceneController : MonoBehaviour
{
    [Header("텍스트 표시")]
    public TMP_Text dayLabel;
    public TMP_Text baseSalaryText;
    public TMP_Text complainDeductionText;
    public TMP_Text bossDeductionText;
    public TMP_Text finalSalaryText;
    public TMP_Text tipTotalText;
    public TMP_Text dayTotalText;
    public TMP_Text cumulativeText;

    [Header("버튼")]
    public Button continueButton;

    [Header("난이도")]
    [Tooltip("true면 어려움(기본급 15만원), false면 쉬움(20만원)")]
    public bool isHardMode = false;

    [Header("씬 전환")]
    [Tooltip("4일차까지 끝났고 목표 금액 달성 시 이동할 해피 엔딩 씬 이름")]
    public string happyEndingSceneName = "HappyEndingScene";
    [Tooltip("4일차까지 끝났지만 목표 미달성, 또는 bossCounter 초과로 해고됐을 때 이동할 배드 엔딩 씬 이름")]
    public string badEndingSceneName = "BadEndingScene";
    [Tooltip("아직 남은 날이 있을 때 이동할 화면①(주문) 씬 이름")]
    public string nextOrderSceneName = "OrderScene";

    [Header("테스트용 - 값 강제 지정 (전체 루프 안 돌려도 분기 테스트 가능)")]
    [Tooltip("체크하면 아래 값들로 OrderSession을 덮어쓰고 시작함")]
    public bool useDebugValues = false;
    [Tooltip("4로 하면 계속 버튼 눌렀을 때 바로 게임 종료(4일차 이후) 분기로 감")]
    public int debugCurrentDay = 4;
    [Tooltip("목표(TotalGoal, 기본 100만원) 이상이면 해피, 미만이면 배드")]
    public int debugTotalEarnings = 1000000;
    [Tooltip("3 초과면 며칠째든 상관없이 즉시 배드 엔딩(해고)")]
    public int debugBossCounter = 0;

    private OrderSession.DailySettlementResult result;

    private void Start()
    {
        if (useDebugValues)
        {
            OrderSession.Instance.CurrentDay = debugCurrentDay;
            OrderSession.Instance.TotalEarnings = debugTotalEarnings;
            OrderSession.Instance.BossCounter = debugBossCounter;
            Debug.Log($"[테스트 모드] CurrentDay={debugCurrentDay}, TotalEarnings={debugTotalEarnings}, " +
                      $"BossCounter={debugBossCounter}로 강제 지정. 실제 플레이 테스트할 땐 Use Debug Values 체크 해제하세요.");
        }

        result = OrderSession.Instance.CalculateDaySettlement(isHardMode);
        DisplayResult();

        if (continueButton != null)
        {
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(OnContinueClicked);
        }
    }

    private void DisplayResult()
    {
        if (dayLabel != null) dayLabel.text = $"Day {result.Day} Settlement";

        if (baseSalaryText != null) baseSalaryText.text = $"Base Salary: {result.BaseSalary:N0}";

        if (complainDeductionText != null)
            complainDeductionText.text = result.ComplainDeduction > 0
                ? $"Complaint Penalty: -{result.ComplainDeduction:N0}"
                : "Complaint Penalty: none";

        if (bossDeductionText != null)
            bossDeductionText.text = result.BossDeduction > 0
                ? $"Manager Penalty: -{result.BossDeduction:N0}"
                : "Manager Penalty: none";

        if (finalSalaryText != null) finalSalaryText.text = $"Final Salary: {result.FinalSalary:N0}";
        if (tipTotalText != null) tipTotalText.text = $"Tips: {result.TipTotal:N0}";
        if (dayTotalText != null) dayTotalText.text = $"Today Total: {result.DayTotal:N0}";
        if (cumulativeText != null) cumulativeText.text = $"Total: {result.CumulativeTotal:N0} / {result.Goal:N0}";
    }

    private void OnContinueClicked()
    {
        OrderSession.Instance.ApplyDaySettlement(result);
        OrderSession.Instance.AdvanceDay();

        // 1. 해고 판정 - bossCounter가 3을 초과했으면 며칠째든 상관없이 즉시 배드 엔딩
        //    (기획서: "해고, 다음 일자 시작과 함께 게임 종료")
        if (OrderSession.Instance.BossCounter > 3)
        {
            Debug.Log($"bossCounter {OrderSession.Instance.BossCounter}(3 초과)로 해고 -> 배드 엔딩");
            SceneManager.LoadScene(badEndingSceneName);
            return;
        }

        // 2. 4일차까지 다 끝났으면 목표 금액 달성 여부로 해피/배드 판정
        if (OrderSession.Instance.IsGameComplete())
        {
            bool success = OrderSession.Instance.TotalEarnings >= OrderSession.Instance.TotalGoal;
            string endingScene = success ? happyEndingSceneName : badEndingSceneName;

            Debug.Log($"전체 일정 종료! 누적 {OrderSession.Instance.TotalEarnings}원 / " +
                      $"목표 {OrderSession.Instance.TotalGoal}원 -> {(success ? "해피" : "배드")} 엔딩으로 이동");
            SceneManager.LoadScene(endingScene);
            return;
        }

        // 3. 둘 다 아니면 다음 날 시작 (화면①로)
        Debug.Log($"{OrderSession.Instance.CurrentDay}일차 시작 " +
                  $"(오늘 목표: {OrderSession.Instance.GetTodayCustomerTarget()}명) -> 주문 씬으로 이동합니다.");
        SceneManager.LoadScene(nextOrderSceneName);
    }
}