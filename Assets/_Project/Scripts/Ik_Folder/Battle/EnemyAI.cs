using System.Collections.Generic;
using UnityEngine;

/// <summary>스테이지(보드게임)별 AI 성향. StageData에서 선택. 개별 로직에 해당 없으면 공통 로직으로 대체.</summary>
public enum AIStyle
{
    None,        // 개별 로직 없음 → 공통 로직만 사용
    DavinciCode,
    Uno,
    Bang,
}

/// <summary>
/// 적 AI의 카드 선택 로직. 스테이지별 개별 로직을 먼저 시도하고, 해당하는 규칙이 없으면 공통 로직으로 고른다.
/// 확률은 '지정 카드에 P%, 나머지 후보 카드들에 (100-P)%를 균등 배분'한 가중치 무작위로 뽑는다.
/// </summary>
public static class EnemyAI
{
    /// <summary>hand(내 손패)에서 opponentField(상대 남은 카드) 상태를 보고 한 장을 고른다.</summary>
    public static FieldCard ChooseCard(AIStyle style, IReadOnlyList<FieldCard> hand,
        IReadOnlyList<FieldCard> opponentField, bool opponentPlayedSpecialLastRound)
    {
        if (hand == null || hand.Count == 0) return null;

        List<(FieldCard card, float weight)> weighted = null;
        if (style == AIStyle.DavinciCode) weighted = DavinciCode(hand);
        else if (style == AIStyle.Uno) weighted = Uno(hand, opponentField, opponentPlayedSpecialLastRound);
        else if (style == AIStyle.Bang) weighted = Bang(hand, opponentField);

        if (weighted == null) weighted = Common(hand, opponentField);
        return PickWeighted(weighted);
    }

    // ── 다빈치 코드 ──────────────────────────────────────────
    static List<(FieldCard, float)> DavinciCode(IReadOnlyList<FieldCard> hand)
    {
        List<FieldCard> specials = FilterSpecial(hand);
        if (specials.Count > 0)
            return SplitWeight(hand, specials, 0.65f); // 고유 카드 65%, 나머지 균등 35%

        List<FieldCard> twos = FilterNumber(hand, 2);
        if (twos.Count > 0)
            return SplitWeight(hand, twos, 0.10f); // 고유 카드 없으면 2번 카드 10%, 나머지 균등 90%

        return null; // 해당 없음 → 공통 로직
    }

    // ── 우노 ────────────────────────────────────────────────
    static List<(FieldCard, float)> Uno(IReadOnlyList<FieldCard> hand, IReadOnlyList<FieldCard> opponentField, bool opponentSpecialLast)
    {
        bool opponentHasSpecialNow = HasSpecial(opponentField);

        if (opponentSpecialLast && !opponentHasSpecialNow)
        {
            List<FieldCard> plusTwo = FilterEffect(hand, SpecialEffect.DrawTwo);
            if (plusTwo.Count > 0) return SplitWeight(hand, plusTwo, 0.85f);
        }

        if (!opponentHasSpecialNow)
        {
            List<FieldCard> reverse = FilterEffect(hand, SpecialEffect.Reverse);
            if (reverse.Count > 0) return SplitWeight(hand, reverse, 0.65f);
        }

        return null;
    }

    // ── 뱅! ─────────────────────────────────────────────────
    static List<(FieldCard, float)> Bang(IReadOnlyList<FieldCard> hand, IReadOnlyList<FieldCard> opponentField)
    {
        List<FieldCard> miss = FilterEffect(hand, SpecialEffect.Miss);
        if (miss.Count > 0) return SplitWeight(hand, miss, 0.85f);

        List<FieldCard> bang = FilterEffect(hand, SpecialEffect.Bang);
        List<FieldCard> beer = FilterEffect(hand, SpecialEffect.Beer);
        if (bang.Count > 0 && beer.Count > 0)
        {
            var w = new List<(FieldCard, float)>();
            foreach (FieldCard c in bang) w.Add((c, 0.5f / bang.Count));
            foreach (FieldCard c in beer) w.Add((c, 0.5f / beer.Count));
            return w;
        }

        int opponentSpecialCount = CountSpecial(opponentField);
        if (opponentSpecialCount >= 2 && bang.Count > 0)
            return SplitWeight(hand, bang, 1f); // 상대 고유 2장 이상 → Bang 확정

        // TODO(기획): 더 추가 필요. 지금은 위 조건 다 아니면 공통 로직으로.
        return null;
    }

