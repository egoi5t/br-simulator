using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 화면③ 2차 사용(포장) 씬의 컨트롤러.
/// 1차 사용(용기 선택)의 CupSelectionSceneController와는 완전히 별개 스크립트.
/// 같은 씬 안에서 공존하되, 지휘자 스크립트(추후 만들 CheckoutFlowController)가
/// 상황에 따라 이 컴포넌트를 켜고 끄는 방식으로 쓸 예정.
///
/// 흐름:
/// 1. Start()에서 GameSessionData.FilledCupSprite를 테이블에 표시
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

    [Header("뚜껑 처리")]
    public CupVisualData visualData;
    [Tooltip("날아가는 임시 뚜껑 오브젝트를 어디에 넣을지. 비워두면 Table Cup Image의 부모를 자동 사용")]
    public RectTransform flyingEffectParent;
    public float lidFlyDuration = 0.3f;

    [Header("쇼핑백 처리")]
    public float bagFlyDuration = 0.3f;
    [Tooltip("쇼핑백에 담긴 후 보여줄 이미지. 사이즈 상관없이 공용 이미지 하나만 씀")]
    public Sprite baggedSprite;

    [Header("테스트용")]
    [Tooltip("GameSessionData.FilledCupSprite가 비어있을 때(화면②ㅁ미연결) 대신 보여줄 더미 이미지")]
    public Sprite fallbackSprite;
    [Tooltip("1차 화면을 안 거치고 이 씬만 단독으로 테스트할 때 체크. GameSessionData.SelectedCupSize를 강제로 지정함")]
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
            GameSessionData.SelectedCupSize = debugSize;
            Debug.Log($"[테스트 모드] SelectedCupSize를 {debugSize}(으)로 강제 지정했습니다. " +
                      "실제 플레이 흐름 테스트할 땐 Use Debug Size 체크 해제하세요.");
        }

        DisplayFilledCup();
        CheckLidRequirement();
    }

    /// <summary>이 사이즈가 뚜껑을 쓰는 사이즈인지 미리 확인해서 lidRequired에 기억.</summary>
    private void CheckLidRequirement()
    {
        CupSize? size = GameSessionData.SelectedCupSize;
        var entry = (visualData != null && size.HasValue) ? visualData.GetEntry(size.Value) : null;

        // 데이터가 없으면 일단 "필요하다"고 가정 (안전한 쪽으로)
        lidRequired = entry == null || entry.hasLid;

        if (!lidRequired)
        {
            Debug.Log($"{size.Value}는 뚜껑이 필요 없는 사이즈입니다. " +
                      "뚜껑 UI는 그대로 보이지만, 클릭 안 해도 포장 진행에는 지장 없습니다.");
        }
    }

    /// <summary>화면②에서 넘어온 완성 이미지를 테이블에 표시</summary>
    private void DisplayFilledCup()
    {
        Sprite spriteToShow = GameSessionData.FilledCupSprite != null
            ? GameSessionData.FilledCupSprite
            : fallbackSprite;

        if (spriteToShow == null)
        {
            Debug.LogWarning("PackagingSceneController: 표시할 완성 컵 이미지가 없습니다. " +
                              "화면②에서 GameSessionData.FilledCupSprite를 설정했는지, " +
                              "혹은 Fallback Sprite를 임시로 넣었는지 확인하세요.");
            return;
        }

        tableCupImage.sprite = spriteToShow;
    }

    /// <summary>LidClickable이 뚜껑 클릭 시 호출. 정답/오답 판정 후 진행.</summary>
    /// <param name="chosenSize">클릭한 뚜껑의 사이즈</param>
    /// <param name="sourceRect">클릭한 뚜껑 자신의 RectTransform (날아가는 애니메이션 출발점)</param>
    public void SelectLid(CupSize chosenSize, RectTransform sourceRect)
    {
        if (IsLidOn) return; // 이미 덮여있으면 중복 처리 방지

        CupSize? correctSize = GameSessionData.SelectedCupSize;

        if (!correctSize.HasValue)
        {
            Debug.LogWarning("PackagingSceneController: GameSessionData.SelectedCupSize가 비어있어 " +
                              "정답 판정을 할 수 없습니다. 1차 화면에서 컵을 먼저 선택하고 오세요.");
            return;
        }

        if (chosenSize != correctSize.Value)
        {
            // 오답: 지금은 경고만 출력. 나중에 complainCounter 연동 지점.
            Debug.LogWarning($"잘못된 뚜껑을 선택했습니다. (정답: {correctSize.Value}, 선택: {chosenSize}) " +
                              "-> complainCounter++ 연동 필요");
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
            // 뚜껑이 안 덮인 상태로 쇼핑백을 누른 경우 -> 지금은 경고만. complainCounter 연동 지점.
            Debug.LogWarning("뚜껑이 덮이지 않아 쇼핑백에 넣을 수 없습니다. -> complainCounter++ 연동 필요");
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

        CupSize? size = GameSessionData.SelectedCupSize; // 로그/추후 확장용으로만 참고

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

        Debug.Log("포장 완료! -> 주문 처리 정산 시작 지점 (complainCounter/bossCounter 판정 여기서 연동 필요)");

        // TODO: 여기서 다음 단계로 이어지는 처리
        // 예) 화면①로 돌아가서 손님에게 제공하는 씬 전환
        // 예) SceneManager.LoadScene("OrderScene");
    }
}