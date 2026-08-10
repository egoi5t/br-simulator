using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 화면③ 씬(CupSelectionScene)을 1차(용기 선택)/2차(포장) 두 가지 모드로 전환하는 지휘자.
/// 이 씬은 플레이 루프에서 두 번 쓰임 (주문 접수 직후, 제작 완료 직후).
/// 씬 진입 시점에 어떤 모드인지 판단해서 관련 UI/로직을 켜고 끕니다.
///
/// 모드 판단 기준: OrderSession.Instance.FilledCupSprite가 있으면 -> 2차(포장) 모드
///              없으면 -> 1차(용기 선택) 모드
///
/// 인스펙터 연결:
/// - Shelf Area: 1차에서만 보이는 컵 선반 오브젝트
/// - Lid Area: 2차에서만 보이는 뚜껑 선반 오브젝트
/// - Bag Object: 2차에서만 보이는 쇼핑백 오브젝트
/// - Cup Selection Controller / Packaging Controller: 각 로직 스크립트
/// - Next Button / Next Button Label: 모드에 따라 라벨이 바뀌는 버튼
///   (2차에서는 포장 완료가 쇼핑백 클릭으로 처리되므로, 버튼은 라벨만 "체크아웃"으로
///    바뀌고 클릭 기능은 비활성화됩니다)
/// </summary>
public class CheckoutSceneModeController : MonoBehaviour
{
    [Header("모드별 영역")]
    public GameObject shelfArea;
    public GameObject lidArea;
    public GameObject bagObject;
    [Tooltip("TableCupImage + FlavorSlot들을 묶은 부모 오브젝트. 2차(포장)에서만 보여야 함")]
    public GameObject packagingCupArea;

    [Header("로직 컴포넌트")]
    public CupSelectionSceneController cupSelectionController;
    public PackagingSceneController packagingController;

    [Header("버튼")]
    public Button nextButton;
    public TMP_Text nextButtonLabel;

    private bool isPackagingPhase;

    private void Awake()
    {
        isPackagingPhase = CraftResultSession.Instance.ContainerIndex > 0
                            && CraftResultSession.Instance.FlavorIds != null
                            && CraftResultSession.Instance.FlavorIds.Count > 0;

        // 영역 표시
        if (shelfArea != null) shelfArea.SetActive(!isPackagingPhase);
        if (packagingCupArea != null) packagingCupArea.SetActive(isPackagingPhase);

        // 뚜껑/쇼핑백은 항상 "보이고 클릭도 되게" 둠. 잘못된 타이밍에 누르면
        // 각 스크립트(SelectLid/TryPackageIntoBag) 안에서 경고를 띄우고 무시하는 방식으로 처리.
        if (lidArea != null) lidArea.SetActive(true);
        if (bagObject != null) bagObject.SetActive(true);

        // 로직 컴포넌트 활성화. Awake 시점에 처리해야 각 컨트롤러의 Start()가
        // 올바른 모드로 실행되거나(활성화된 쪽) 아예 안 실행됨(비활성화된 쪽).
        if (cupSelectionController != null) cupSelectionController.enabled = !isPackagingPhase;
        if (packagingController != null) packagingController.enabled = isPackagingPhase;

        // 버튼 라벨/기능 - 어느 모드든 항상 클릭은 되게 두고, 준비 안 됐으면
        // 각 컨트롤러(OnNextButtonPressed / GoToNextScene) 안에서 경고 처리
        if (nextButton != null)
        {
            nextButton.interactable = true;
            nextButton.onClick.RemoveAllListeners();
        }

        if (isPackagingPhase)
        {
            if (nextButtonLabel != null) nextButtonLabel.text = "포장 완료";

            if (nextButton != null)
            {
                nextButton.onClick.AddListener(() =>
                {
                    if (packagingController != null)
                        packagingController.GoToNextScene(); // 포장 안 끝났으면 컨트롤러 내부에서 경고 처리
                });
            }
        }
        else
        {
            if (nextButtonLabel != null) nextButtonLabel.text = "맛 선택으로";
            // Next Button의 onClick은 CupSelectionSceneController.Start()에서
            // OnNextButtonPressed를 등록함 (1차 모드일 때만 그 컴포넌트가 활성화되므로)
        }

        Debug.Log($"[CheckoutSceneModeController] 모드: {(isPackagingPhase ? "2차(포장)" : "1차(용기 선택)")}");
    }

    private void Start()
    {
        StartCoroutine(ForceButtonStateNextFrame());
    }

    /// <summary>
    /// 다른 스크립트의 Start()가 실행 순서상 이 스크립트보다 늦게 돌면서
    /// nextButton.interactable을 덮어써버리는 경우를 대비한 안전장치.
    /// 한 프레임 기다렸다가 버튼이 항상 클릭 가능한 상태인지 다시 한번 확인.
    /// </summary>
    private IEnumerator ForceButtonStateNextFrame()
    {
        yield return null; // 한 프레임 대기 (모든 Start()가 끝난 뒤)

        if (nextButton != null)
        {
            nextButton.interactable = true;
        }
    }
}