using UnityEngine;
using UnityEngine.SceneManagement;

// 씬에 빈 GameObject 만들어서 이 스크립트 붙이고,
// 버튼 3개의 OnClick()에 각각 아래 메서드를 연결하면 됩니다.
public class MainMenuManager : MonoBehaviour
{
    [Header("씬 전환")]
    public string orderSceneName = "OrderScene"; // Build Settings에 등록된 이름과 동일해야 함

    [Header("환경설정 패널 (같은 씬 안의 UI 패널을 껐다 켰다 하는 방식)")]
    public GameObject settingsPanel; // 비활성화 상태로 시작해두면 됨

    [Header("게임방법 패널 (같은 씬 안의 UI 패널을 껐다 켰다 하는 방식)")]
    public GameObject howToPlayPanel; // 비활성화 상태로 시작해두면 됨

    void Start()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (howToPlayPanel != null)
            howToPlayPanel.SetActive(false);
    }

    // 1) OrderScene으로 이동
    public void OnClickPlay()
    {
        SceneManager.LoadScene(orderSceneName);
    }

    // 2) 환경설정 패널 열기
    public void OnClickOpenSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(true);
    }

    // 2-1) 환경설정 패널 닫기 (패널 안의 '닫기'/'뒤로' 버튼에 연결)
    public void OnClickCloseSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    // 4) 게임방법 패널 열기
    public void OnClickOpenHowToPlay()
    {
        if (howToPlayPanel != null)
            howToPlayPanel.SetActive(true);
    }

    // 4-1) 게임방법 패널 닫기 (패널 안의 '닫기'/'뒤로' 버튼에 연결)
    public void OnClickCloseHowToPlay()
    {
        if (howToPlayPanel != null)
            howToPlayPanel.SetActive(false);
    }

    // 3) 게임 종료
    public void OnClickQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // 에디터에서 Play 모드 종료
#else
        Application.Quit(); // 실제 빌드에서 앱 종료
#endif
    }
}