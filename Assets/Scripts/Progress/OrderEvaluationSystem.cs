using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 주문 처리 점수 산정 시스템 (기획서 "주문 처리 점수 산정 시스템 플로우 차트" 그대로 구현).
///
/// 판정 순서:
/// 1. 메뉴(사이즈) 확인 - 주문받은 사이즈와 실제로 고른 사이즈가 같은가?
/// 2. 맛 확인 - 올바르게 맛을 담았는가? (화면② 미연동 상태라 지금은 항상 통과 처리, TODO)
/// 3. 시간 확인 - 소요시간 30초 미만이면 패널티 없음, 30~60초면 컴플레인,
///    60초 이상이면 컴플레인 + bossCounter까지 증가
/// 4. 위에서 쌓인 complainCounter 값에 따라 최종 케이스 판정 (Case 0~3)
///
/// 참고: "좋은 피자, 위대한 피자" 류 게임의 평가 방식(주문-결과 비교 + 시간 압박 -> 보상 차등)과
/// 같은 원리를 기획서 자체 설계(complainCounter/bossCounter)로 구현한 것.
/// </summary>
public static class OrderEvaluationSystem
{
    /// <summary>최종 판정 결과. 기획서의 Case 0~3에 대응.</summary>
    public enum Outcome
    {
        NoProblem,          // Case 0: 문제 없음
        NoTip,              // Case 1: 팁 없음
        NoTipNoPay,         // Case 2: 팁 없음 + 페이 없음
        BossAngry           // Case 3: 앞선 패널티 전부 + bossCounter++
    }

    /// <summary>
    /// 포장 완료 시점에 호출. 지금까지 쌓인 정보를 바탕으로 최종 판정을 내리고
    /// OrderSession의 카운터/팁/일일 정산 데이터를 갱신한다.
    /// </summary>
    public static Outcome Evaluate()
    {
        var session = OrderSession.Instance;
        session.LastOrderBossAngered = false; // 이번 주문의 보스 화남 여부 초기화

        // 기획상 평가 요소는 용기/맛/시간 3가지뿐 (실시간 미스는 페널티 없음)
        int complainCount = session.ComplainCounter; // 미스 미부과라 사실상 0에서 시작

        // 1. 메뉴(사이즈) 확인 - 스냅샷 vs 실제로 만든 사이즈(CraftResultSession)
        bool menuCorrect = CheckMenuCorrect();
        if (!menuCorrect)
        {
            complainCount++;
            Debug.Log("[평가] 메뉴(사이즈) 불일치 -> complainCount++");
        }

        // 2. 맛 확인 - 스냅샷 vs 실제로 담은 맛(CraftResultSession)
        bool flavorCorrect = CheckFlavorCorrect();
        if (!flavorCorrect)
        {
            complainCount++;
            Debug.Log("[평가] 맛 불일치 -> complainCount++");
        }

        // 3. 시간 확인
        float elapsed = session.GetElapsedTime();
        if (elapsed >= 60f)
        {
            session.RegisterBossAnger();
            complainCount++;
            Debug.Log($"[평가] 소요시간 {elapsed:F1}초 (60초 이상) -> complainCount++, bossCounter++");
        }
        else if (elapsed >= 30f)
        {
            complainCount++;
            Debug.Log($"[평가] 소요시간 {elapsed:F1}초 (30~60초) -> complainCount++");
        }
        else
        {
            Debug.Log($"[평가] 소요시간 {elapsed:F1}초 (30초 미만) -> 시간 패널티 없음");
        }

        // 4. 최종 케이스 판정
        Outcome outcome;
        switch (complainCount)
        {
            case 0:
                outcome = Outcome.NoProblem;
                break;
            case 1:
                outcome = Outcome.NoTip;
                break;
            case 2:
                outcome = Outcome.NoTipNoPay;
                break;
            default: // 3 이상
                session.RegisterBossAnger();
                outcome = Outcome.BossAngry;
                break;
        }

        // 5. 팁 계산 - 실수가 하나라도 있으면 팁 없음, 없으면 소요시간 기준 차등 지급
        int tip = complainCount > 0 ? 0 : CalculateTip(elapsed);
        session.LastOrderTip = tip;
        session.DailyTipTotal += tip;
        session.CustomersServedToday++;

        if (complainCount > 0)
            session.DailyComplainOccurred = true;

        Debug.Log($"[평가] 최종 판정: {outcome} (complainCount={complainCount}, 팁={tip}원, " +
                  $"오늘 누적 팁={session.DailyTipTotal}원, 오늘 처리한 손님={session.CustomersServedToday}명, " +
                  $"누적 bossCounter={session.BossCounter})");

        // 기획서: "각각의 손님이 가짐. 즉, 한 손님의 주문이 끝날 경우 0으로 초기화 한다."
        session.ComplainCounter = 0;

        return outcome;
    }

