using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// OrderScene의 메인 컨트롤러.
//
// 이 스크립트는 OrderScene이 로드될 때마다 두 가지 상황 중 하나로 동작을 나눈다.
//
// [상황 A] 새 손님 차례 (OrderSession.CurrentOrder == null)
//   → 스폰 큐에서 다음 손님을 꺼내 스폰, 주문 대사 출력, 확인 버튼 연결
//
// [상황 B] 컵선택→제작→포장을 거쳐 배달하러 돌아온 상황 (OrderSession.CurrentOrder != null)
//   → 같은 손님을 대사 없이 조용히 다시 세워두기만 함 (확인 버튼 X)
//   → 플레이어가 실제로 "전달" 동작을 했을 때(드래그 앤 드롭 등에서 DeliverToCustomer() 호출)
//     OrderSession.LastOrderOutcome을 읽어 만족/불만족 대사를 그제서야 보여줌
public class CustomerManager : MonoBehaviour
{
    [Header("CSV 데이터 (customers.csv / flavors.csv 를 그대로 드래그)")]
    public TextAsset customerCsvFile;
    public TextAsset flavorCsvFile;

    [Header("스폰 설정")]
    public GameObject customerPrefab;   // CustomerView가 붙은 프리팹
    public Transform spawnPoint;        // 손님이 나타날 위치
    public CustomerVisualData visualData; // customer_id별 스프라이트 매핑 에셋

    [Header("씬 전환")]
    public string cupSelectionSceneName = "CupSelectionScene"; // 주문 확인 버튼 누르면 이동

    [Header("배달용 쇼핑백 (드래그 아이템)")]
    public GameObject iceCreamBagPrefab; // DeliveryBagItem이 붙은 프리팹, 스프라이트는 쇼핑백 하나로 고정
    public RectTransform iceCreamSpawnPoint; // 완성된 아이스크림이 처음 놓일 자리 (예: 카운터 위)

    [Header("배달 후 다음 손님으로 넘어가기 전 대기 시간(초)")]
    public float resultDisplayDuration = 1.5f;

    private Dictionary<string, FlavorData> flavorTable;
    private GameObject currentCustomerObj;
    private CustomerView currentView;
    private GameObject currentIceCreamBag;

    void Start()
    {
        LoadFlavorTable();

        if (OrderSession.Instance.CurrentOrder != null)
        {
            // 상황 B: 배달하러 돌아온 상태
            ShowReturningCustomer();
        }
        else
        {
            // 상황 A: 새 손님 차례
            if (!OrderSession.Instance.IsSpawnQueueBuilt)
                BuildSpawnQueueIntoSession();

            SpawnNextCustomer();
        }
    }

    private void LoadFlavorTable()
    {
        if (flavorCsvFile == null)
        {
            Debug.LogError("CustomerManager: flavorCsvFile이 연결되지 않았습니다.");
            return;
        }
        flavorTable = CsvOrderParser.ParseFlavors(flavorCsvFile.text);
    }

    // 게임 최초 시작 시 딱 한 번만 호출됨 (OrderSession에 큐가 없을 때만)
    private void BuildSpawnQueueIntoSession()
    {
        if (customerCsvFile == null)
        {
            Debug.LogError("CustomerManager: customerCsvFile이 연결되지 않았습니다.");
            return;
        }

        List<CustomerOrder> allOrders = CsvOrderParser.ParseCsv(customerCsvFile.text);
        var queue = new List<CustomerOrder>();

        if (allOrders.Count > 0)
        {
            queue.Add(allOrders[0]); // 첫 손님 고정
            List<CustomerOrder> rest = allOrders.GetRange(1, allOrders.Count - 1);
            Shuffle(rest);
            queue.AddRange(rest);
        }

        OrderSession.Instance.SpawnQueue = queue;
        OrderSession.Instance.SpawnIndex = 0;
    }

