using System.Collections.Generic;
using UnityEngine;

// 씬 사이에 주문 정보를 넘겨주는 싱글턴. 씬에 배치할 필요 없이
// 어떤 스크립트에서든 OrderSession.Instance 로 접근하면 자동 생성되고
// 씬이 바뀌어도 유지된다(DontDestroyOnLoad).
//
// [주문 씬] CustomerManager.ConfirmOrderAndProceed() 에서
//   OrderSession.Instance.SetOrder(currentOrder, flavorTable) 호출 후 씬 전환
//
// [제작 씬] 사용자가 제작 화면에 들어왔을 때:
//   CustomerOrder order = OrderSession.Instance.CurrentOrder;
//   Dictionary<string, FlavorData> flavors = OrderSession.Instance.FlavorTable;
//   CupSize cup = OrderSession.Instance.SelectedCup;
public class OrderSession : MonoBehaviour
{
    private static OrderSession _instance;

    public static OrderSession Instance
    {
        get
        {
            if (_instance == null)
            {
                var go = new GameObject("OrderSession");
                _instance = go.AddComponent<OrderSession>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    public CustomerOrder CurrentOrder { get; private set; }
    public Dictionary<string, FlavorData> FlavorTable { get; private set; }

    // CupSelection 씬에서 선택한 컵 크기 (CraftScene 에서 읽어 사용)
    public CupSize SelectedCup { get; private set; }

    public void SetOrder(CustomerOrder order, Dictionary<string, FlavorData> flavorTable)
    {
        CurrentOrder = order;
        FlavorTable = flavorTable;
    }

    public void SetSelectedCup(CupSize size)
    {
        SelectedCup = size;
    }
}
