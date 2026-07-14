using UnityEngine;

/// <summary>특수 카드가 고를 수 있는 효과 종류. 새 효과는 여기에 추가.</summary>
public enum SpecialEffect
{
    None,
    ForceDraw,     // 플레잉 조커: 결과를 무승부로
    RandomNumber,  // 다빈치 조커: 승부 시 숫자가 0~13 랜덤으로 정해짐
    DrawTwo,       // 드로우 2: 상대가 전 턴에 특수카드를 냈으면 무승부+다음 라운드 2배, 아니면 숫자 2
    Reverse,       // 리버스: 2로 간주. 상대 숫자카드면 교환, 상대 특수면 교환 없이 패배
    // 예정: RevealEnemy, Cancel ...
}

/// <summary>
/// 특수 카드 효과의 실제 로직 모음(중앙 집중). SpecialCardData는 여기서 함수를 가져다 쓴다.
/// 효과를 추가하려면 (1) SpecialEffect에 값 추가 (2) 아래 switch에 case 추가만 하면 된다.
/// </summary>
public static class SpecialCardEffects
{
    /// <summary>승부 판정에 효과를 적용. isPlayer = 이 카드를 낸 쪽이 플레이어인지.</summary>
    public static void ApplyOnShowdown(SpecialEffect effect, ShowdownResult result, bool isPlayer)
    {
        switch (effect)
        {
            case SpecialEffect.ForceDraw:
                result.ForceDraw = true;
                break;

            case SpecialEffect.RandomNumber:
            {
                int roll = Random.Range(0, 14); // 0~13
                if (isPlayer) result.PlayerNumber = roll;
                else result.EnemyNumber = roll;
                Debug.Log($"[특수] 다빈치 조커({(isPlayer ? "플레이어" : "적")}) → 숫자 {roll}");
                break;
            }

            case SpecialEffect.DrawTwo:
            {
                // '상대'(이 카드를 낸 쪽의 반대편)가 전 턴에 특수카드를 냈는지.
                bool opponentSpecialLast = isPlayer ? result.EnemyPlayedSpecialLast
                                                    : result.PlayerPlayedSpecialLast;
                string who = isPlayer ? "플레이어" : "적";
                if (opponentSpecialLast)
                {
                    result.ForceDraw = true; // 이번 판은 무승부
                    if (isPlayer) result.PlayerDoubleNextRound = true;
                    else result.EnemyDoubleNextRound = true;
                    Debug.Log($"[특수] 드로우 2({who}) → 조건 충족: 이번 무승부, 다음 라운드 점수 2배 예약");
                }
                else
                {
                    if (isPlayer) result.PlayerNumber = 2; else result.EnemyNumber = 2;
                    Debug.Log($"[특수] 드로우 2({who}) → 조건 불만족: 숫자 2로 간주");
                }
                break;
            }

            case SpecialEffect.Reverse:
            {
                string who = isPlayer ? "플레이어" : "적";
                // 리버스는 2로 간주.
                if (isPlayer) result.PlayerNumber = 2; else result.EnemyNumber = 2;

                bool opponentSpecial = isPlayer ? result.EnemyCardIsSpecial : result.PlayerCardIsSpecial;
                if (opponentSpecial)
                {
                    // 상대가 특수카드 → 교환 없이 리버스 낸 쪽 패배.
                    result.ForceOutcome = true;
                    result.ForcedCmp = isPlayer ? -1 : 1;
                    Debug.Log($"[특수] 리버스({who}) → 상대가 특수카드: 교환 없이 패배");
                }
                else
                {
                    // 상대 숫자카드 → 두 카드 교환 후 승부.
                    int tmp = result.PlayerNumber;
                    result.PlayerNumber = result.EnemyNumber;
                    result.EnemyNumber = tmp;
                    result.SwapCards = true;
                    Debug.Log($"[특수] 리버스({who}) → 상대 숫자카드와 교환 후 승부");
                }
                break;
            }

            // case SpecialEffect.RevealEnemy:
            //     if (isPlayer) result.PlayerNumber += 2; else result.EnemyNumber += 2;
            //     break;

            case SpecialEffect.None:
            default:
                break;
        }
    }
}
