using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// 엔딩 씬(해피/배드 둘 다 공용)에서 "메인으로" 버튼 눌렀을 때 처리.
/// 메인 화면으로 돌아가면서 게임 진행 데이터(누적 수익, 며칠째, 카운터 등)를
/// 초기화해서 다음에 새 게임을 시작할 수 있게 함.
///
/// 인스펙터 설정:
/// - Main Menu Button: "메인으로" 버튼
/// - Main Scene Name: 실제 메인/타이틀 씬 이름
/// - Reset Progress On Return: 체크하면 메인으로 돌아갈 때 진행 데이터 초기화
/// </summary>
public class EndingSceneController : MonoBehaviour
{
    [Header("버튼")]
    public Button mainMenuButton;

    [Header("엔딩 이미지 (배드 엔딩 씬 전용: 해고 시 교체)")]
    [Tooltip("엔딩 배경 Image. 해고(WasFired)면 아래 Fired Sprite로 교체됨. 해피 엔딩 씬에선 비워둬도 됨")]
    public Image endingImage;
    [Tooltip("해고 시 표시할 스프라이트 (_ending_fired)")]
    public Sprite firedSprite;

    [Header("씬 전환")]
    public string mainSceneName = "MainScene";

    [Header("진행 데이터 초기화")]
    [Tooltip("체크하면 메인으로 돌아갈 때 OrderSession의 진행 데이터를 전부 리셋함 (새 게임 준비)")]
    public bool resetProgressOnReturn = true;

    private void Start()
    {
        // 해고로 진입한 배드 엔딩이면 엔딩 이미지를 _ending_fired 로 교체
        if (OrderSession.Instance.WasFired && endingImage != null && firedSprite != null)
            endingImage.sprite = firedSprite;

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.RemoveAllListeners();
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        }
        else
        {
            Debug.LogWarning("EndingSceneController: Main Menu Button이 연결되지 않았습니다.");
        }
    }

    private void OnMainMenuClicked()
    {
        if (resetProgressOnReturn)
        {
            ResetGameProgress();
        }

        SceneManager.LoadScene(mainSceneName);
    }

    /// <summary>다음 플레이를 위해 게임 진행 데이터를 처음 상태로 되돌림.</summary>
    private void ResetGameProgress()
    {
        OrderSession.Instance.CurrentDay = 1;
        OrderSession.Instance.TotalEarnings = 0;
        OrderSession.Instance.BossCounter = 0;
        OrderSession.Instance.WasFired = false;
        OrderSession.Instance.ComplainCounter = 0;
        OrderSession.Instance.DailyComplainOccurred = false;
        OrderSession.Instance.DailyBossOccurred = false;
        OrderSession.Instance.CustomersServedToday = 0;
        OrderSession.Instance.DailyTipTotal = 0;

        Debug.Log("[EndingSceneController] 게임 진행 데이터 초기화 완료 -> 메인 화면으로 이동");
    }
}
