using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class Customermanager : MonoBehaviour
{
    [Header("CSV 데이터 (customers.csv / flavors.csv 를 그대로 드래그)")]
    public TextAsset customerCsvFile;
    public TextAsset flavorCsvFile;

    [Header("스폰 설정")]
    public GameObject customerPrefab;   
    public Transform spawnPoint;        

    [Header("오늘(Day) 설정")]
    public int currentDay = 1;

    private List<CustomerOrder> allOrders;
    private Dictionary<string, FlavorData> flavorTable;
    private List<CustomerOrder> todayOrders;
    private int todayIndex = 0;

    private CustomerOrder currentOrder;
    private GameObject currentCustomerObj;

    void Start()
    {
        LoadData();
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

        todayOrders = allOrders.FindAll(o => o.day == currentDay);
        todayIndex = 0;

        if (todayOrders.Count == 0)
            Debug.LogWarning($"Day {currentDay}에 해당하는 손님 데이터가 없습니다.");
    }

    // 다음 손님을 스폰하고 말풍선에 주문 대사를 띄운다.
    public void SpawnNextCustomer()
    {
        if (todayOrders == null || todayIndex >= todayOrders.Count)
        {
            Debug.Log("오늘 손님이 모두 끝났습니다.");
            // TODO: 다음 날로 넘기거나, 하루 마감 씬으로 전환하는 로직을 여기 추가
            return;
        }

        currentOrder = todayOrders[todayIndex];
        todayIndex++;

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
    }

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
}