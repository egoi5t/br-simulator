using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CustomerManager : MonoBehaviour
{
    [Header("CSV 데이터 (customers.csv / flavors.csv 를 그대로 드래그)")]
    public TextAsset customerCsvFile;
    public TextAsset flavorCsvFile;

    [Header("스폰 설정")]
    public GameObject customerPrefab;   // CustomerView가 붙은 프리팹
    public Transform spawnPoint;        // 손님이 나타날 위치

    [Header("일자/할당량 설정")]
    public int[] customersPerDay = new int[] { 7, 7, 8, 8 }; // 배열 길이 = 총 일수, 각 값 = 해당 날짜의 손님 수

    [Header("씬 전환")]
    public string scoopSceneName = "IceCreamScoopScene"; // 실제 담기 씬 이름으로 변경

    // 날짜가 바뀌거나(파라미터: 방금 끝난 날짜) 전체 게임이 끝났을 때 UI 쪽에서 구독해서 쓸 수 있는 이벤트
    public event System.Action<int> OnDayCompleted;
    public event System.Action OnAllDaysCompleted;

    private List<CustomerOrder> allOrders;
    private Dictionary<string, FlavorData> flavorTable;

    private List<CustomerOrder> spawnQueue; // 실제 스폰 순서 (0번=고정 첫 손님, 이후=랜덤)
    private int spawnIndex = 0;
    private int dayCustomerCount = 0;
    private int currentDay = 1;

    private CustomerOrder currentOrder;
    private GameObject currentCustomerObj;

    void Start()
    {
        LoadData();
        BuildSpawnQueue();
        SpawnNextCustomer();
    }

    private void LoadData()
    {
        if (customerCsvFile == null || flavorCsvFile == null)
        {
            Debug.LogError("CustomerManager: CSV TextAsset이 연결되지 않았습니다.");
            return;
        }

        allOrders = CsvOrderParser.ParseCsv(customerCsvFile.text);
        flavorTable = CsvOrderParser.ParseFlavors(flavorCsvFile.text);

        int expected = 0;
        foreach (int n in customersPerDay) expected += n;

        if (allOrders.Count != expected)
        {
            Debug.LogWarning($"CSV 손님 수({allOrders.Count})가 예상 값({expected} = {ArrayToString(customersPerDay)} 합계)과 다릅니다. 확인해주세요.");
        }
    }

    private string ArrayToString(int[] arr) => "[" + string.Join(",", arr) + "]";

    // 0번 손님은 고정, 나머지는 랜덤 셔플해서 스폰 순서를 미리 만들어둔다.
    private void BuildSpawnQueue()
    {
        spawnQueue = new List<CustomerOrder>();
        if (allOrders == null || allOrders.Count == 0) return;

        spawnQueue.Add(allOrders[0]); // 게임 시작 첫 손님 고정

        List<CustomerOrder> rest = allOrders.GetRange(1, allOrders.Count - 1);
        Shuffle(rest);
        spawnQueue.AddRange(rest);

        spawnIndex = 0;
        dayCustomerCount = 0;
        currentDay = 1;
    }

    // Fisher-Yates 셔플
    private void Shuffle(List<CustomerOrder> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    // 다음 손님을 스폰하고 말풍선에 주문 대사를 띄운다.
    public void SpawnNextCustomer()
    {
        if (spawnQueue == null || spawnIndex >= spawnQueue.Count)
        {
            Debug.Log("모든 날짜의 손님이 끝났습니다.");
            OnAllDaysCompleted?.Invoke();
            // TODO: 엔딩/결과 씬으로 전환하는 로직을 여기 추가
            return;
        }

        currentOrder = spawnQueue[spawnIndex];
        spawnIndex++;
        dayCustomerCount++;

        if (currentCustomerObj != null)
            Destroy(currentCustomerObj);

        currentCustomerObj = Instantiate(customerPrefab, spawnPoint.position, spawnPoint.rotation);

        CustomerView view = currentCustomerObj.GetComponent<CustomerView>();
        if (view == null)
        {
            Debug.LogError("customerPrefab에 CustomerView 컴포넌트가 없습니다.");
            return;
        }

        view.Setup(currentOrder, flavorTable);
        view.ShowOrderLine();
        view.SetConfirmAction(OnClickConfirmOrder); // 프리팹 안의 확인 버튼을 이 매니저의 동작과 연결

        // 오늘(currentDay번째) 할당량을 채웠는지 체크 (다음 스폰부터 날짜가 넘어감)
        int todayQuota = GetQuotaForDay(currentDay);
        if (dayCustomerCount >= todayQuota)
        {
            int finishedDay = currentDay;
            dayCustomerCount = 0;
            currentDay++;

            OnDayCompleted?.Invoke(finishedDay);
            // TODO: 날짜 전환 연출(화면 전환, "Day 2 시작!" 배너 등)이 필요하면 이 지점에서 처리

            if (currentDay > customersPerDay.Length)
            {
                Debug.Log("마지막 날 손님까지 모두 스폰했습니다.");
            }
        }
    }

    // day는 1부터 시작, 배열은 0부터 시작이라 -1 보정. 범위를 벗어나면 0(더 이상 없음) 반환
    private int GetQuotaForDay(int day)
    {
        int idx = day - 1;
        if (idx < 0 || idx >= customersPerDay.Length) return 0;
        return customersPerDay[idx];
    }

    public int GetCurrentDay() => currentDay;
    public int GetDayCustomerCount() => dayCustomerCount;
    public int GetTotalDays() => customersPerDay.Length;

    // '주문 확인' 버튼 등에서 호출: 현재 주문 정보를 다음 씬(아이스크림 담기)으로 넘기고 씬 전환
    public void ConfirmOrderAndProceed(string nextSceneName)
    {
        if (currentOrder == null)
        {
            Debug.LogWarning("넘길 주문 정보가 없습니다.");
            return;
        }

        OrderSession.Instance.SetOrder(currentOrder, flavorTable);
        SceneManager.LoadScene(nextSceneName);
    }

    public CustomerOrder GetCurrentOrder() => currentOrder;

    // '주문 확인' 버튼 OnClick에 파라미터 없이 바로 연결하는 용도
    public void OnClickConfirmOrder()
    {
        ConfirmOrderAndProceed(scoopSceneName);
    }
}