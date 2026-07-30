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
}