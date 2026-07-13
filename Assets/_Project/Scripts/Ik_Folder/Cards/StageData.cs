using UnityEngine;

/// <summary>
/// 스테이지 정의(ScriptableObject). 최초 클리어 시 지급할 카드를 담는다.
/// 실제 지급은 전투/클리어 로직 구현 시 Deck.AddCard(stage.FirstClearReward)로 연결한다.
/// </summary>
[CreateAssetMenu(menuName = "CardBoardWar/Stage", fileName = "Stage")]
public class StageData : ScriptableObject
{
    [SerializeField] string id;
    [SerializeField] string displayName;

    [Tooltip("이 스테이지를 최초로 클리어했을 때 지급하는 카드")]
    [SerializeField] CardData firstClearReward;

    public string Id => id;
    public string DisplayName => displayName;
    public CardData FirstClearReward => firstClearReward;
}
