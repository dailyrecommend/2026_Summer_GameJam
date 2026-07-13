using UnityEngine;

/// <summary>
/// 특수 카드. 덱에서 우측에 정렬된다.
/// 카드마다 발동 로직이 다르면 이 클래스를 상속해 <see cref="Activate"/> 를 override 하면 된다.
/// (전투가 아직 없으므로 지금은 훅만 존재)
/// </summary>
[CreateAssetMenu(menuName = "CardBoardWar/Cards/Special Card", fileName = "SpecialCard")]
public class SpecialCardData : CardData
{
    public override CardCategory Category => CardCategory.Special;

    /// <summary>특수카드 발동. 전투 구현 시 ctx로 대상/상태를 받아 처리한다.</summary>
    public virtual void Activate(CardUseContext ctx)
    {
        Debug.Log($"[SpecialCard] '{DisplayName}' 발동 — 로직 미구현");
    }
}
