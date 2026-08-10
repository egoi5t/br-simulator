using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// 씬이 바뀌어도 사라지지 않고 화면에 계속 떠 있는 타이머 UI.
/// OrderSession처럼 DontDestroyOnLoad로 유지되는 싱글톤이라, 한 번만 만들어두면
/// 화면①→③→②→③ 씬이 바뀌는 내내 같은 오브젝트가 계속 남아서 타이머를 표시함.
///
/// 표시 규칙:
/// - 새 주문이 들어오는 순간(OrderSession.OnOrderChanged) -> 자동으로 숨김
///   (이전 손님의 남은 시간이 잠깐이라도 보이는 걸 방지)
/// - 화면③ 1차(용기 선택)에서 실제로 타이머가 시작되는 순간 -> 다시 보임
///   (CupSelectionSceneController가 ShowTimer() 호출)
///
/// 사용법:
/// 1. 이 스크립트를 Canvas(타이머 텍스트를 담은) 최상위 오브젝트에 붙임
/// 2. 게임에서 제일 먼저 로드되는 씬에 딱 한 번 배치
/// 3. Timer Text와 배경 이미지 등을 전부 "Timer Root" 밑에 자식으로 넣어두면,
///    Timer Root 전체가 켜지고 꺼지면서 이미지까지 같이 사라짐/나타남
/// </summary>
public class PersistentTimerUI : MonoBehaviour
{
    public static PersistentTimerUI Instance;

    [Header("타이머 UI 전체 (텍스트 + 배경 이미지 등을 전부 감싸는 부모)")]
    [Tooltip("여기 연결한 오브젝트가 통째로 보이거나 숨겨짐. 비워두면 Timer Text만 숨김/표시")]
    public GameObject timerRoot;

    [Header("타이머 텍스트")]
    public TMP_Text timerText;

    // 2026-08-10: 이 씬들이 로드되면 타이머를 무조건 숨김 (메인·주문 받기·엔딩)
    [Header("이 씬에서는 타이머 숨김 (메인/주문/엔딩 등)")]
    public string[] hideOnScenes = { "MainScene", "OrderScene", "HappyEndingScene", "BadEndingScene" };

    private void Awake()
    {
        // 씬이 다시 로드되면서 이 오브젝트가 중복으로 또 생기는 경우 방지
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        HideTimer(); // 시작할 때는 일단 숨겨둠 (아직 어떤 주문의 타이머도 시작 안 된 상태)
    }

    private void OnEnable()
    {
        OrderSession.Instance.OnOrderChanged += HandleOrderChanged;
        // 2026-08-10: 씬이 로드될 때마다 숨김 대상 씬인지 확인
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        if (OrderSession.Instance != null)
            OrderSession.Instance.OnOrderChanged -= HandleOrderChanged;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // 2026-08-10: 메인·주문 받기·엔딩 씬에서는 타이머가 남아있지 않도록 자동 숨김
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        foreach (string sceneName in hideOnScenes)
        {
            if (scene.name == sceneName)
            {
                HideTimer();
                return;
            }
        }
    }

    /// <summary>새 주문이 잡히거나(접수) 끝났을 때(전달 완료) 자동으로 호출됨.</summary>
    private void HandleOrderChanged()
    {
        HideTimer();
    }

    /// <summary>화면③ 1차에서 실제로 타이머를 시작시킨 직후 호출해서 다시 보이게 함.</summary>
    public void ShowTimer()
    {
        if (timerRoot != null)
            timerRoot.SetActive(true);
        else if (timerText != null)
            timerText.gameObject.SetActive(true);
    }

    public void HideTimer()
    {
        if (timerRoot != null)
            timerRoot.SetActive(false);
        else if (timerText != null)
            timerText.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (timerText == null) return;
        if (timerRoot != null && !timerRoot.activeSelf) return;
        if (timerRoot == null && !timerText.gameObject.activeSelf) return;

        float remaining = OrderSession.Instance.GetRemainingTime();
        int seconds = Mathf.CeilToInt(remaining);
        int m = seconds / 60;
        int s = seconds % 60;

        timerText.text = $"{m}:{s:00}";
    }
}