using UnityEngine;
using TweenKit;

/// <summary>
/// 카드 프리팹에 붙이는 뷰. CardData의 텍스쳐를 3D 카드 면(Renderer)에 입히고,
/// 정렬(홈) 위치/스케일 + 마우스 호버 확대를 함께 관리한다.
/// 호버/클릭 감지는 CardInteractor가 담당하며, 이 컴포넌트는 상태 반영만 한다.
/// (카드 프리팹에는 Collider가 있어야 CardInteractor가 인식한다.)
/// </summary>
[DisallowMultipleComponent]
public class CardView : MonoBehaviour
{
    [Header("텍스쳐")]
    [Tooltip("카드 면 텍스쳐를 입힐 렌더러")]
    [SerializeField] Renderer targetRenderer;
    [Tooltip("텍스쳐 프로퍼티 이름. 빌트인 Unlit/Texture=_MainTex, URP=_BaseMap")]
    [SerializeField] string textureProperty = "_MainTex";

    [Header("호버")]
    [Tooltip("마우스를 올렸을 때 확대 배율")]
    [SerializeField] float hoverScale = 1.15f;
    [Tooltip("호버 시 위치 오프셋(로컬). 앞으로 띄우고 싶으면 조정")]
    [SerializeField] Vector3 hoverOffset = Vector3.zero;
    [SerializeField] float hoverDuration = 0.15f;
    [SerializeField] Ease hoverEase = Ease.OutQuad;

    [Header("플립 (우클릭 뒤집기)")]
    [Tooltip("뒤집을 회전 축(로컬). 카드를 좌우로 넘기면 Y, 위아래로 넘기면 X")]
    [SerializeField] Vector3 flipAxis = Vector3.up;
    [SerializeField] float flipDuration = 0.3f;
    [SerializeField] Ease flipEase = Ease.OutQuad;

    [Header("호버 기울기 (커서 쪽으로 기움)")]
    [Tooltip("최대 기울기 각도(도)")]
    [SerializeField] float tiltMaxAngle = 12f;
    [Tooltip("기울기 따라오는 부드러움")]
    [SerializeField] float tiltSmooth = 14f;

    CardData _data;
    MaterialPropertyBlock _mpb;
    Tween _moveTween;
    Tween _scaleTween;
    Tween _flipTween;

    Vector3 _homePos;
    Vector3 _homeScale = Vector3.one;
    Quaternion _baseRot;
    bool _hovered;
    bool _flipped;
    float _flipAngle;

    void Awake()
    {
        _baseRot = transform.localRotation;
    }

    public CardData Data => _data;
    public bool IsHovered => _hovered;

    /// <summary>카드 데이터를 뷰에 반영(텍스쳐 적용).</summary>
    public void Bind(CardData data)
    {
        _data = data;
        if (targetRenderer != null && data != null && data.Texture != null)
        {
            if (_mpb == null) _mpb = new MaterialPropertyBlock();
            targetRenderer.GetPropertyBlock(_mpb);
            _mpb.SetTexture(textureProperty, data.Texture);
            targetRenderer.SetPropertyBlock(_mpb);
        }
    }

    /// <summary>DeckView가 호출: 정렬된 홈 위치/스케일 지정 후 이동(호버 상태 반영).</summary>
    public void SetHome(Vector3 localPos, Vector3 localScale, float duration, Ease ease)
    {
        _homePos = localPos;
        _homeScale = localScale;
        ApplyMove(duration, ease);
        ApplyScale(duration, Ease.OutBack);
    }

    /// <summary>호버 상태 반영(확대/축소).</summary>
    public void SetHovered(bool value)
    {
        if (_hovered == value) return;
        _hovered = value;
        ApplyMove(hoverDuration, hoverEase);
        ApplyScale(hoverDuration, hoverEase);
    }

    void ApplyMove(float duration, Ease ease)
    {
        Vector3 target = _homePos + (_hovered ? hoverOffset : Vector3.zero);
        _moveTween?.Kill();
        _moveTween = transform.DOLocalMove(target, duration).SetEase(ease);
    }

    void ApplyScale(float duration, Ease ease)
    {
        Vector3 target = _homeScale * (_hovered ? hoverScale : 1f);
        _scaleTween?.Kill();
        _scaleTween = transform.DOScale(target, duration).SetEase(ease);
    }

    /// <summary>카드를 180도 뒤집기(토글). 다시 호출하면 원래대로 돌아온다.</summary>
    public void Flip()
    {
        _flipped = !_flipped;
        float to = _flipped ? 180f : 0f;

        _flipTween?.Kill();
        _flipTween = Tw.To(() => _flipAngle, v =>
        {
            _flipAngle = v;
            transform.localRotation = _baseRot * Quaternion.AngleAxis(v, flipAxis);
        }, to, flipDuration).SetEase(flipEase);
    }

    public bool IsFlipped => _flipped;
}
