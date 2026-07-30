using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

// 손님 프리팹 루트에 붙이는 스크립트.
// 프리팹 구조 예시:
// CustomerRoot (SpriteRenderer + CustomerView)
//   └ SpeechBubbleCanvas (World Space Canvas)
//         └ BubbleBackground (Image)
//               └ OrderText (TMP_Text)
public class CustomerView : MonoBehaviour
{
    [Header("말풍선 UI")]
    public GameObject speechBubble;   // 말풍선 배경 오브젝트 (평소엔 꺼둠)
    public TMP_Text orderText;        // 말풍선 안에 들어갈 텍스트

    [Header("타이핑 효과")]
    public bool useTypingEffect = true;
    public float charInterval = 0.03f;

    [Header("주문 확인 버튼 (말풍선 안/옆에 배치)")]
    public Button confirmButton;

    private CustomerOrder order;
    private Dictionary<string, FlavorData> flavorTable;
    private Coroutine typingRoutine;

    public void Setup(CustomerOrder order, Dictionary<string, FlavorData> flavorTable)
    {
        this.order = order;
        this.flavorTable = flavorTable;

        if (speechBubble != null)
            speechBubble.SetActive(false);
    }

    // 스폰 직후 주문 대사를 말풍선에 표시
    public void ShowOrderLine()
    {
        ShowLine(order.orderLine);
    }

    // 다음 씬에서 결과에 따라 재사용할 수 있도록 함께 준비
    public void ShowSatisfiedLine() => ShowLine(order.satisfiedLine, typing: false);
    public void ShowUnsatisfiedLine() => ShowLine(order.unsatisfiedLine, typing: false);

    private void ShowLine(string line, bool typing = true)
    {
        if (string.IsNullOrEmpty(line) || orderText == null) return;

        if (speechBubble != null)
            speechBubble.SetActive(true);

        if (typingRoutine != null)
            StopCoroutine(typingRoutine);

        if (useTypingEffect && typing)
            typingRoutine = StartCoroutine(TypeLine(line));
        else
            orderText.text = line;
    }

    private IEnumerator TypeLine(string line)
    {
        var sb = new StringBuilder();
        orderText.text = "";
        foreach (char c in line)
        {
            sb.Append(c);
            orderText.text = sb.ToString();
            yield return new WaitForSeconds(charInterval);
        }
    }

    // CustomerManager가 스폰 직후 호출: 이 손님의 확인 버튼을 실제 동작에 연결
    public void SetConfirmAction(UnityAction action)
    {
        if (confirmButton == null) return;

        confirmButton.onClick.RemoveAllListeners();
        confirmButton.onClick.AddListener(action);
    }

    // 담기 씬에서 참고할 수 있도록 맛 ID를 맛 이름으로 변환
    public List<string> GetFlavorNames()
    {
        var names = new List<string>();
        if (order == null) return names;

        foreach (string id in order.flavorIds)
        {
            if (flavorTable != null && flavorTable.TryGetValue(id, out FlavorData f))
                names.Add(f.flavorName);
            else
                names.Add(id);
        }
        return names;
    }

    public CustomerOrder GetOrder() => order;
}