    private void Shuffle(List<CustomerOrder> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    // [상황 A] 다음 손님을 스폰하고 말풍선에 주문 대사를 띄운다.
    public void SpawnNextCustomer()
    {
        var session = OrderSession.Instance;

        if (session.SpawnQueue == null || session.SpawnIndex >= session.SpawnQueue.Count)
        {
            Debug.Log("스폰할 손님이 더 이상 없습니다.");
            return;
        }

        CustomerOrder order = session.SpawnQueue[session.SpawnIndex];
        session.SpawnIndex++;

        SpawnCustomerObject(order);

        currentView.ShowOrderLine();
        currentView.SetConfirmAction(OnClickConfirmOrder);
    }

    // [상황 B] 이미 확정된 CurrentOrder로 같은 손님을 대사 없이 다시 세워둠
    private void ShowReturningCustomer()
    {
        CustomerOrder order = OrderSession.Instance.CurrentOrder;
        SpawnCustomerObject(order);
        currentView.HideBubbleAndButton(); // 말풍선/확인 버튼 확실히 숨김 - 배달(전달) 동작만 기다리는 상태
        currentView.SetDeliveryAction(DeliverToCustomer); // 손님 위 드롭존에 배달 콜백 연결

        SpawnDeliverableIceCream();
    }

    // 완성된 아이스크림(쇼핑백)을 카운터 위치에 드래그 가능한 상태로 스폰
    private void SpawnDeliverableIceCream()
    {
        if (iceCreamBagPrefab == null || iceCreamSpawnPoint == null)
        {
            Debug.LogWarning("CustomerManager: iceCreamBagPrefab 또는 iceCreamSpawnPoint가 연결되지 않았습니다.");
            return;
        }

        if (currentIceCreamBag != null)
            Destroy(currentIceCreamBag);

        currentIceCreamBag = Instantiate(iceCreamBagPrefab, iceCreamSpawnPoint);
        var rect = currentIceCreamBag.GetComponent<RectTransform>();
        if (rect != null)
            rect.anchoredPosition = Vector2.zero;
    }

    private void SpawnCustomerObject(CustomerOrder order)
    {
        if (order == null)
        {
            Debug.LogError("CustomerManager: order가 null입니다. OrderSession.CurrentOrder 또는 SpawnQueue 상태를 확인하세요.");
            return;
        }

        if (currentCustomerObj != null)
            Destroy(currentCustomerObj);

        currentCustomerObj = Instantiate(customerPrefab, spawnPoint.position, spawnPoint.rotation);
        currentView = currentCustomerObj.GetComponent<CustomerView>();

        if (currentView == null)
        {
            Debug.LogError("customerPrefab에 CustomerView 컴포넌트가 없습니다.");
            return;
        }

        currentView.Setup(order, flavorTable, GetSpriteFor(order.customerId));
    }

    private Sprite GetSpriteFor(string customerId)
    {
        if (visualData == null) return null;
        return visualData.GetSprite(customerId);
    }

    // "주문 확인" 버튼 클릭 시: 이번 주문을 OrderSession에 확정 저장하고 컵 선택 씬으로 이동
    public void OnClickConfirmOrder()
    {
        if (currentView == null) return;

        CustomerOrder order = currentView.GetOrder();
        OrderSession.Instance.SetOrder(order, flavorTable);

        // 판정용 스냅샷도 같이 떠둠 (포장 단계에서 CurrentOrder가 사라져도 비교할 수 있도록)
        OrderSession.Instance.SnapshotOrderedFlavorIds = new List<string>(order.flavorIds);

        SceneManager.LoadScene(cupSelectionSceneName);
    }

    // [상황 B 전용] 드래그 앤 드롭 등 실제 전달 동작이 완료됐을 때 호출.
    // OrderSession.LastOrderOutcome을 읽어 만족/불만족 대사를 보여주고, 다음 손님으로 넘어간다.
    public void DeliverToCustomer()
    {
        StartCoroutine(DeliverRoutine());
    }

    private System.Collections.IEnumerator DeliverRoutine()
    {
        var session = OrderSession.Instance;
        var outcome = session.LastOrderOutcome;

        // 4단계 판정에 따라 카운터 반영 + 대사 결정
        // (CustomerOrder엔 satisfiedLine/unsatisfiedLine 2종류뿐이라, NoProblem만 만족 대사, 나머지는 전부 불만족 대사)
        bool isSatisfied = false;

        switch (outcome)
        {
            case OrderEvaluationSystem.Outcome.NoProblem:
                isSatisfied = true;
                break;

            case OrderEvaluationSystem.Outcome.NoTip:
                session.RegisterComplaint();
                break;

            case OrderEvaluationSystem.Outcome.NoTipNoPay:
                session.RegisterComplaint();
                break;

            case OrderEvaluationSystem.Outcome.BossAngry:
                // 주석상 "앞선 패널티 전부 + bossCounter++" 이므로 컴플레인도 같이 기록
                session.RegisterComplaint();
                session.RegisterBossAnger();
                break;

            default:
                Debug.LogWarning("CustomerManager: LastOrderOutcome이 설정되지 않은 상태로 배달이 호출됐습니다.");
                break;
        }

        if (currentView != null)
        {
            if (isSatisfied)
                currentView.ShowSatisfiedLine();
            else
                currentView.ShowUnsatisfiedLine();
        }

        yield return new WaitForSeconds(resultDisplayDuration);

        currentIceCreamBag = null; // DeliveryBagItem이 스스로 Destroy됐으므로 참조만 정리
        session.LastOrderOutcome = null;
        session.CompleteOrder();

        session.CustomersServedToday++;

        if (session.IsTodayComplete())
        {
            session.AdvanceDay();
            // TODO: 여기서 일일 정산 화면으로 전환하는 로직 연결 (session.CalculateDaySettlement() 사용)
            Debug.Log($"{session.CurrentDay - 1}일차 손님 목표 달성. 정산 화면 연결 필요.");

            if (session.IsGameComplete())
            {
                Debug.Log("4일차까지 전부 완료. 엔딩 처리 필요.");
                yield break;
            }
        }

        SpawnNextCustomer();
    }
}