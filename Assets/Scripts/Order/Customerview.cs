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
    [Header("외형")]
    public SpriteRenderer bodyRenderer; // 손님 캐릭터 스프라이트가 그려지는 SpriteRenderer

    [Header("말풍선 UI")]
    public GameObject speechBubble;   // 말풍선 배경 오브젝트 (평소엔 꺼둠)
    public TMP_Text orderText;        // 말풍선 안에 들어갈 텍스트

    [Header("타이핑 효과")]
    public bool useTypingEffect = true;
    public float charInterval = 0.03f;

    [Header("주문 확인 버튼 (말풍선 안/옆에 배치)")]
    public Button confirmButton;

    [Header("배달 드롭존 (손님 위에 겹쳐진 투명 UI)")]
    public CustomerDropZone dropZone;

    private CustomerOrder order;
    private Dictionary<string, FlavorData> flavorTable;
    private Coroutine typingRoutine;

    public void Setup(CustomerOrder order, Dictionary<string, FlavorData> flavorTable, Sprite sprite = null)
    {
        this.order = order;
        this.flavorTable = flavorTable;

        if (sprite != null && bodyRenderer != null)
            bodyRenderer.sprite = sprite;

        if (speechBubble != null)
            speechBubble.SetActive(false);
    }

    // 배달 대기용(재등장) 상태: 말풍선/확인 버튼 둘 다 확실히 숨김.
    // speechBubble의 자식 구조와 무관하게 동작하도록 각각 명시적으로 끔.
    public void HideBubbleAndButton()
    {
        if (typingRoutine != null)
        {
            StopCoroutine(typingRoutine);
            typingRoutine = null;
        }

        if (speechBubble != null)
            speechBubble.SetActive(false);

        if (confirmButton != null)
            confirmButton.gameObject.SetActive(false);
    }

    // 스폰 직후 주문 대사를 말풍선에 표시
    public void ShowOrderLine()
    {
        if (confirmButton != null)
            confirmButton.gameObject.SetActive(true); // 새 손님 차례에는 버튼 다시 보이게
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

    // CustomerManager가 "배달 대기" 상태로 재등장시킬 때 호출: 드롭존에 배달 콜백 연결
    public void SetDeliveryAction(UnityAction action)
    {
        if (dropZone != null)
            dropZone.SetDeliveryCallback(action);
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