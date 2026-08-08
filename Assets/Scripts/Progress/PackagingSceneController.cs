using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// 화면③ 2차 사용(포장) 씬의 컨트롤러.
/// 1차 사용(용기 선택)의 CupSelectionSceneController와는 완전히 별개 스크립트.
/// 같은 씬 안에서 공존하되, 지휘자 스크립트(추후 만들 CheckoutFlowController)가
/// 상황에 따라 이 컴포넌트를 켜고 끄는 방식으로 쓸 예정.
///
/// 흐름:
/// 1. Start()에서 OrderSession.Instance.FilledCupSprite를 테이블에 표시
///    (화면②가 아직 연결 안 됐으면 fallbackSprite로 대체)
/// 2. 동시에 지금 사이즈가 뚜껑을 쓰는 사이즈인지 확인해서 lidRequired에 기억
///    (UI는 항상 그대로 보임 - 숨기지 않음)
/// 3. LidClickable이 뚜껑 클릭을 감지하면 SelectLid() 호출 -> 정답/오답 판정
/// 4. 정답이면 애니메이션 후 뚜껑 덮힌 이미지로 교체, IsLidOn = true
/// 5. (다음 단계) 쇼핑백 스크립트는 IsLidOn이 아니라 IsReadyForBag을 체크해야 함
///    -> 뚜껑이 필요 없는 사이즈면 클릭 안 해도 자동으로 true
///
/// 인스펙터 설정:
/// - Table Cup Image: 테이블에 표시될 완성 컵의 Image 컴포넌트
/// - Visual Data: CupVisualData 에셋 (뚜껑 덮힌 이미지 조회용)
/// - Lid Fly Start Point: 뚜껑이 날아오기 시작할 위치 (뚜껑 스택 근처)
/// - Fallback Sprite: 화면②가 아직 없을 때 테스트용으로 보여줄 더미 이미지
/// </summary>
public class PackagingSceneController : MonoBehaviour
{
    [Header("테이블의 완성 컵")]
    [Tooltip("테이블에 이미 놓여있는 완성 컵의 Image 컴포넌트를 직접 연결")]
    public Image tableCupImage;
    [Tooltip("맛 데이터로 컵 이미지를 직접 합성하는 컴포넌트")]
    public FilledCupVisualComposer visualComposer;

    [Header("뚜껑 처리")]
    public CupVisualData visualData;
    [Tooltip("날아가는 임시 뚜껑 오브젝트를 어디에 넣을지. 비워두면 Table Cup Image의 부모를 자동 사용")]
    public RectTransform flyingEffectParent;
    public float lidFlyDuration = 0.3f;

    [Header("쇼핑백 처리")]
    public float bagFlyDuration = 0.3f;
    [Tooltip("쇼핑백에 담긴 후 보여줄 이미지. 사이즈 상관없이 공용 이미지 하나만 씀")]
    public Sprite baggedSprite;

    [Header("씬 전환")]
    [Tooltip("포장 완료 후 돌아갈 화면①(주문/손님) 씬 이름. Build Profiles에 등록된 이름과 정확히 일치해야 함")]
    public string orderSceneName = "OrderScene";
    [Tooltip("체크하면 포장 완료 시 씬 전환을 하지 않음 (이 씬만 단독 테스트할 때)")]
    public bool skipSceneTransitionForTest = true;

    [Header("테스트용")]
    [Tooltip("Composer가 아직 없거나 CraftResultSession이 비어있을 때 대신 보여줄 더미 이미지")]
    public Sprite fallbackSprite;
    [Tooltip("2차 화면만 단독으로 테스트할 때 체크. CraftResultSession을 강제로 채움")]
    public bool useDebugSize = false;
    public CupSize debugSize = CupSize.Goko;

    /// <summary>이 사이즈가 애초에 뚜껑을 쓰는 사이즈인지. CheckLidRequirement()에서 결정됨.</summary>
    private bool lidRequired = true;

    /// <summary>실제로 뚜껑을 클릭해서 덮었는지 여부.</summary>
    public bool IsLidOn { get; private set; } = false;

    /// <summary>
    /// 쇼핑백 스크립트가 확인해야 할 최종 값.
    /// 뚜껑이 필요한 사이즈면 IsLidOn을 그대로 보고,
    /// 애초에 뚜껑이 필요 없는 사이즈면 클릭 여부와 상관없이 통과시킴.
    /// </summary>
    public bool IsReadyForBag => !lidRequired || IsLidOn;

