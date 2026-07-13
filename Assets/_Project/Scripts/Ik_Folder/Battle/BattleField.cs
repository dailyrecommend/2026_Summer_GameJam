using System.Collections.Generic;
using UnityEngine;
using TweenKit;

/// <summary>
/// 플레이어의 필드. 카드들을 중앙 정렬로 동적 배치하고, 추가/제거 시 자동으로 재정렬한다.
/// (DeckView와 같은 방식) 배치 규칙: 숫자 내림차순으로 '오른쪽이 가장 높게'.
/// </summary>
public class BattleField : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] FieldCard cardPrefab;
    [Tooltip("생성된 카드가 놓일 부모. 이 로컬 원점이 필드 중앙이 된다.")]
    [SerializeField] Transform cardParent;
    [Tooltip("여기서 카드를 뽑아 배치(DealFromDeck용). 전투 진입 시 연결")]
    [SerializeField] Deck deck;

    [Header("레이아웃")]
    [Tooltip("한 라운드에 놓을 최대 카드 수")]
    [SerializeField] int maxCards = 4;
    [Tooltip("카드 사이 간격(로컬 X)")]
    [SerializeField] float spacing = 1.2f;
    [Tooltip("0보다 크면 이 폭에 맞춰 간격을 줄여 겹치게 함")]
    [SerializeField] float maxWidth = 0f;
    [Tooltip("카드마다 살짝 앞으로 겹치게 하는 Z 간격")]
    [SerializeField] float depthStep = 0.01f;

    [Header("정렬 연출")]
    [SerializeField] float arrangeDuration = 0.3f;
    [SerializeField] Ease arrangeEase = Ease.OutCubic;

    [Header("딜 연출 (드로우 더미에서 한 장씩)")]
    [Tooltip("카드가 뽑혀 나오는 시작 위치(뽑을 카드 더미)")]
    [SerializeField] Transform drawPile;
    [Tooltip("카드 사이 나오는 간격(초). 작을수록 휙휙 빠르게")]
    [SerializeField] float dealStagger = 0.09f;
    [Tooltip("한 장이 날아가는 시간")]
    [SerializeField] float dealDuration = 0.25f;
    [SerializeField] Ease dealEase = Ease.OutCubic;

    [Header("승부")]
    [Tooltip("클릭·재클릭으로 승부에 올린 카드가 이동할 위치")]
    [SerializeField] Transform showdownSlot;
    [Tooltip("승부 클릭을 받는 인터랙터")]
    [SerializeField] BattleFieldInteractor interactor;
    [SerializeField] float commitDuration = 0.3f;
    [SerializeField] Ease commitEase = Ease.OutCubic;

    readonly List<FieldCard> _cards = new List<FieldCard>();
    Vector3 _baseLocalPos;

    public IReadOnlyList<FieldCard> Cards => _cards;

    void Awake()
    {
        if (cardPrefab != null) _baseLocalPos = cardPrefab.transform.localPosition;
    }

    void OnEnable()
    {
        if (interactor != null) interactor.CardCommitted += OnCommitted;
    }

    void OnDisable()
    {
        if (interactor != null) interactor.CardCommitted -= OnCommitted;
    }

    // 승부에 올린 카드 → 지정 위치로 이동 후 필드에서 제거.
    void OnCommitted(FieldCard card)
    {
        if (card == null || !_cards.Remove(card)) return; // 이 필드의 카드만 처리

        if (showdownSlot != null)
        {
            Transform parent = cardParent != null ? cardParent : transform;
            Vector3 local = parent.InverseTransformPoint(showdownSlot.position);
            card.PlaceAt(local, commitDuration, commitEase);
        }
        Arrange();
    }

    /// <summary>연결된 덱에서 카드를 뽑아 배치. (UnityEvent 연결용, 파라미터 없음)</summary>
    public void DealFromDeck()
    {
        if (deck == null) { Debug.LogWarning("[BattleField] Deck이 연결되지 않았습니다."); return; }
        if (cardPrefab == null) { Debug.LogWarning("[BattleField] Card Prefab이 없습니다."); return; }
        if (deck.Cards.Count == 0) { Debug.LogWarning("[BattleField] 덱이 비어 있습니다(Starting Cards 확인)."); return; }

        Deal(deck.Cards);
        Debug.Log($"[BattleField] 배치 {_cards.Count}장 (덱 {deck.Cards.Count}장 중 최대 {maxCards})");
    }

    /// <summary>주어진 카드들을 필드에 배치. (숫자 오름차순으로 좌→우 = 오른쪽이 가장 높음)</summary>
    public void Deal(IEnumerable<CardData> source)
    {
        Clear();
        if (cardPrefab == null) return;

        List<CardData> list = new List<CardData>(source);
        list.Sort((a, b) => a.Number.CompareTo(b.Number)); // 오름차순 → 우측이 최고 숫자

        int n = Mathf.Min(list.Count, maxCards);
        Transform parent = cardParent != null ? cardParent : transform;

        // 시작은 뽑을 카드 더미 위에 모두 겹쳐둔다.
        Vector3 pileLocal = drawPile != null ? parent.InverseTransformPoint(drawPile.position) : Vector3.zero;

        for (int i = 0; i < n; i++)
        {
            FieldCard card = Instantiate(cardPrefab, parent);
            card.Bind(list[i]);
            card.SnapTo(pileLocal);
            card.SetFaceDown();   // 뒷면으로 나옴
            _cards.Add(card);
        }
        ArrangeInternal(true); // 더미에서 한 장씩 휙휙 (도착 시 앞면)
    }

    /// <summary>현재 카드들을 중앙 정렬로 재배치(즉시, 재정렬용).</summary>
    public void Arrange() => ArrangeInternal(false);

    void ArrangeInternal(bool dealt)
    {
        int n = _cards.Count;
        if (n == 0) return;

        float step = spacing;
        if (maxWidth > 0f && n > 1)
        {
            float needed = (n - 1) * spacing;
            if (needed > maxWidth) step = maxWidth / (n - 1);
        }
        float startX = -(n - 1) * step * 0.5f;

        for (int i = 0; i < n; i++)
        {
            FieldCard card = _cards[i];
            card.transform.SetSiblingIndex(i);
            Vector3 target = new Vector3(
                startX + i * step,
                _baseLocalPos.y,
                _baseLocalPos.z - i * depthStep);

            if (dealt)
            {
                // 우측(높은 숫자)부터 먼저 나오도록 딜레이 부여. 도착하면 앞면으로 뒤집기.
                float delay = (n - 1 - i) * dealStagger;
                FieldCard c = card; // 클로저 캡처
                card.PlaceAt(target, dealDuration, dealEase, delay, () => c.FlipUp());
            }
            else
            {
                card.PlaceAt(target, arrangeDuration, arrangeEase);
            }
        }
    }

    /// <summary>필드에서 카드 제거 후 재정렬. (승부에 올렸을 때 등) 카드 오브젝트는 파괴하지 않음.</summary>
    public void Remove(FieldCard card)
    {
        if (_cards.Remove(card)) Arrange();
    }

    public void Clear()
    {
        foreach (FieldCard c in _cards)
            if (c != null) Destroy(c.gameObject);
        _cards.Clear();
    }
}