    // ── 공통 로직 (개별 로직에 해당 없을 때) ───────────────────
    static List<(FieldCard, float)> Common(IReadOnlyList<FieldCard> hand, IReadOnlyList<FieldCard> opponentField)
    {
        bool opponentHasOne = false;
        foreach (FieldCard c in opponentField)
            if (c != null && c.Data != null && c.Data.Number == 1) { opponentHasOne = true; break; }

        if (!opponentHasOne)
        {
            List<FieldCard> sixes = FilterNumber(hand, 6);
            if (sixes.Count > 0) return SplitWeight(hand, sixes, 0.75f);
        }

        // 기본: 4장 모두 균등 무작위.
        var uniform = new List<(FieldCard, float)>();
        foreach (FieldCard c in hand) uniform.Add((c, 1f / hand.Count));
        return uniform;
    }

    // ── 헬퍼 ────────────────────────────────────────────────
    static bool HasSpecial(IReadOnlyList<FieldCard> field)
    {
        if (field == null) return false;
        foreach (FieldCard c in field) if (c != null && c.Data is SpecialCardData) return true;
        return false;
    }

    static int CountSpecial(IReadOnlyList<FieldCard> field)
    {
        int n = 0;
        if (field == null) return 0;
        foreach (FieldCard c in field) if (c != null && c.Data is SpecialCardData) n++;
        return n;
    }

    static List<FieldCard> FilterSpecial(IReadOnlyList<FieldCard> hand)
    {
        var list = new List<FieldCard>();
        foreach (FieldCard c in hand) if (c != null && c.Data is SpecialCardData) list.Add(c);
        return list;
    }

    static List<FieldCard> FilterNumber(IReadOnlyList<FieldCard> hand, int number)
    {
        var list = new List<FieldCard>();
        foreach (FieldCard c in hand) if (c != null && c.Data != null && c.Data.Number == number) list.Add(c);
        return list;
    }

    static List<FieldCard> FilterEffect(IReadOnlyList<FieldCard> hand, SpecialEffect effect)
    {
        var list = new List<FieldCard>();
        foreach (FieldCard c in hand)
            if (c != null && c.Data is SpecialCardData sp && sp.Effect == effect) list.Add(c);
        return list;
    }

    /// <summary>target 카드들에게 totalWeight를 균등 배분, 나머지 후보 카드들에 (1-totalWeight)를 균등 배분.</summary>
    static List<(FieldCard, float)> SplitWeight(IReadOnlyList<FieldCard> hand, List<FieldCard> target, float totalWeight)
    {
        var result = new List<(FieldCard, float)>();
        float each = totalWeight / target.Count;
        foreach (FieldCard c in target) result.Add((c, each));

        var rest = new List<FieldCard>();
        foreach (FieldCard c in hand) if (!target.Contains(c)) rest.Add(c);

        if (rest.Count > 0)
        {
            float restEach = (1f - totalWeight) / rest.Count;
            foreach (FieldCard c in rest) result.Add((c, restEach));
        }
        return result;
    }

    static FieldCard PickWeighted(List<(FieldCard card, float weight)> weighted)
    {
        float total = 0f;
        foreach (var w in weighted) total += Mathf.Max(0f, w.weight);
        if (total <= 0f) return weighted.Count > 0 ? weighted[0].card : null;

        float r = Random.value * total;
        float acc = 0f;
        foreach (var w in weighted)
        {
            acc += Mathf.Max(0f, w.weight);
            if (r <= acc) return w.card;
        }
        return weighted[weighted.Count - 1].card;
    }
}
