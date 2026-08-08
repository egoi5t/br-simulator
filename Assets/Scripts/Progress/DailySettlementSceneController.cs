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
/// 3. 4일차까지 다 끝났으면 엔딩 씬으로, 아니면 다음 날 화면①(주문)로 전환
///
/// 인스펙터 설정:
/// - Day Label: "1일차 정산" 같은 제목 텍스트
/// - Base Salary Text / Complain Deduction Text / Boss Deduction Text / Final Salary Text
/// - Tip Total Text / Day Total Text / Cumulative Text (누적/목표)
/// - Continue Button
/// - Ending Scene Name / Next Order Scene Name
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
    [Tooltip("4일차까지 끝났을 때 이동할 엔딩 씬 이름")]
    public string endingSceneName = "EndingScene";
    [Tooltip("아직 남은 날이 있을 때 이동할 화면①(주문) 씬 이름")]
    public string nextOrderSceneName = "OrderScene";

    private OrderSession.DailySettlementResult result;

    private void Start()
    {
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

        if (OrderSession.Instance.IsGameComplete())
        {
            Debug.Log("4일차까지 모두 종료! 엔딩 씬으로 이동합니다.");
            SceneManager.LoadScene(endingSceneName);
        }
        else
        {
            Debug.Log($"{OrderSession.Instance.CurrentDay}일차 시작 " +
                      $"(오늘 목표: {OrderSession.Instance.GetTodayCustomerTarget()}명) -> 주문 씬으로 이동합니다.");
            SceneManager.LoadScene(nextOrderSceneName);
        }
    }
}
