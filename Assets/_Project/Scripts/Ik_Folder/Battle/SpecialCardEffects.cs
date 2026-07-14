using System.Collections.Generic;
using UnityEngine;

/// <summary>특수 카드가 고를 수 있는 효과 종류. 새 효과는 여기에 추가.</summary>
public enum SpecialEffect
{
    None,
    ForceDraw,     // 플레잉 조커: 결과를 무승부로
    RandomNumber,  // 다빈치 조커: 승부 시 숫자가 0~13 랜덤으로 정해짐
    DrawTwo,       // 드로우 2: 상대가 전 턴에 특수카드를 냈으면 무승부+다음 라운드 2배, 아니면 숫자 2
    Reverse,       // 리버스: 2로 간주. 상대 숫자카드면 교환, 상대 특수면 교환 없이 패배
    Bang,          // Bang!: 상대 필드에 남은 고유카드 하나를 무작위로 파괴하고 승리. 없으면 숫자 4로 간주
    Miss,          // 빗나감: 이번 판 무승부 + 다음 라운드 상대는 이겨도 승점 없음
    Beer,          // 맥주: 상대가 고유카드를 냈으면 승점 회복(승리). 아니면 숫자 3으로 간주
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

            case SpecialEffect.Bang:
            {
                string who = isPlayer ? "플레이어" : "적";
                // 발동 조건: 상대가 이번 판에 고유(특수) 카드를 냄.
                bool opponentPlayedSpecial = isPlayer ? result.EnemyCardIsSpecial : result.PlayerCardIsSpecial;

                if (!opponentPlayedSpecial)
                {
                    if (isPlayer) result.PlayerNumber = 4; else result.EnemyNumber = 4;
                    Debug.Log($"[특수] Bang!({who}) → 상대가 고유카드를 안 냄: 발동 안 함, 숫자 4로 간주");
                    break;
                }

                BattleField targetField = isPlayer ? result.EnemyField : result.PlayerField;
                CardPile targetPile = isPlayer ? result.EnemyPile : result.PlayerPile;

                // 상대 필드에 '남아있는'(이번 판에 안 쓴) 카드 중 고유(특수) 카드만 후보로.
                List<FieldCard> candidates = new List<FieldCard>();
                if (targetField != null)
                {
                    foreach (FieldCard c in targetField.Cards)
                        if (c != null && c.Data is SpecialCardData) candidates.Add(c);
                }

                if (candidates.Count > 0)
                {
                    FieldCard picked = candidates[Random.Range(0, candidates.Count)];
                    Debug.Log($"[특수] Bang!({who}) → 발동(상대 고유카드 냄): '{picked.Data.DisplayName}' 파괴, 승리");
                    targetField.DiscardCard(picked, targetPile);
                    result.ForceOutcome = true;
                    result.ForcedCmp = isPlayer ? 1 : -1;
                }
                else
                {
                    if (isPlayer) result.PlayerNumber = 4; else result.EnemyNumber = 4;
                    Debug.Log($"[특수] Bang!({who}) → 발동했지만 상대 필드에 파괴할 고유카드 없음: 숫자 4로 간주");
                }
                break;
            }

            case SpecialEffect.Miss:
            {
                string who = isPlayer ? "플레이어" : "적";
                result.ForceDraw = true;
                // '상대'가 다음 라운드에 이겨도 승점을 못 얻게.
                if (isPlayer) result.EnemyNoWinNextRound = true;
                else result.PlayerNoWinNextRound = true;
                Debug.Log($"[특수] 빗나감({who}) → 이번 무승부, 다음 라운드 상대 승점 무효 예약");
                break;
            }

            case SpecialEffect.Beer:
            {
                string who = isPlayer ? "플레이어" : "적";
                bool opponentSpecial = isPlayer ? result.EnemyCardIsSpecial : result.PlayerCardIsSpecial;
                if (opponentSpecial)
                {
                    // 승점 회복 = 상대 점수를 1점 깎음(0점이면 그대로). 이 판 자체의 승패는 그대로 숫자로 겨룬다.
                    if (isPlayer) result.PlayerStealPoint = true; else result.EnemyStealPoint = true;
                    Debug.Log($"[특수] 맥주({who}) → 상대가 고유카드 냄: 상대 점수 1점 회수");
                }
                else
                {
                    if (isPlayer) result.PlayerNumber = 3; else result.EnemyNumber = 3;
                    Debug.Log($"[특수] 맥주({who}) → 상대 일반카드: 숫자 3으로 간주");
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
