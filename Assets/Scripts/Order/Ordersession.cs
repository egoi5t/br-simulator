using System.Collections.Generic;
using UnityEngine;

// [주문 씬] CustomerManager.ConfirmOrderAndProceed() 에서
//   OrderSession.Instance.SetOrder(currentOrder, flavorTable) 호출 후 SceneManager.LoadScene(...)
//
// [컵 선택 씬] 담당자가 컵 선택 후 다음처럼 저장:
//   OrderSession.Instance.SetSelectedCup(selectedCup.Value);
//
// [담기 씬] 담당자가 다음처럼 읽으면 됨:
//   CustomerOrder order = OrderSession.Instance.CurrentOrder;
//   Dictionary<string, FlavorData> flavors = OrderSession.Instance.FlavorTable;
//   CupSize? cup = OrderSession.Instance.SelectedCup;
//
// [담기 씬] 손님에게 아이스크림을 전달해서 주문이 끝나면 반드시 호출:
//   OrderSession.Instance.CompleteOrder();
// → 이걸 호출해야 컨닝페이퍼(CheatSheetUI)가 자동으로 닫힘
public class OrderSession : MonoBehaviour
{
    private static OrderSession _instance;
    private static bool _quitting = false; // 앱 종료/씬 정리 중에는 새로 생성하지 않기 위한 플래그

