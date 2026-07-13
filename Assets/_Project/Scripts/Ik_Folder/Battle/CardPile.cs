using System.Collections.Generic;

/// <summary>
/// 전투 런타임용 카드 더미. 뽑을 더미/버린 더미를 관리하고, 뽑을 더미가 비면 버린 더미를 섞어 되채운다.
/// (수집 Deck을 소모하지 않도록 참조 목록으로 별도 관리)
/// </summary>
public class CardPile
{
    readonly List<CardData> _draw = new List<CardData>();
    readonly List<CardData> _discard = new List<CardData>();

    public int DrawCount => _draw.Count;
    public int DiscardCount => _discard.Count;
    public int Total => _draw.Count + _discard.Count;

    /// <summary>버린 더미의 카드들(리셔플 연출용).</summary>
    public IReadOnlyList<CardData> DiscardCards => _discard;

    /// <summary>주어진 카드들로 뽑을 더미를 초기화하고 섞는다.</summary>
    public void Init(IEnumerable<CardData> cards)
    {
        _draw.Clear();
        _discard.Clear();
        _draw.AddRange(cards);
        Shuffle(_draw);
    }

    /// <summary>한 장 뽑는다. 뽑을 더미가 비면 버린 더미를 섞어 되채운다. 완전히 없으면 null.</summary>
    public CardData Draw()
    {
        if (_draw.Count == 0) Reshuffle();
        if (_draw.Count == 0) return null;

        int last = _draw.Count - 1;
        CardData c = _draw[last];
        _draw.RemoveAt(last);
        return c;
    }

    public void Discard(CardData card)
    {
        if (card != null) _discard.Add(card);
    }

    void Reshuffle()
    {
        if (_discard.Count == 0) return;
        _draw.AddRange(_discard);
        _discard.Clear();
        Shuffle(_draw);
    }

    static void Shuffle(List<CardData> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
