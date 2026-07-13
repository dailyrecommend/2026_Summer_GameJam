using UnityEngine;
using TweenKit;

/// <summary>
/// 전투 필드 카드. 호버 시 확대, 클릭 시 상승, 재클릭 시 승부(지정 위치로 이동)를 위한 상태를 관리한다.
/// 호버/클릭 감지는 BattleFieldInteractor가 담당한다. (프리팹에 Collider 필요)
/// </summary>
[DisallowMultipleComponent]
public class FieldCard : MonoBehaviour
{
    [Header("텍스쳐")]
    [SerializeField] Renderer targetRenderer;
    [SerializeField] string textureProperty = "_MainTex";

    [Header("호버 (확대)")]
    [SerializeField] float hoverScale = 1.15f;
    [SerializeField] float hoverDuration = 0.15f;
    [SerializeField] Ease hoverEase = Ease.OutQuad;

    [Header("상승 (클릭)")]
    [Tooltip("클릭 시 상승 오프셋(로컬). 카드 크기의 약 20%에 맞춰 조절")]
    [SerializeField] Vector3 raiseOffset = new Vector3(0f, 0.2f, 0f);
    [SerializeField] float moveDuration = 0.18f;
    [SerializeField] Ease moveEase = Ease.OutQuad;

    [Header("딜 플립 (뒷면으로 나와 도착 시 앞면)")]
    [Tooltip("뒤집는 회전 축(로컬). 카드가 눕혀진 방향에 맞춰 X/Z 중 선택")]
    [SerializeField] Vector3 flipAxis = Vector3.right;
    [SerializeField] float flipDuration = 0.25f;
    [SerializeField] Ease flipEase = Ease.OutQuad;

    CardData _data;
    MaterialPropertyBlock _mpb;
    Tween _moveTween;
    Tween _scaleTween;
    Tween _flipTween;

    Vector3 _homePos;
    Vector3 _baseScale = Vector3.one;
    Quaternion _baseRot;
    float _flipAngle; // 0=앞면, 180=뒷면
    bool _raised;
    bool _hovered;

    public CardData Data => _data;
    public bool IsRaised => _raised;

    void Awake()
    {
        _baseScale = transform.localScale;
        _baseRot = transform.localRotation;
    }

    /// <summary>즉시 뒷면 상태로.</summary>
    public void SetFaceDown()
    {
        _flipTween?.Kill();
        _flipAngle = 180f;
        transform.localRotation = _baseRot * Quaternion.AngleAxis(_flipAngle, flipAxis);
    }

    /// <summary>앞면으로 뒤집기(애니메이션).</summary>
    public void FlipUp()
    {
        _flipTween?.Kill();
        _flipTween = Tw.To(() => _flipAngle, v =>
        {
            _flipAngle = v;
            transform.localRotation = _baseRot * Quaternion.AngleAxis(v, flipAxis);
        }, 0f, flipDuration).SetEase(flipEase);
    }

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

    /// <summary>지정 위치(로컬)로 즉시 스냅. (딜 시작 전 더미 위에 올려둘 때)</summary>
    public void SnapTo(Vector3 localPos)
    {
        _homePos = localPos;
        _raised = false;
        transform.localPosition = localPos;
    }

    /// <summary>배치 위치(로컬)로 이동. 상승 상태는 해제. delay만큼 늦게 출발, 도착 시 onArrived 호출.</summary>
    public void PlaceAt(Vector3 localPos, float duration, Ease ease, float delay = 0f, System.Action onArrived = null)
    {
        _homePos = localPos;
        _raised = false;
        ApplyMove(duration, ease, delay, onArrived);
        ApplyScale(duration, ease);
    }

    public void SetHovered(bool value)
    {
        if (_hovered == value) return;
        _hovered = value;
        ApplyScale(hoverDuration, hoverEase);
    }

    public void SetRaised(bool value)
    {
        if (_raised == value) return;
        _raised = value;
        ApplyMove(moveDuration, moveEase);
    }

    void ApplyMove(float duration, Ease ease, float delay = 0f, System.Action onArrived = null)
    {
        Vector3 target = _homePos + (_raised ? raiseOffset : Vector3.zero);
        _moveTween?.Kill();
        _moveTween = transform.DOLocalMove(target, duration).SetEase(ease).SetDelay(delay);
        if (onArrived != null) _moveTween.OnComplete(onArrived);
    }

    void ApplyScale(float duration, Ease ease)
    {
        Vector3 target = _baseScale * (_hovered ? hoverScale : 1f);
        _scaleTween?.Kill();
        _scaleTween = transform.DOScale(target, duration).SetEase(ease);
    }
}
