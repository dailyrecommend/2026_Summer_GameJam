using System.Collections.Generic;

/// <summary>
/// 적 카드 선택 AI. 지금은 TestAI: 항상 숫자가 가장 높은 카드를 고른다.
/// (나중에 눈치싸움/전략 AI로 교체 예정)
/// </summary>
public static class BattleAI
{
    /// <summary>후보 중 숫자가 가장 높은 카드를 선택. 비었으면 null.</summary>
    public static CardData PickHighest(IReadOnlyList<CardData> candidates)
    {
        CardData best = null;
        for (int i = 0; i < candidates.Count; i++)
        {
            CardData c = candidates[i];
            if (c == null) continue;
            if (best == null || c.Number > best.Number) best = c;
        }
        return best;
    }
}
