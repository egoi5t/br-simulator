using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("정답 데이터 (테스트용 고정, 화면①에서 못 받으면 이 값 사용)")]
    public List<string> orderFlavorIds = new List<string> { "FLV-001", "FLV-005", "FLV-007" };

    [Header("연결할 오브젝트")]
    public ContainerVisual containerVisual;
    public OrderTimer orderTimer;

    private List<string> currentFlavorIds = new List<string>();
    private bool isMenuComplete = false;

    [Header("용기 선택 상태 (1~6, 화면③에서 전달받음)")]
    public int selectedContainerIndex = 0; // 0이면 아직 선택 안 됨

    [Header("카운터")]
    public int complainCounter = 0;
    public int bossCounter = 0;

    [Header("맛 코드 → 파일명 매핑 (실제 이미지 파일명 기준, 검증 완료)")]
    private readonly Dictionary<string, string> flavorFileNames = new Dictionary<string, string>
    {
        { "FLV-001", "Mint-Chocolate-Chip" },
        { "FLV-002", "Puss-in-Boots" },
        { "FLV-003", "Shooting-Star" },
        { "FLV-004", "Almond-Bon-Bon" },
        { "FLV-005", "New-York-Cheesecake" },
        { "FLV-006", "Cherry-Jubilee" },
        { "FLV-007", "Very-Berry-Strawberry" },
        { "FLV-008", "Rainbow-Sherbet" },
        { "FLV-009", "Chocolate-Mousse" },
        { "FLV-010", "Cookies-n-Cream" },
        { "FLV-011", "Love-Potion-31" },
        { "FLV-012", "Cotton-Candy-Wonderland" },
        { "FLV-013", "Jamoca-Almond-Fudge" },
        { "FLV-014", "Green-Tea" },
        { "FLV-015", "Pistachio-Almond" },
        { "FLV-016", "Pralines-n-Cream" },
        { "FLV-017", "Twinberry-Cheesecake" },
        { "FLV-018", "Vanilla" },
        { "FLV-019", "Chocolate" },
        { "FLV-020", "31-Yogurt" },
    };

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

        // 화면①에서 온 주문(맛) 데이터
        CustomerOrder order = OrderSession.Instance.CurrentOrder;
        if (order != null)
        {
            orderFlavorIds = order.flavorIds;
            Debug.Log("주문 접수 — 맛: " + string.Join(", ", orderFlavorIds));
        }
        else
        {
            Debug.Log("⚠️ OrderSession에 주문 없음 — Inspector 고정값(orderFlavorIds)으로 테스트 진행");
        }

        // 화면③에서 온 용기 데이터
        CupSize? cup = OrderSession.Instance.SelectedCup;
        if (cup.HasValue)
        {
            // CupSize는 0(Ikko)~5(Rokko), 우리 시스템은 1~6이므로 +1
            int containerIndex = (int)cup.Value + 1;
            ReceiveContainer(containerIndex);
        }
        else
        {
            Debug.Log("⚠️ OrderSession에 용기 정보 없음 — 테스트 버튼으로 직접 선택 필요");
        }
    }

    // 화면③에서 넘어온 용기 정보 적용
    public void ReceiveContainer(int containerIndex)
    {
        selectedContainerIndex = containerIndex;
        containerVisual.SetContainer(containerIndex);
        Debug.Log("용기 수신: " + containerIndex + "번");
    }

    public void AddFlavor(string flavorId, RectTransform sourceRect)
    {
        if (!ToolManager.Instance.HasToolEquipped())
        {
            Debug.Log("⚠️ 먼저 도구를 선택해주세요!");
            FeedbackManager.Instance.PlayErrorFeedbackAtMouse();   
            return;
        }

        if (!IsCorrectToolForContainer())
        {
            Debug.Log("⚠️ 이 용기에는 다른 도구를 사용해야 합니다!");
            FeedbackManager.Instance.PlayErrorFeedbackAtMouse();   
            return;
        }

        if (isMenuComplete)
        {
            Debug.Log("이미 완성된 메뉴입니다.");
            FeedbackManager.Instance.PlayErrorFeedbackAtMouse();  
            return;
        }

        if (currentFlavorIds.Count >= selectedContainerIndex)
        {
            Debug.Log("⚠️ 이 용기는 최대 " + selectedContainerIndex + "개까지만 담을 수 있어요!");
            FeedbackManager.Instance.PlayErrorFeedbackAtMouse();   
            return;
        }

        if (!flavorFileNames.TryGetValue(flavorId, out string fileFormattedName))
        {
            Debug.LogError("파일명 매핑이 없는 flavorId: " + flavorId);
            return;
        }

        currentFlavorIds.Add(flavorId);
        int slotOrder = currentFlavorIds.Count;
        containerVisual.ApplyFlavor(slotOrder, flavorId, fileFormattedName);

        Debug.Log("담긴 맛: " + string.Join(", ", currentFlavorIds));
    }

    // 현재 용기(1~6)에 맞는 도구를 들고 있는지 확인
    private bool IsCorrectToolForContainer()
    {
        bool needsScoop = selectedContainerIndex <= 2; // 1,2번은 스쿱
        int currentTool = ToolManager.Instance.currentTool; // 1=Scoop, 2=Spade

        if (needsScoop)
        {
            return currentTool == 1; // Scoop
        }
        else
        {
            return currentTool == 2; // Spade
        }
    }

    public void TryCompleteMenu()
    {
        if (ToolManager.Instance.HasToolEquipped())
        {
            Debug.Log("⚠️ 도구를 내려놓은 후 완성할 수 있습니다!");
            FeedbackManager.Instance.PlayErrorFeedbackAtMouse();
            return;
        }

        bool tasteCorrect = ScrambledEquals(currentFlavorIds, orderFlavorIds);
        float elapsed = orderTimer.GetElapsedTime();

        if (!tasteCorrect)
        {
            Debug.Log("현재 경과 시간: " + elapsed.ToString("F1") + "초");
            Debug.Log("❌ 맛이 틀렸습니다 → complainCounter++");
            complainCounter++;
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

        // 완성 결과를 화면3 으로 넘기기 위해 저장
        CraftResultSession.Instance.SetResult(selectedContainerIndex, currentFlavorIds);

        //OrderSession.Instance.CompleteOrder();

        Invoke(nameof(GoToCupSelection), 0.1f);
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

    private void GoToCupSelection()
    {
        SceneManager.LoadScene("CupSelectionScene");
    }
}