    public static OrderSession Instance
    {
        get
        {
            if (_quitting) return null; // 종료되는 중이면 새로 만들지 않고 null 반환

            if (_instance == null)
            {
                var go = new GameObject("OrderSession");
                _instance = go.AddComponent<OrderSession>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    private void OnApplicationQuit()
    {
        _quitting = true;
    }

    public CustomerOrder CurrentOrder { get; private set; }
    public Dictionary<string, FlavorData> FlavorTable { get; private set; }
    public CupSize? SelectedCup { get; private set; } // 컵 선택 씬에서 저장

    // 주문이 새로 잡히거나(주문 확인) 끝났을 때(전달 완료) 알려주는 이벤트.
    // CheatSheetUI가 이걸 구독해서 자동으로 켜지고/꺼짐.
    public event System.Action OnOrderChanged;

    public void SetOrder(CustomerOrder order, Dictionary<string, FlavorData> flavorTable)
    {
        CurrentOrder = order;
        FlavorTable = flavorTable;
        SelectedCup = null; // 새 주문이 시작되면 이전 손님이 고른 컵 정보는 초기화
        OnOrderChanged?.Invoke();
    }

    // 컵 선택 씬에서 호출: 이번 주문에 쓸 컵 사이즈를 저장
    public void SetSelectedCup(CupSize size)
    {
        SelectedCup = size;
    }

    // 담기 씬에서 아이스크림을 손님에게 전달한 직후(만족/불만족 판정 후) 호출
    public void CompleteOrder()
    {
        CurrentOrder = null;
        FlavorTable = null;
        SelectedCup = null;
        OnOrderChanged?.Invoke();
    }
    // ════════════════════════════════════════════════════════
    // 아래는 화면③(최보광) 작업분 추가.
    // 이 블록을 OrderSession 클래스의 마지막 닫는 중괄호(}) "바로 위"에 붙여넣으세요.
    // 기존 코드(CurrentOrder, FlavorTable, SelectedCup, SetOrder, SetSelectedCup)는
    // 손대지 않습니다.
    // ════════════════════════════════════════════════════════

    // ── 주문 처리 타이머 (화면① 접수 ~ 화면③ 포장 완료까지 공유) ──

    /// <summary>제한 시간(초). 기본 30초. StartOrderTimer() 호출 시 실제 값으로 덮어써짐.</summary>
    public float OrderTimeLimit { get; private set; } = 30f;

    private float orderStartTime = -1f;

    /// <summary>새 주문이 시작될 때 한 번 호출 (원칙상 화면①에서 주문 접수 시).</summary>
    public void StartOrderTimer(float timeLimit = 30f)
    {
        OrderTimeLimit = timeLimit;
        orderStartTime = Time.time;
    }

    public float GetElapsedTime()
    {
        if (orderStartTime < 0f) return 0f;
        return Time.time - orderStartTime;
    }

    public float GetRemainingTime()
    {
        return Mathf.Max(0f, OrderTimeLimit - GetElapsedTime());
    }

    public bool IsTimeUp()
    {
        return orderStartTime >= 0f && GetElapsedTime() >= OrderTimeLimit;
    }

    // ── 점수 카운터 ──

    /// <summary>이번 손님 한정 실수 카운터. 손님 주문이 끝나면 0으로 초기화.</summary>
    public int ComplainCounter { get; set; } = 0;

    /// <summary>게임 전체 누적 카운터. 해고/게임오버 트리거로 쓰일 예정.</summary>
    public int BossCounter { get; set; } = 0;

    // ── 일일 정산용 ──

    /// <summary>오늘 하루 중 complainCounter가 한 번이라도 늘었는지 (급여 삭감 판정용).</summary>
    public bool DailyComplainOccurred { get; set; } = false;

    /// <summary>오늘 하루 중 bossCounter가 한 번이라도 늘었는지 (급여 삭감 판정용).</summary>
    public bool DailyBossOccurred { get; set; } = false;

    /// <summary>오늘 처리한 손님 수. 하루 일과 종료(손님 수 기준) 트리거에 사용.</summary>
    public int CustomersServedToday { get; set; } = 0;

    /// <summary>오늘 누적된 팁(원). 하루 정산 시 급여에 합산됨.</summary>
    public int DailyTipTotal { get; set; } = 0;

    /// <summary>게임 전체 누적 수익(원). 100만원 채우면 목표 달성.</summary>
    public int TotalEarnings { get; set; } = 0;

    /// <summary>목표 금액(원). 기본 100만원.</summary>
    public int TotalGoal = 1000000;

    /// <summary>바로 직전 주문의 팁(원). UI 표시용.</summary>
    public int LastOrderTip { get; set; } = 0;

    /// <summary>
    /// 손님의 화를 유발할 행동(사이즈/맛 오류, 시간 초과, 뚜껑 오답 등)을 했을 때 호출.
    /// ComplainCounter를 늘리는 동시에, 오늘 하루 실수가 있었다는 걸 기록해서 급여 삭감에 반영.
    /// </summary>
    public void RegisterComplaint()
    {
        ComplainCounter++;
        DailyComplainOccurred = true;
    }

    /// <summary>
    /// 점장이 화가 날 만한 행동(시간 60초 초과, complainCounter 3 이상 등)을 했을 때 호출.
    /// </summary>
    public void RegisterBossAnger()
    {
        BossCounter++;
        DailyBossOccurred = true;

        if (BossCounter > 3)
        {
            Debug.LogWarning($"bossCounter가 {BossCounter}(3 초과)입니다 - 해고/게임오버 트리거 지점. " +
                              "실제 게임오버 처리는 별도 시스템에서 연결 필요.");
        }
    }

    /// <summary>일일 정산 결과 하나를 담는 데이터. 정산 화면 UI 표시용.</summary>
    [System.Serializable]
    public struct DailySettlementResult
    {
        public int Day;
        public int BaseSalary;
        public int ComplainDeduction;
        public int BossDeduction;
        public int FinalSalary;
        public int TipTotal;
        public int DayTotal;
        public int CumulativeTotal;
        public int Goal;
    }

    /// <summary>
    /// 오늘 하루 정산 결과를 "계산만" 함 (데이터는 아직 안 바꿈).
    /// 정산 화면에서 이 값을 받아서 먼저 화면에 보여준 뒤, 확인 버튼 누르면 ApplyDaySettlement() 호출.
    /// </summary>
    public DailySettlementResult CalculateDaySettlement(bool isHardMode = false)
    {
        int baseSalary = isHardMode ? 150000 : 200000;
        int complainDeduction = DailyComplainOccurred ? 50000 : 0;
        int bossDeduction = DailyBossOccurred ? 100000 : 0;
        int finalSalary = Mathf.Max(0, baseSalary - complainDeduction - bossDeduction);
        int dayTotal = finalSalary + DailyTipTotal;

        return new DailySettlementResult
        {
            Day = CurrentDay,
            BaseSalary = baseSalary,
            ComplainDeduction = complainDeduction,
            BossDeduction = bossDeduction,
            FinalSalary = finalSalary,
            TipTotal = DailyTipTotal,
            DayTotal = dayTotal,
            CumulativeTotal = TotalEarnings + dayTotal,
            Goal = TotalGoal
        };
    }

    /// <summary>정산 화면에서 확인 버튼을 눌렀을 때 호출. 계산된 결과를 실제로 반영하고 하루 데이터 초기화.</summary>
    public void ApplyDaySettlement(DailySettlementResult result)
    {
        TotalEarnings = result.CumulativeTotal;

        DailyComplainOccurred = false;
        DailyBossOccurred = false;
        DailyTipTotal = 0;
        CustomersServedToday = 0;

        Debug.Log($"[일일 정산] {result.Day}일차 반영 완료. 누적 {TotalEarnings}원 / 목표 {TotalGoal}원");
    }

    /// <summary>다음 날로 하루 넘김. ApplyDaySettlement() 이후에 호출.</summary>
    public void AdvanceDay()
    {
        CurrentDay++;
    }

    // ── 일일 루프 (날짜, 하루 목표 손님 수) ──

    /// <summary>지금 며칠째인지. 1부터 시작.</summary>
    public int CurrentDay { get; set; } = 1;

    /// <summary>일차별 목표 손님 수. 1일차=7, 2일차=7, 3일차=8, 4일차=8.</summary>
    private static readonly int[] DailyCustomerTargets = { 7, 7, 8, 8 };

    /// <summary>전체 게임 일수 (DailyCustomerTargets 배열 길이 기준).</summary>
    public int TotalDays => DailyCustomerTargets.Length;

    /// <summary>오늘(CurrentDay 기준) 채워야 할 목표 손님 수.</summary>
    public int GetTodayCustomerTarget()
    {
        int index = Mathf.Clamp(CurrentDay - 1, 0, DailyCustomerTargets.Length - 1);
        return DailyCustomerTargets[index];
    }

    /// <summary>오늘 목표 손님 수를 다 채웠는지.</summary>
    public bool IsTodayComplete()
    {
        return CustomersServedToday >= GetTodayCustomerTarget();
    }

    /// <summary>마지막 날(4일차)까지 다 끝났는지.</summary>
    public bool IsGameComplete()
    {
        return CurrentDay > DailyCustomerTargets.Length;
    }

    /// <summary>
    /// 포장 완료 시점의 최종 판정 결과. 화면①이 이 값을 읽어서
    /// 손님의 satisfiedLine/unhappyLine 중 뭘 보여줄지 결정할 수 있음.
    /// </summary>
    public OrderEvaluationSystem.Outcome? LastOrderOutcome;

    /// <summary>
    /// CurrentOrder.scoopCount(주문한 스쿱 개수, 1~6)를 CupSize enum으로 변환.
    /// paper(주문서 텍스트, 예: "바닐라 하나")는 자연스러운 문장이라 파싱이 불안정해서
    /// 대신 이미 구조화된 scoopCount 필드를 사용함.
    /// </summary>
    public CupSize? GetOrderedCupSize()
    {
        if (CurrentOrder == null || CurrentOrder.scoopCount < 1 || CurrentOrder.scoopCount > 6)
        {
            Debug.LogWarning($"OrderSession: scoopCount 값({CurrentOrder?.scoopCount})이 1~6 범위를 벗어나 " +
                              "사이즈를 판정할 수 없습니다.");
            return null;
        }

        return (CupSize)(CurrentOrder.scoopCount - 1); // 1~6 -> Ikko(0)~Rokko(5)
    }

    // ── 판정용 스냅샷 ──
    // CompleteOrder()가 호출되면 CurrentOrder가 null이 되어버려서, 포장 시점엔
    // "손님이 원래 뭘 주문했는지"를 알 수 없게 됨. 그래서 1차(용기 선택) 화면에서
    // 주문 정보가 아직 살아있을 때 미리 떠두는 스냅샷.

    /// <summary>1차 화면에서 미리 떠둔 "손님이 주문한 사이즈" 스냅샷.</summary>
    public CupSize? SnapshotOrderedCupSize;

    /// <summary>1차 화면에서 미리 떠둔 "손님이 주문한 맛 리스트" 스냅샷.</summary>
    public List<string> SnapshotOrderedFlavorIds;

    /// <summary>한글 사이즈 이름 <-> CupSize enum 매핑. 테스트용 더미 주문 생성 시에도 사용.</summary>
    private static readonly Dictionary<string, CupSize> koreanSizeNames = new Dictionary<string, CupSize>
{
    { "잇코", CupSize.Ikko },
    { "니코", CupSize.Niko },
    { "산코", CupSize.Sanko },
    { "욘코", CupSize.Yonko },
    { "고코", CupSize.Goko },
    { "록코", CupSize.Rokko },
};

    /// <summary>CupSize -> 한글 사이즈 이름. 테스트용 더미 주문 만들 때 사용.</summary>
    public static string ToKoreanSizeName(CupSize size)
    {
        foreach (var kv in koreanSizeNames)
            if (kv.Value == size) return kv.Key;
        return size.ToString();
    }
}