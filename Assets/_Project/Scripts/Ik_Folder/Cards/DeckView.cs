using System.Collections.Generic;
using UnityEngine;
using TweenKit;

/// <summary>
/// 덱을 화면 하단에 가로로 보여준다. 일반 카드는 좌측, 특수 카드는 우측으로 정렬하고,
/// 카드가 추가/제거되면 자동으로 재배치(중앙 정렬)한다.
/// 카드 뷰는 기존 카드 프리팹(CardView 포함)을 생성해 사용.
/// </summary>
[DisallowMultipleComponent]
public class DeckView : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] Deck deck;
    [Tooltip("카드 뷰들이 놓일 부모. 이 로컬 원점이 덱의 중앙이 된다.")]
    [SerializeField] Transform container;
    [Tooltip("생성할 카드 프리팹 (CardView 포함)")]
    [SerializeField] CardView cardPrefab;

    [Header("레이아웃")]
    [Tooltip("카드 사이 간격(로컬 X)")]
    [SerializeField] float spacing = 1.2f;
    [Tooltip("0보다 크면 이 폭에 맞춰 간격을 줄여 겹치게 함(카드 많을 때)")]
    [SerializeField] float maxWidth = 0f;
    [Tooltip("카드마다 살짝 앞으로 겹치게 하는 Z 간격(겹침 정렬용)")]
    [SerializeField] float depthStep = 0.01f;

    [Header("정렬 연출")]
    [SerializeField] float arrangeDuration = 0.35f;
    [SerializeField] Ease arrangeEase = Ease.OutCubic;

    readonly Dictionary<CardData, CardView> _views = new Dictionary<CardData, CardView>();
    readonly List<CardData> _sorted = new List<CardData>();
    readonly List<CardData> _toRemove = new List<CardData>();

    Vector3 _baseScale = Vector3.one;
    Vector3 _baseLocalPos;

    void Awake()
    {
        if (cardPrefab != null)
        {
            _baseScale = cardPrefab.transform.localScale;
            _baseLocalPos = cardPrefab.transform.localPosition;
        }
    }

    void OnEnable()
    {
        if (deck != null) deck.OnChanged += Rebuild;
    }

    void OnDisable()
    {
        if (deck != null) deck.OnChanged -= Rebuild;
    }

    void Start() => Rebuild();

    /// <summary>덱 상태에 맞춰 카드 뷰를 생성/제거하고 재배치한다.</summary>
    public void Rebuild()
    {
        if (deck == null || container == null || cardPrefab == null) return;

        // 1) 정렬: 일반(좌) → 특수(우). 각 그룹 내부는 획득 순서 유지.
        _sorted.Clear();
        foreach (CardData c in deck.Cards)
            if (c != null && c.Category == CardCategory.Normal) _sorted.Add(c);
        foreach (CardData c in deck.Cards)
            if (c != null && c.Category == CardCategory.Special) _sorted.Add(c);

        // 2) 덱에서 빠진 카드의 뷰 제거
        _toRemove.Clear();
        foreach (var kv in _views)
            if (!deck.Contains(kv.Key)) _toRemove.Add(kv.Key);
        foreach (CardData key in _toRemove)
        {
            CardView v = _views[key];
            _views.Remove(key);
            if (v != null) Destroy(v.gameObject);
        }

        // 3) 새 카드 뷰 생성 (팝인 위해 스케일 0에서 시작)
        foreach (CardData c in _sorted)
        {
            if (!_views.ContainsKey(c))
            {
                CardView v = Instantiate(cardPrefab, container);
                v.Bind(c);
                v.transform.localScale = Vector3.zero;
                _views[c] = v;
            }
        }

        // 4) 중앙 정렬 배치 + 애니메이션
        int n = _sorted.Count;
        float step = spacing;
        if (maxWidth > 0f && n > 1)
        {
            float needed = (n - 1) * spacing;
            if (needed > maxWidth) step = maxWidth / (n - 1); // 폭 초과 시 겹치도록 축소
        }
        float startX = -(n - 1) * step * 0.5f;

        for (int i = 0; i < n; i++)
        {
            CardView v = _views[_sorted[i]];
            v.transform.SetSiblingIndex(i);
            Vector3 target = new Vector3(
                startX + i * step,
                _baseLocalPos.y,
                _baseLocalPos.z - i * depthStep);
            v.AnimateTo(target, _baseScale, arrangeDuration, arrangeEase);
        }
    }
}
