using UnityEngine;
using UnityEngine.UI;

// 게임방법 패널 "안"에 붙이는 스크립트. 패널을 열고 닫는 건 MainMenuManager가 담당하고,
// 이 스크립트는 패널이 열려있는 동안 이미지 페이지만 순서대로 넘겨줌.
public class HowToPlayController : MonoBehaviour
{
    [Header("보여줄 이미지들 (순서대로 등록)")]
    public Sprite[] pages;

    [Header("UI 연결")]
    public Image displayImage;
    public Button nextButton;
    public Button prevButton;   // 이전 페이지 버튼. 필요 없으면 비워둬도 됨
    public Text pageIndicatorText; // TextMeshPro 쓰면 TMP_Text로 타입만 바꿔서 쓰면 됨. 필요 없으면 비워둬도 됨

    private int currentIndex = 0;

    void OnEnable()
    {
        // 패널이 열릴 때마다(꺼졌다 켜질 때마다) 항상 첫 페이지부터 다시 보여줌
        currentIndex = 0;
        UpdateDisplay();
    }

    // "다음" 버튼 OnClick에 연결
    public void OnClickNext()
    {
        if (pages == null || currentIndex >= pages.Length - 1) return;
        currentIndex++;
        UpdateDisplay();
    }

    // "이전" 버튼 OnClick에 연결 (안 쓰면 버튼 자체를 안 만들어도 됨)
    public void OnClickPrev()
    {
        if (currentIndex <= 0) return;
        currentIndex--;
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (pages == null || pages.Length == 0 || displayImage == null) return;

        displayImage.sprite = pages[currentIndex];

        // 첫 페이지/마지막 페이지에서 버튼 비활성화 (양쪽 다 원치 않으면 이 부분 지워도 됨)
        if (prevButton != null)
            prevButton.interactable = currentIndex > 0;

        if (nextButton != null)
            nextButton.interactable = currentIndex < pages.Length - 1;

        if (pageIndicatorText != null)
            pageIndicatorText.text = $"{currentIndex + 1} / {pages.Length}";
    }
}