    /// <summary>쇼핑백에 담겨 포장이 완전히 끝났는지 여부.</summary>
    public bool IsPackagingComplete { get; private set; } = false;

    private void Start()
    {
        if (useDebugSize)
        {
            var dummyFlavors = new List<string> { "FLV-001", "FLV-005", "FLV-007" };
            CraftResultSession.Instance.SetResult((int)debugSize + 1, dummyFlavors);
            Debug.Log($"[테스트 모드] CraftResultSession을 {debugSize}(으)로 강제 지정했습니다. " +
                      "실제 플레이 흐름 테스트할 땐 Use Debug Size 체크 해제하세요.");
        }

        DisplayFilledCup();
        CheckLidRequirement();
    }

    /// <summary>이 사이즈가 뚜껑을 쓰는 사이즈인지 미리 확인해서 lidRequired에 기억.</summary>
    private void CheckLidRequirement()
    {
        int containerIndex = CraftResultSession.Instance.ContainerIndex;

        if (containerIndex <= 0)
        {
            Debug.LogWarning("PackagingSceneController: CraftResultSession.ContainerIndex가 비어있어 " +
                              "뚜껑 필요 여부를 판단할 수 없습니다. 일단 '뚜껑 필요함'으로 처리합니다.");
            lidRequired = true;
            return;
        }

        CupSize size = (CupSize)(containerIndex - 1);
        var entry = visualData != null ? visualData.GetEntry(size) : null;

        // 데이터가 없으면 일단 "필요하다"고 가정 (안전한 쪽으로)
        lidRequired = entry == null || entry.hasLid;

        if (!lidRequired)
        {
            Debug.Log($"{size}는 뚜껑이 필요 없는 사이즈입니다. " +
                      "뚜껑 UI는 그대로 보이지만, 클릭 안 해도 포장 진행에는 지장 없습니다.");
        }
    }

    /// <summary>화면②에서 넘어온 맛 데이터로 완성 컵을 합성해서 표시</summary>
    private void DisplayFilledCup()
    {
        if (visualComposer != null)
        {
            visualComposer.Compose();
        }
        else if (fallbackSprite != null)
        {
            // Composer가 아직 안 붙어있을 때 테스트용으로 최소한의 이미지라도 보여줌
            tableCupImage.sprite = fallbackSprite;
        }
        else
        {
            Debug.LogWarning("PackagingSceneController: Visual Composer도 Fallback Sprite도 없어서 " +
                              "완성 컵을 표시할 수 없습니다.");
        }
    }

    /// <summary>LidClickable이 뚜껑 클릭 시 호출. 정답/오답 판정 후 진행.</summary>
    /// <param name="chosenSize">클릭한 뚜껑의 사이즈</param>
    /// <param name="sourceRect">클릭한 뚜껑 자신의 RectTransform (날아가는 애니메이션 출발점)</param>
    public void SelectLid(CupSize chosenSize, RectTransform sourceRect)
    {
        if (IsLidOn) return; // 이미 덮여있으면 중복 처리 방지

        int containerIndex = CraftResultSession.Instance.ContainerIndex;

        if (containerIndex <= 0)
        {
            Debug.LogWarning("PackagingSceneController: CraftResultSession.ContainerIndex가 비어있어 " +
                              "정답 판정을 할 수 없습니다.");
            return;
        }

        CupSize correctSize = (CupSize)(containerIndex - 1);

        if (chosenSize != correctSize)
        {
            OrderSession.Instance.RegisterComplaint();
            Debug.LogWarning($"잘못된 뚜껑을 선택했습니다. (정답: {correctSize}, 선택: {chosenSize}) -> complainCounter++");
            return;
        }

        // 정답인 경우에만 실제로 진행
        StartCoroutine(LidFlyAndApply(chosenSize, sourceRect));
    }

