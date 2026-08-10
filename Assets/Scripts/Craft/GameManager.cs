using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("연결할 오브젝트")]
    public ContainerVisual containerVisual;

    [Header("타이머 표시")]
    public TMP_Text timerText;

    private List<string> currentFlavorIds = new List<string>();
    private bool isMenuComplete = false;

    [Header("용기 선택 상태 (1~6, 화면③에서 전달받음)")]
    public int selectedContainerIndex = 0;

    [Header("맛 코드 → 파일명 매핑")]
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
        // 타이머는 화면③에서 이미 시작됨

        // 화면③에서 온 용기 데이터
        CupSize? cup = OrderSession.Instance.SelectedCup;
        if (cup.HasValue)
        {
            int containerIndex = (int)cup.Value + 1;
            ReceiveContainer(containerIndex);
        }
        else
        {
            Debug.Log("⚠️ OrderSession에 용기 정보 없음");
        }
    }

    private void Update()
    {
        UpdateTimerDisplay();
    }

    private void UpdateTimerDisplay()
    {
        if (timerText == null) return;

        int seconds = Mathf.CeilToInt(OrderSession.Instance.GetRemainingTime());
        int m = seconds / 60;
        int s = seconds % 60;
        timerText.text = $"{m}:{s:00}";
    }

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
        CraftSfxManager.Instance?.PlayAddFlavor(); // 맛 담기 성공음

        Debug.Log("담긴 맛: " + string.Join(", ", currentFlavorIds));
    }

    private bool IsCorrectToolForContainer()
    {
        bool needsScoop = selectedContainerIndex <= 2;
        int currentTool = ToolManager.Instance.currentTool;
        return needsScoop ? currentTool == 1 : currentTool == 2;
    }

    public void TryCompleteMenu()
    {
        if (ToolManager.Instance.HasToolEquipped())
        {
            Debug.Log("⚠️ 도구를 내려놓은 후 완성할 수 있습니다!");
            FeedbackManager.Instance.PlayErrorFeedbackAtMouse();
            return;
        }

        isMenuComplete = true;
        CraftSfxManager.Instance?.PlayComplete(); // 담기 완성음

        // 완성 결과를 화면③(포장)으로 넘기기 위해 저장
        CraftResultSession.Instance.SetResult(selectedContainerIndex, currentFlavorIds);


        Debug.Log("담기 완료 - 포장 화면으로 이동");

        //OrderSession.Instance.CompleteOrder();


        SceneManager.LoadScene("CupSelectionScene");
    }
}