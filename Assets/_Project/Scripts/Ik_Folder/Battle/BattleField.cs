using System.Collections.Generic;
using UnityEngine;
using TweenKit;

/// <summary>
/// 한쪽(플레이어 또는 적)의 필드. 카드를 중앙 정렬로 배치하고, 드로우 더미에서 뽑혀 나오는 딜 연출을 한다.
/// 승부에 올린 카드는 지정 슬롯으로 이동시킨다. 라운드 진행은 BattleManager가 제어한다.
/// </summary>
public class BattleField : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] FieldCard cardPrefab;
    [Tooltip("생성된 카드가 놓일 부모. 이 로컬 원점이 필드 중앙이 된다.")]
    [SerializeField] Transform cardParent;
    [Tooltip("여기서 카드를 뽑아 배치(DealFromDeck 테스트용)")]
    [SerializeField] Deck deck;

    [Header("레이아웃")]
    [SerializeField] int maxCards = 4;
    [SerializeField] float spacing = 1.2f;
    [Tooltip("0보다 크면 이 폭에 맞춰 간격을 줄여 겹치게 함")]
    [SerializeField] float maxWidth = 0f;
    [SerializeField] float depthStep = 0.01f;

    [Header("정렬 연출")]
    [SerializeField] float arrangeDuration = 0.3f;
    [SerializeField] Ease arrangeEase = Ease.OutCubic;

    [Header("딜 연출 (드로우 더미에서 한 장씩)")]
    [Tooltip("카드가 뽑혀 나오는 시작 위치(뽑을 카드 더미)")]
    [SerializeField] Transform drawPile;
    [SerializeField] float dealStagger = 0.09f;
    [SerializeField] float dealDuration = 0.25f;
    [SerializeField] Ease dealEase = Ease.OutCubic;

    [Header("승부")]
    [Tooltip("승부에 올린 카드가 이동할 위치")]
    [SerializeField] Transform showdownSlot;
    [SerializeField] float commitDuration = 0.3f;
    [SerializeField] Ease commitEase = Ease.OutCubic;

    [Header("버림 / 리셔플")]
    [Tooltip("버린 카드가 쌓이는 위치")]
    [SerializeField] Transform discardPile;
    [SerializeField] float discardDuration = 0.3f;
    [SerializeField] Ease discardEase = Ease.OutCubic;
    [Tooltip("리셔플 연출: 버린 더미 → 뽑을 더미로 넘어가는 한 장 시간/간격")]
    [SerializeField] float reshuffleDuration = 0.22f;
    [SerializeField] float reshuffleStagger = 0.06f;
    [SerializeField] Ease reshuffleEase = Ease.OutCubic;
    [Tooltip("리셔플 때 실제로 날리는 카드 연출 최대 장수")]
    [SerializeField] int reshuffleVisualMax = 6;

    readonly List<FieldCard> _cards = new List<FieldCard>();
    Vector3 _baseLocalPos;

    public IReadOnlyList<FieldCard> Cards => _cards;

    void Awake()
    {
        if (cardPrefab != null) _baseLocalPos = cardPrefab.transform.localPosition;
    }

    // ── 배치 ──────────────────────────────────────────────────

    /// <summary>덱에서 뽑아 배치(테스트용).</summary>
    public void DealFromDeck()
    {
        if (deck == null || cardPrefab == null || deck.Cards.Count == 0) return;
        Deal(deck.Cards);
    }

    /// <summary>기존 카드를 지우고 새로 배치.</summary>
    public void Deal(IEnumerable<CardData> source)
    {
        Clear();
        List<CardData> list = new List<CardData>(source);
        if (list.Count > maxCards) list = list.GetRange(0, maxCards);
        AddCards(list);
    }

    /// <summary>카드들을 필드에 추가(드로우 더미에서 뒷면으로 나와 도착 시 앞면). 기존 카드는 유지·재정렬.</summary>
    public void AddCards(IEnumerable<CardData> newData)
    {
        if (cardPrefab == null) return;
        Transform parent = cardParent != null ? cardParent : transform;
        Vector3 pileLocal = drawPile != null ? parent.InverseTransformPoint(drawPile.position) : Vector3.zero;

        List<FieldCard> added = new List<FieldCard>();
        foreach (CardData d in newData)
        {
            if (d == null) continue;
            FieldCard card = Instantiate(cardPrefab, parent);
            card.Bind(d);
            card.SnapTo(pileLocal);
            card.SetFaceDown();
            _cards.Add(card);
            added.Add(card);
        }
        Reposition(added);
    }

    /// <summary>현재 카드들을 중앙 정렬로 재배치(즉시).</summary>
    public void Arrange() => Reposition(null);

    // dealNew에 든 카드는 더미에서 휙 날아와 도착 시 앞면으로, 나머지는 즉시 정렬.
    void Reposition(List<FieldCard> dealNew)
    {
        _cards.Sort((a, b) => a.Data.Number.CompareTo(b.Data.Number)); // 좌:낮음, 우:높음

        int n = _cards.Count;
        if (n == 0) return;

        float step = spacing;
        if (maxWidth > 0f && n > 1)
        {
            float needed = (n - 1) * spacing;
            if (needed > maxWidth) step = maxWidth / (n - 1);
        }
        float startX = -(n - 1) * step * 0.5f;

        int dealIndex = 0;
        for (int i = 0; i < n; i++)
        {
            FieldCard card = _cards[i];
            card.transform.SetSiblingIndex(i);
            Vector3 target = new Vector3(
                startX + i * step,
                _baseLocalPos.y,
                _baseLocalPos.z - i * depthStep);

            if (dealNew != null && dealNew.Contains(card))
            {
                float delay = dealIndex * dealStagger;
                dealIndex++;
                FieldCard c = card;
                card.PlaceAt(target, dealDuration, dealEase, delay, () => c.FlipUp());
            }
            else
            {
                card.PlaceAt(target, arrangeDuration, arrangeEase);
            }
        }
    }

    // ── 승부/제거 ─────────────────────────────────────────────

    /// <summary>카드를 승부 슬롯으로 이동시키고 필드 목록에서 제거(오브젝트는 유지).</summary>
    public void CommitCard(FieldCard card)
    {
        if (card == null || !_cards.Remove(card))
        {
            Debug.LogWarning($"[BattleField] CommitCard 무시: 카드가 이 필드에 없음 ({name})");
            return;
        }

        if (showdownSlot == null)
        {
            Debug.LogWarning($"[BattleField] Showdown Slot이 연결되지 않았습니다 ({name})");
        }
        else
        {
            Transform parent = cardParent != null ? cardParent : transform;
            Vector3 local = parent.InverseTransformPoint(showdownSlot.position);
            card.PlaceAt(local, commitDuration, commitEase);
        }
        Arrange();
    }

    public bool Contains(FieldCard card) => _cards.Contains(card);

    public void Remove(FieldCard card)
    {
        if (_cards.Remove(card)) Arrange();
    }

    // ── 버림 / 리셔플 ─────────────────────────────────────────

    /// <summary>한 장을 버린 더미로 보낸다(연출 후 파괴). pile에 데이터 추가.</summary>
    public void DiscardCard(FieldCard card, CardPile pile)
    {
        if (card == null) return;
        _cards.Remove(card); // 필드에 남아 있으면 제거
        if (pile != null && card.Data != null) pile.Discard(card.Data);
        MoveToDiscardAndDestroy(card);
    }

    /// <summary>필드에 남은 모든 카드를 버린 더미로 보낸다.</summary>
    public void DiscardAll(CardPile pile)
    {
        List<FieldCard> snapshot = new List<FieldCard>(_cards);
        _cards.Clear();
        foreach (FieldCard c in snapshot)
        {
            if (pile != null && c != null && c.Data != null) pile.Discard(c.Data);
            MoveToDiscardAndDestroy(c);
        }
    }

    void MoveToDiscardAndDestroy(FieldCard card)
    {
        if (card == null) return;

        card.SetFaceDown(); // 버린 더미로 갈 땐 뒷면

        if (discardPile != null)
        {
            Transform parent = cardParent != null ? cardParent : transform;
            Vector3 local = parent.InverseTransformPoint(discardPile.position);
            FieldCard c = card;
            card.PlaceAt(local, discardDuration, discardEase, 0f, () => { if (c != null) Destroy(c.gameObject); });
        }
        else
        {
            Destroy(card.gameObject);
        }
    }

    /// <summary>버린 더미 → 뽑을 더미로 카드가 넘어가는 리셔플 연출(순수 시각). 뒷면 + 텍스쳐.</summary>
    public void PlayReshuffle(IReadOnlyList<CardData> cards)
    {
        if (cardPrefab == null || drawPile == null || discardPile == null || cards == null || cards.Count == 0) return;

        Transform parent = cardParent != null ? cardParent : transform;
        Vector3 fromLocal = parent.InverseTransformPoint(discardPile.position);
        Vector3 toLocal = parent.InverseTransformPoint(drawPile.position);

        int n = Mathf.Min(cards.Count, reshuffleVisualMax);
        for (int i = 0; i < n; i++)
        {
            FieldCard ghost = Instantiate(cardPrefab, parent);
            Collider col = ghost.GetComponent<Collider>();
            if (col != null) col.enabled = false; // 연출용이라 클릭 안 받게
            ghost.Bind(cards[i]);   // 텍스쳐 입히기
            ghost.SetFaceDown();    // 뒷면
            ghost.SnapTo(fromLocal);

            float delay = i * reshuffleStagger;
            FieldCard g = ghost;
            ghost.PlaceAt(toLocal, reshuffleDuration, reshuffleEase, delay, () => { if (g != null) Destroy(g.gameObject); });
        }
    }

    /// <summary>리셔플 연출 총 소요 시간(대기용).</summary>
    public float ReshuffleTime(int count)
    {
        int n = Mathf.Min(Mathf.Max(count, 0), reshuffleVisualMax);
        if (n <= 0) return 0f;
        return (n - 1) * reshuffleStagger + reshuffleDuration;
    }

    public void Clear()
    {
        foreach (FieldCard c in _cards)
            if (c != null) Destroy(c.gameObject);
        _cards.Clear();
    }
}
