using UnityEngine;
using UnityEngine.UI;
using TMPro;

// 씬에 딱 한 번만 배치하면 됨 (OrderScene에 프리팹으로 하나 두는 걸 추천).
// DontDestroyOnLoad로 씬이 바뀌어도 사라지지 않고, OrderSession의 주문 상태를 계속 구독해서
// 주문이 있으면 자동으로 내용을 갱신하고, 주문이 끝나면(OrderSession.CompleteOrder 호출 시) 자동으로 숨겨진다.
public class CheatSheetUI : MonoBehaviour
{
    private static CheatSheetUI _instance;

    [Header("UI 연결")]
    public GameObject panel;       // 컨닝페이퍼 내용이 담긴 패널 (토글로 열고 닫힘)
    public TMP_Text paperText;     // paper 텍스트가 표시될 곳
    public Button toggleButton;    // 화면 구석의 "컨닝페이퍼 보기/숨기기" 버튼 (선택)

    void Awake()
    {
        // 씬에 중복으로 남아있으면(씬 재진입 등) 기존 걸 유지하고 새로 들어온 건 파괴
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void OnEnable()
    {
        OrderSession.Instance.OnOrderChanged += Refresh;
        if (toggleButton != null)
            toggleButton.onClick.AddListener(TogglePanel);

        Refresh(); // 처음 켜질 때 현재 상태 반영
    }

    void OnDisable()
    {
        if (OrderSession.Instance != null)
            OrderSession.Instance.OnOrderChanged -= Refresh;
        if (toggleButton != null)
            toggleButton.onClick.RemoveListener(TogglePanel);
    }

    // 주문 상태가 바뀔 때마다(주문 시작/완료) 자동 호출됨
    private void Refresh()
    {
        CustomerOrder order = OrderSession.Instance.CurrentOrder;

        if (order == null)
        {
            // 진행 중인 주문이 없으면 컨닝페이퍼 자체를 숨김
            if (panel != null) panel.SetActive(false);
            if (toggleButton != null) toggleButton.gameObject.SetActive(false);
            return;
        }

        // 새 주문이 잡히면 토글 버튼은 다시 보이게 하고, paper 텍스트를 갱신
        if (toggleButton != null) toggleButton.gameObject.SetActive(true);
        if (paperText != null) paperText.text = order.paper;

        // 평소엔 닫혀있다가 플레이어가 클릭해서 열어보는 방식이므로,
        // 새 주문이 시작될 때마다 패널은 무조건 닫힌 상태로 리셋
        if (panel != null) panel.SetActive(false);
    }

    private void TogglePanel()
    {
        if (panel == null) return;
        panel.SetActive(!panel.activeSelf);
    }
}