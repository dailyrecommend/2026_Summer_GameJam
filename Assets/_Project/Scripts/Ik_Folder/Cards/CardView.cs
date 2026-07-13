using UnityEngine;
using TweenKit;

/// <summary>
/// 카드 프리팹에 붙이는 뷰. CardData의 텍스쳐를 3D 카드 면(Renderer)에 입힌다.
/// 정렬 시 위치/스케일 트윈도 여기서 관리한다.
/// </summary>
[DisallowMultipleComponent]
public class CardView : MonoBehaviour
{
    [Tooltip("카드 면 텍스쳐를 입힐 렌더러")]
    [SerializeField] Renderer targetRenderer;

    [Tooltip("텍스쳐 프로퍼티 이름. URP=_BaseMap, 빌트인=_MainTex")]
    [SerializeField] string textureProperty = "_BaseMap";

    CardData _data;
    MaterialPropertyBlock _mpb;
    Tween _moveTween;
    Tween _scaleTween;

    public CardData Data => _data;

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

    /// <summary>지정 위치/스케일로 부드럽게 이동. 이전 트윈은 취소해 겹침 방지.</summary>
    public void AnimateTo(Vector3 localPos, Vector3 localScale, float duration, Ease ease)
    {
        _moveTween?.Kill();
        _scaleTween?.Kill();
        _moveTween = transform.DOLocalMove(localPos, duration).SetEase(ease);
        _scaleTween = transform.DOScale(localScale, duration).SetEase(Ease.OutBack);
    }
}
