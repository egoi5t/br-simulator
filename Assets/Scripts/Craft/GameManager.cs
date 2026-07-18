using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("정답 데이터 (테스트용 고정)")]
    public int orderContainerIndex = 3; // 1~6 (손님이 요청한 용기)
    public List<string> orderFlavorIds = new List<string> { "FLV-001", "FLV-005", "FLV-007" }; // 정답 맛 순서

    [Header("연결할 오브젝트")]
    public ContainerVisual containerVisual;
    public OrderTimer orderTimer;

    private List<string> currentFlavorIds = new List<string>();
    private bool isMenuComplete = false;

    [Header("용기 선택 상태 (화면③에서 전달받음, 지금은 테스트용 직접 세팅)")]
    public int selectedContainerIndex = 0; // 0이면 아직 선택 안 됨

    [Header("카운터")]
    public int complainCounter = 0;
    public int bossCounter = 0;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (orderTimer != null)
        {
            orderTimer.StartTimer();
            Debug.Log("주문 처리 시작 — 타이머 가동");
        }
    }

    // 화면③에서 넘겨받을 함수 (지금은 테스트용으로 Inspector 값 그대로 사용해도 됨)
    public void ReceiveContainer(int containerIndex)
    {
        selectedContainerIndex = containerIndex;
        containerVisual.SetContainer(containerIndex);
        Debug.Log("용기 수신: " + containerIndex + "번");
    }

    public void AddFlavor(string flavorId, string flavorName)
    {
        if (selectedContainerIndex == 0)
        {
            Debug.Log("⚠️ 용기 정보가 아직 없습니다!");
            return;
        }

        if (isMenuComplete)
        {
            Debug.Log("이미 완성된 메뉴입니다.");
            return;
        }

        if (currentFlavorIds.Count >= selectedContainerIndex)
        {
            Debug.Log("⚠️ 이 용기는 최대 " + selectedContainerIndex + "개까지만 담을 수 있어요!");
            return;
        }

        currentFlavorIds.Add(flavorId);
        int slotOrder = currentFlavorIds.Count; // 몇 번째로 담겼는지
        containerVisual.ApplyFlavor(slotOrder, flavorId, flavorName);

        Debug.Log("담긴 맛: " + string.Join(", ", currentFlavorIds));
    }

    public void TryCompleteMenu()
    {
        bool containerCorrect = selectedContainerIndex == orderContainerIndex;
        bool tasteCorrect = ScrambledEquals(currentFlavorIds, orderFlavorIds);
        float elapsed = orderTimer.GetElapsedTime();

        if (!containerCorrect || !tasteCorrect)
        {
            Debug.Log("현재 경과 시간: " + elapsed.ToString("F1") + "초");

            if (!containerCorrect)
            {
                Debug.Log("❌ 메뉴(용기)가 틀렸습니다 → complainCounter++");
                complainCounter++;
            }
            if (!tasteCorrect)
            {
                Debug.Log("❌ 맛이 틀렸습니다 → complainCounter++");
                complainCounter++;
            }
            Debug.Log("⚠️ 다시 담아보세요");

            currentFlavorIds.Clear();
            containerVisual.ResetVisual();
            return;
        }

        isMenuComplete = true;
        orderTimer.StopTimer();

        Debug.Log("최종 소요 시간: " + elapsed.ToString("F1") + "초");

        if (elapsed >= 60f)
        {
            Debug.Log("⏰ 60초 초과! bossCounter++ , complainCounter++");
            bossCounter++;
            complainCounter++;
        }
        else if (elapsed >= 30f)
        {
            Debug.Log("⏰ 30초 초과! complainCounter++");
            complainCounter++;
        }
        else
        {
            Debug.Log("✅ 완벽한 처리! 포장 단계로 진행");
        }

        Debug.Log("현재 누적 complainCounter: " + complainCounter + " / bossCounter: " + bossCounter);

        Invoke(nameof(ResetOrder), 2f);
    }

    private bool ScrambledEquals(List<string> a, List<string> b)
    {
        if (a.Count != b.Count) return false;
        List<string> sortedA = new List<string>(a);
        List<string> sortedB = new List<string>(b);
        sortedA.Sort();
        sortedB.Sort();
        for (int i = 0; i < sortedA.Count; i++)
        {
            if (sortedA[i] != sortedB[i]) return false;
        }
        return true;
    }

    public void ResetOrder()
    {
        currentFlavorIds.Clear();
        isMenuComplete = false;
        selectedContainerIndex = 0;
        containerVisual.ResetVisual();
    }
}