    /// <summary>
    /// 소요시간 기준 팁 계산. 3초 구간마다 1000원씩 차등 (1구간=10000원 ~ 10구간=1000원, 30초 초과는 0원).
    /// 기획서 "일일 정산 시스템 개요" 팁 테이블 그대로 구현.
    /// </summary>
    private static int CalculateTip(float elapsedSeconds)
    {
        int band = Mathf.CeilToInt(elapsedSeconds / 3f);
        if (band < 1) band = 1;
        return Mathf.Max(0, 11 - band) * 1000;
    }

    /// <summary>1차 화면에서 떠둔 스냅샷(주문한 사이즈) vs 실제로 만든 사이즈(CraftResultSession)가 같은지 확인.</summary>
    private static bool CheckMenuCorrect()
    {
        CupSize? orderedSize = OrderSession.Instance.SnapshotOrderedCupSize;
        int containerIndex = CraftResultSession.Instance.ContainerIndex;

        if (!orderedSize.HasValue || containerIndex <= 0)
        {
            Debug.LogWarning($"[평가/사이즈] 데이터 비어있음 - SnapshotOrderedCupSize={orderedSize}, " +
                              $"CraftResultSession.ContainerIndex={containerIndex} -> 확인 건너뜀 (통과 처리)");
            return true; // 데이터 없으면 일단 통과 처리 (테스트 편의)
        }

        CupSize actualSize = (CupSize)(containerIndex - 1);
        bool isMatch = orderedSize.Value == actualSize;

        Debug.Log($"[평가/사이즈] 주문: {orderedSize.Value} (스냅샷) / 실제 제작: {actualSize} " +
                  $"(ContainerIndex={containerIndex}) -> {(isMatch ? "일치 ✅" : "불일치 ❌")}");

        return isMatch;
    }

    /// <summary>1차 화면에서 떠둔 스냅샷(주문한 맛) vs 실제로 담은 맛(CraftResultSession)이 같은지 확인 (순서 무관).</summary>
    private static bool CheckFlavorCorrect()
    {
        var orderedFlavors = OrderSession.Instance.SnapshotOrderedFlavorIds;
        var actualFlavors = CraftResultSession.Instance.FlavorIds;

        if (orderedFlavors == null || actualFlavors == null)
        {
            Debug.LogWarning($"[평가/맛] 데이터 비어있음 - SnapshotOrderedFlavorIds={(orderedFlavors == null ? "null" : "있음")}, " +
                              $"CraftResultSession.FlavorIds={(actualFlavors == null ? "null" : "있음")} -> 확인 건너뜀 (통과 처리)");
            return true;
        }

        string orderedStr = string.Join(", ", orderedFlavors);
        string actualStr = string.Join(", ", actualFlavors);

        if (orderedFlavors.Count != actualFlavors.Count)
        {
            Debug.Log($"[평가/맛] 주문: [{orderedStr}] ({orderedFlavors.Count}개) / " +
                      $"실제 제작: [{actualStr}] ({actualFlavors.Count}개) -> 개수 불일치 ❌");
            return false;
        }

        var sortedOrdered = new List<string>(orderedFlavors);
        var sortedActual = new List<string>(actualFlavors);
        sortedOrdered.Sort();
        sortedActual.Sort();

        for (int i = 0; i < sortedOrdered.Count; i++)
        {
            if (sortedOrdered[i] != sortedActual[i])
            {
                Debug.Log($"[평가/맛] 주문: [{orderedStr}] / 실제 제작: [{actualStr}] " +
                          $"-> 내용 불일치 ❌ ('{sortedOrdered[i]}' vs '{sortedActual[i]}'에서 다름)");
                return false;
            }
        }

        Debug.Log($"[평가/맛] 주문: [{orderedStr}] / 실제 제작: [{actualStr}] -> 일치 ✅");
        return true;
    }
}