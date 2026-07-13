using UnityEngine;

/// <summary>특수 카드가 고를 수 있는 효과 종류. 새 효과는 여기에 추가.</summary>
public enum SpecialEffect
{
    None,
    ForceDraw,     // 플레잉 조커: 결과를 무승부로
    RandomNumber,  // 다빈치 조커: 승부 시 숫자가 0~13 랜덤으로 정해짐
    // 예정: PlusTwo, RevealEnemy, Cancel ...
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

            // case SpecialEffect.PlusTwo:
            //     if (isPlayer) result.PlayerNumber += 2; else result.EnemyNumber += 2;
            //     break;

            case SpecialEffect.None:
            default:
                break;
        }
    }
}
