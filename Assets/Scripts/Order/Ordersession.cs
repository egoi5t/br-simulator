using System.Collections.Generic;
using UnityEngine;

// 씬에 직접 배치할 필요 없음. 아무 스크립트에서나 OrderSession.Instance 로 접근하면
// 자동으로 생성되고 씬이 바뀌어도 유지된다.
//
// [주문 씬] CustomerManager.ConfirmOrderAndProceed() 에서
//   OrderSession.Instance.SetOrder(currentOrder, flavorTable) 호출 후 SceneManager.LoadScene(...)
//
// [담기 씬] 담당자가 다음처럼 읽으면 됨:
//   CustomerOrder order = OrderSession.Instance.CurrentOrder;
//   Dictionary<string, FlavorData> flavors = OrderSession.Instance.FlavorTable;
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

    public void SetOrder(CustomerOrder order, Dictionary<string, FlavorData> flavorTable)
    {
        CurrentOrder = order;
        FlavorTable = flavorTable;
    }
}