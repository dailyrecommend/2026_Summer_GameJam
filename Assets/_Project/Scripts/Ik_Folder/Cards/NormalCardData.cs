using UnityEngine;

/// <summary>일반 카드. 덱에서 좌측에 정렬된다.</summary>
[CreateAssetMenu(menuName = "CardBoardWar/Cards/Normal Card", fileName = "NormalCard")]
public class NormalCardData : CardData
{
    public override CardCategory Category => CardCategory.Normal;
}
