using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어가 보유한 카드 목록(런타임). 중복은 허용하지 않는다.
/// 카드가 추가/제거되면 이벤트로 알려 DeckView가 자동 정렬하게 한다.
/// </summary>
public class Deck : MonoBehaviour
{
    [Tooltip("시작 시 보유할 카드")]
    [SerializeField] List<CardData> startingCards = new List<CardData>();

    readonly List<CardData> _cards = new List<CardData>();

    /// <summary>보유 카드(획득 순서). 읽기 전용.</summary>
    public IReadOnlyList<CardData> Cards => _cards;

    /// <summary>덱 구성이 바뀔 때마다 호출.</summary>
    public event Action OnChanged;
    /// <summary>새 카드를 획득했을 때 호출(연출용).</summary>
    public event Action<CardData> OnCardAdded;

    void Awake()
    {
        foreach (CardData c in startingCards)
            if (c != null && !_cards.Contains(c)) _cards.Add(c);
    }

    /// <summary>카드 획득. 이미 보유 중이거나 null이면 무시. 실제로 추가되면 true.</summary>
    public bool AddCard(CardData card)
    {
        if (card == null || _cards.Contains(card)) return false;
        _cards.Add(card);
        OnCardAdded?.Invoke(card);
        OnChanged?.Invoke();
        return true;
    }

    public bool RemoveCard(CardData card)
    {
        if (!_cards.Remove(card)) return false;
        OnChanged?.Invoke();
        return true;
    }

    public bool Contains(CardData card) => card != null && _cards.Contains(card);
}
