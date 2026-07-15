using UnityEngine;

/// <summary>
/// 뽑을/버린 카드 더미의 '쌓인 높이'를 카드 수에 맞춰 Z 스케일로 표현한다.
/// CardPile을 직접 폴링하지 않고, 해당 카드의 이동 연출이 '끝난 시점'에 BattleField가
/// SetCount를 명시적으로 호출해줘야 갱신된다(즉 연출과 정확히 맞물려서 바뀜).
/// </summary>
public class PileStackVisual : MonoBehaviour
{
    [Tooltip("Z 스케일을 조절할 대상(비우면 이 오브젝트 자신)")]
    [SerializeField] Transform stackVisual;

    [Tooltip("0장일 때 Z 스케일 배율(0=완전히 납작해짐)")]
    [SerializeField] float minScale = 0f;
    [Tooltip("총 장수를 다 채웠을 때 Z 스케일 배율(보통 1)")]
    [SerializeField] float maxScale = 1f;
    [Tooltip("목표 높이로 부드럽게 수렴하는 속도")]
    [SerializeField] float smooth = 10f;

    [Header("텍스쳐 (선택 — 스테이지마다 더미 생김새가 다를 때)")]
    [Tooltip("텍스쳐를 입힐 렌더러(비우면 stackVisual에서 자동으로 찾음)")]
    [SerializeField] Renderer pileRenderer;
    [SerializeField] string texturePropertyName = "_BaseMap";

    int _total = 1;
    int _count;
    Vector3 _baseScale = Vector3.one;
    MaterialPropertyBlock _mpb;

    bool _baseScaleCaptured; // Awake 또는 Bind(선점) 중 먼저 실행된 쪽에서만 baseScale을 잡는다.

    void Awake()
    {
        if (stackVisual == null) stackVisual = transform;
        if (!_baseScaleCaptured)
        {
            _baseScale = stackVisual.localScale;
            _baseScaleCaptured = true;
        }
        Debug.Log($"[PileStackVisual:{name}] Awake baseScale={_baseScale} activeInHierarchy={gameObject.activeInHierarchy}");
        if (pileRenderer == null) pileRenderer = stackVisual.GetComponentInChildren<Renderer>();
    }

    /// <summary>더미 모형의 텍스쳐 교체(스테이지마다 카드 더미 생김새가 다를 때).</summary>
    public void SetTexture(Texture tex)
    {
        if (pileRenderer == null || tex == null) return;
        if (_mpb == null) _mpb = new MaterialPropertyBlock();
        pileRenderer.GetPropertyBlock(_mpb);
        _mpb.SetTexture(texturePropertyName, tex);
        pileRenderer.SetPropertyBlock(_mpb);
    }

    /// <summary>
    /// 기준 총 장수 지정(보통 그 쪽 덱 전체 장수)하고 초기 표시 개수로 '즉시' 스냅한다(연출 없음).
    /// 오브젝트(또는 부모)가 비활성 상태면 Update가 안 돌아서 lerp가 반영되지 않으므로,
    /// 전투 시작 시점의 초기화는 반드시 이렇게 즉시 적용해야 한다.
    /// </summary>
    public void Bind(int total, int initialCount)
    {
        if (!_baseScaleCaptured)
        {
            // Awake가 아직 안 돌았다(오브젝트/부모가 비활성 상태였을 가능성) → 지금 강제로 캡처.
            // (나중에 Awake가 뒤늦게 실행돼도 이미 캡처된 값을 덮어쓰지 않는다)
            Debug.LogWarning($"[PileStackVisual:{name}] Bind가 Awake보다 먼저 호출됨! baseScale을 지금 강제로 캡처합니다.");
            if (stackVisual == null) stackVisual = transform;
            _baseScale = stackVisual.localScale;
            _baseScaleCaptured = true;
        }

        _total = Mathf.Max(1, total);
        _count = Mathf.Clamp(initialCount, 0, _total);
        Debug.Log($"[PileStackVisual:{name}] Bind total={_total} initialCount={_count} baseScale={_baseScale} stackVisual={(stackVisual != null ? stackVisual.name : "null")}");
        SnapToCurrentCount();
    }

    /// <summary>지금 이 더미가 보여줘야 할 장수. 호출 시점에 맞춰 목표 높이가 갱신된다(Update에서 부드럽게 수렴).</summary>
    public void SetCount(int count)
    {
        int clamped = Mathf.Clamp(count, 0, _total);
        Debug.Log($"[PileStackVisual:{name}] SetCount {count} (clamped {clamped}) / total {_total}");
        _count = clamped;
    }

    // 현재 _count에 해당하는 스케일을 lerp 없이 즉시 적용.
    void SnapToCurrentCount()
    {
        if (stackVisual == null) return;
        float frac = (float)_count / _total;
        Vector3 s = stackVisual.localScale;
        s.z = Mathf.Lerp(minScale, maxScale, frac) * _baseScale.z;
        stackVisual.localScale = s;
    }

    void Update()
    {
        if (stackVisual == null) return;

        float frac = (float)_count / _total;
        float targetZ = Mathf.Lerp(minScale, maxScale, frac) * _baseScale.z;

        Vector3 s = stackVisual.localScale;
        float t = 1f - Mathf.Exp(-smooth * Time.deltaTime);
        s.z = Mathf.Lerp(s.z, targetZ, t);
        stackVisual.localScale = s;
    }
}
