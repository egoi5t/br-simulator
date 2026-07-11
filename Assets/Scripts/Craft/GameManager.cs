using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("정답 데이터 (테스트용 고정)")]
    public string orderContainerType = "Small";
    public List<string> orderIngredients = new List<string> { "Choco", "Vanilla" };

    [Header("연결할 오브젝트")]
    public SpriteRenderer containerRenderer;
    public OrderTimer orderTimer; // 타이머 연결

    private List<string> currentContainerContents = new List<string>();
    private bool isMenuComplete = false;

    [Header("용기 선택 상태")]
    public string selectedContainerType;
    private SpriteRenderer selectedContainerRenderer;

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
            Debug.Log("주문 처리 시작");
        }
    }

    public void SelectContainer(string type, SpriteRenderer renderer)
    {
        selectedContainerType = type;
        selectedContainerRenderer = renderer;
        containerRenderer = renderer;
        Debug.Log("용기 선택됨: " + type);
    }

    public void AddIngredient(string ingredient, Color ingredientColor)
    {
        if (string.IsNullOrEmpty(selectedContainerType))
        {
            Debug.Log("⚠️ 먼저 용기를 선택해주세요!");
            return;
        }

        if (isMenuComplete)
        {
            Debug.Log("이미 완성된 메뉴입니다. 재질문 필요!");
            return;
        }

        currentContainerContents.Add(ingredient);
        Debug.Log("담긴 재료: " + string.Join(", ", currentContainerContents));

        if (containerRenderer != null)
        {
            containerRenderer.color = ingredientColor;
        }
    }

    public void TryCompleteMenu()
    {
        bool containerCorrect = selectedContainerType == orderContainerType;
        bool tasteCorrect = ScrambledEquals(currentContainerContents, orderIngredients);
        float elapsed = orderTimer.GetElapsedTime();

        if (!containerCorrect || !tasteCorrect)
        {
            //틀렸으면 계속 진행
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

            Invoke(nameof(ResetContainer), 2f);
            Debug.Log("소요 시간: " + elapsed.ToString("F1") + "초");
            return;
        }

        //맞았을 때만 실행
        isMenuComplete = true;
        orderTimer.StopTimer();

        Debug.Log("소요 시간: " + elapsed.ToString("F1") + "초");

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
            Debug.Log("✅ 완벽한 처리!");
        }

        Debug.Log("현재 누적 complainCounter: " + complainCounter + " / bossCounter: " + bossCounter);

        Invoke(nameof(ResetContainer), 2f);
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

    public void ResetContainer()
    {
        currentContainerContents.Clear();
        isMenuComplete = false;
        selectedContainerType = null;

        if (containerRenderer != null)
        {
            containerRenderer.color = Color.black;
        }
    }
}