    private IEnumerator LidFlyAndApply(CupSize size, RectTransform sourceRect)
    {
        var entry = visualData != null ? visualData.GetEntry(size) : null;
        RectTransform targetRect = tableCupImage.GetComponent<RectTransform>();

        // 날아가는 동안 보여줄 임시 뚜껑 오브젝트 생성
        GameObject flyingLid = null;
        if (entry != null && entry.lidStackSprite != null)
        {
            Transform parent = flyingEffectParent != null ? flyingEffectParent : targetRect.parent;

            flyingLid = new GameObject("FlyingLid", typeof(RectTransform), typeof(Image));
            flyingLid.transform.SetParent(parent, worldPositionStays: false);

            var flyingRect = flyingLid.GetComponent<RectTransform>();
            flyingRect.sizeDelta = sourceRect.sizeDelta;
            flyingRect.position = sourceRect.position; // 클릭한 뚜껑 위치에서 시작

            var flyingImage = flyingLid.GetComponent<Image>();
            flyingImage.sprite = entry.lidStackSprite;
            flyingImage.raycastTarget = false; // 날아가는 동안 클릭 방해 안 하도록

            // 출발점(sourceRect.position) -> 도착점(targetRect.position)으로 이동
            Vector3 startPos = sourceRect.position;
            Vector3 endPos = targetRect.position;
            float elapsed = 0f;

            while (elapsed < lidFlyDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / lidFlyDuration;
                flyingRect.position = Vector3.Lerp(startPos, endPos, t);
                yield return null;
            }

            Destroy(flyingLid);
        }
        else
        {
            // 날아가는 이미지가 없어도 최소한 시간차는 재현
            float elapsed = 0f;
            while (elapsed < lidFlyDuration)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        // 뚜껑 덮힌 스프라이트로 교체
        if (entry != null && entry.lidClosedSprite != null)
        {
            tableCupImage.sprite = entry.lidClosedSprite;
        }
        else
        {
            Debug.LogWarning("PackagingSceneController: 뚜껑 덮힌 스프라이트를 찾지 못했습니다. " +
                              "CupVisualData의 Lid Closed Sprite가 채워져 있는지 확인하세요.");
        }

        IsLidOn = true;
    }

    /// <summary>BagClickable이 쇼핑백 클릭 시 호출.</summary>
    /// <param name="bagRect">쇼핑백 오브젝트 자신의 RectTransform (컵이 빨려들어갈 도착점)</param>
    public void TryPackageIntoBag(RectTransform bagRect)
    {
        if (IsPackagingComplete) return; // 중복 처리 방지

        if (!IsReadyForBag)
        {
            OrderSession.Instance.RegisterComplaint();
            Debug.LogWarning("뚜껑이 덮이지 않아 쇼핑백에 넣을 수 없습니다. -> complainCounter++");
            return;
        }

        StartCoroutine(PackIntoBagAndComplete(bagRect));
    }

    private IEnumerator PackIntoBagAndComplete(RectTransform bagRect)
    {
        // 뚜껑 때와 동일한 패턴: 잠깐의 텀을 두고 스프라이트만 교체
        float elapsed = 0f;
        while (elapsed < bagFlyDuration)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (baggedSprite != null)
        {
            tableCupImage.sprite = baggedSprite;
        }
        else
        {
            Debug.LogWarning("PackagingSceneController: 쇼핑백에 담긴 이미지를 찾지 못했습니다. " +
                              "Bagged Sprite 필드가 채워져 있는지 확인하세요.");
        }

        IsPackagingComplete = true;

        OrderEvaluationSystem.Outcome outcome = OrderEvaluationSystem.Evaluate();
        OrderSession.Instance.LastOrderOutcome = outcome;
        Debug.Log($"포장 완료! 주문 처리 결과: {outcome} -> '체크아웃' 버튼을 누르면 다음으로 넘어갑니다.");
        // 일일 정산 트리거는 여기서 하지 않음. 화면①에서 손님에게 전달까지 끝난 뒤,
        // OrderSession.IsTodayComplete()를 체크해서 DailySettlementScene으로 넘어가는 방식.

        // 다음 손님 주문 때 이 씬이 다시 "1차(용기 선택) 모드"로 진입하도록 초기화
        CraftResultSession.Instance.SetResult(0, new List<string>());
    }

    /// <summary>
    /// "체크아웃" 버튼 클릭 시 호출 (CheckoutSceneModeController가 연결).
    /// 쇼핑백으로 포장이 이미 끝난 상태여야만 다음 씬으로 넘어감.
    /// </summary>
    public void GoToNextScene()
    {
        if (!IsPackagingComplete)
        {
            Debug.LogWarning("아직 포장이 끝나지 않았습니다. 쇼핑백을 먼저 클릭해서 포장을 완료하세요.");
            return;
        }

        if (skipSceneTransitionForTest)
        {
            Debug.Log("[테스트 모드] Skip Scene Transition For Test 체크됨 - 씬 전환 생략");
            return;
        }

        SceneManager.LoadScene(orderSceneName);
    }
}