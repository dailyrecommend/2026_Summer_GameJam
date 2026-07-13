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

    /// <summary>필드 카드 중 숫자가 가장 높은 것을 선택. 비었으면 null.</summary>
    public static FieldCard PickHighestCard(IReadOnlyList<FieldCard> cards)
    {
        FieldCard best = null;
        for (int i = 0; i < cards.Count; i++)
        {
            FieldCard c = cards[i];
            if (c == null || c.Data == null) continue;
            if (best == null || c.Data.Number > best.Data.Number) best = c;
        }
        return best;
    }
}
