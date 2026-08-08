using UnityEngine;

/// <summary>
/// 씬이 전환되어도 유지되어야 하는 게임 진행 데이터를 담는 정적 저장소.
/// MonoBehaviour가 아니라 순수 정적(static) 클래스라서 씬에 오브젝트로 안 놔도 되고,
/// 어디서든 GameSessionData.xxx 처럼 바로 접근 가능.
///
/// 사용 예:
/// - 화면③ 1차(용기 선택)에서: GameSessionData.SelectedCupSize = CupSize.Goko;
/// - 화면②(제작)에서 읽을 때: var size = GameSessionData.SelectedCupSize;
/// </summary>
public static class GameSessionData
{
    /// <summary>1차 용기 선택에서 고른 사이즈. 아직 선택 전이면 null.</summary>
    public static CupSize? SelectedCupSize;

    /// <summary>
    /// 화면②(제작)에서 완성된 아이스크림 이미지.
    /// 제작 담당자가 완성 시점에 이 값을 채워줘야 함:
    ///   GameSessionData.FilledCupSprite = 완성된스프라이트;
    /// 화면③ 2차(포장)에서는 이 값을 읽어서 테이블에 표시.
    /// 아직 제작 파트가 연결 안 됐으면 null이니, 읽는 쪽에서 null 체크 필수.
    /// </summary>
    public static Sprite FilledCupSprite;

    // ────────────────────────────────────────────
    // 주문 처리 타이머 (화면① 접수 ~ 화면③ 포장 완료까지 공유)
    // ────────────────────────────────────────────

    /// <summary>제한 시간(초). 기본 45초.</summary>
    public static float OrderTimeLimit = 45f;

    private static float orderStartTime = -1f;

    /// <summary>
    /// 새 주문이 시작될 때 한 번 호출. (원칙상 화면①에서 주문을 접수하는 순간 호출해야 함)
    /// Time.time 기준이라 씬이 바뀌어도 계속 흐름.
    /// </summary>
    public static void StartOrderTimer(float timeLimit = 45f)
    {
        OrderTimeLimit = timeLimit;
        orderStartTime = Time.time;
    }

    /// <summary>지금까지 지난 시간(초). 타이머가 시작 안 됐으면 0.</summary>
    public static float GetElapsedTime()
    {
        if (orderStartTime < 0f) return 0f;
        return Time.time - orderStartTime;
    }

    /// <summary>남은 시간(초). 0 밑으로는 안 내려감.</summary>
    public static float GetRemainingTime()
    {
        return Mathf.Max(0f, OrderTimeLimit - GetElapsedTime());
    }

    /// <summary>제한 시간을 넘겼는지 여부.</summary>
    public static bool IsTimeUp()
    {
        return orderStartTime >= 0f && GetElapsedTime() >= OrderTimeLimit;
    }

    // 나중에 필요해지면 여기에 계속 추가하면 됩니다.
    // 예: public static List<string> FilledFlavors;
    // 예: public static int ComplainCounter;
}