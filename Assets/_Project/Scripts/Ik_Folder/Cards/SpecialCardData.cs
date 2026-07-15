using UnityEngine;

/// <summary>
/// 특수 카드. 덱에서 우측에 정렬된다.
/// 서브클래스를 만들지 않고, 인스펙터에서 <see cref="SpecialEffect"/> 를 골라 효과를 지정한다.
/// 실제 로직은 SpecialCardEffects(중앙 스크립트)에 있다.
/// </summary>
[CreateAssetMenu(menuName = "CardBoardWar/Cards/Special Card", fileName = "SpecialCard")]
public class SpecialCardData : CardData
{
    [Header("특수 효과")]
    [Tooltip("이 카드가 가진 효과 (로직은 SpecialCardEffects에 구현)")]
    [SerializeField] SpecialEffect effect = SpecialEffect.None;

    [Header("고유 연출")]
    [Tooltip("승부 시 이 카드만의 고유 VFX. 재생되고 1초 뒤에 실제 능력이 적용됨")]
    [SerializeField] GameObject vfxPrefab;

    public SpecialEffect Effect => effect;
    public GameObject VfxPrefab => vfxPrefab;
    public override CardCategory Category => CardCategory.Special;

    /// <summary>승부 판정 시 효과를 적용(중앙 로직 호출).</summary>
    public void OnShowdown(ShowdownResult result, bool isPlayer)
        => SpecialCardEffects.ApplyOnShowdown(effect, result, isPlayer);
}
