using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 스테이지 정의(ScriptableObject). 스테이지마다 적 덱/생김새/보상이 다르다.
/// AI 성향은 지금은 공통(가장 큰 카드)이라 여기엔 없음 — 나중에 성향이 생기면 필드 추가.
/// </summary>
[CreateAssetMenu(menuName = "CardBoardWar/Stage", fileName = "Stage")]
public class StageData : ScriptableObject
{
    [Header("식별")]
    [SerializeField] string id;
    [SerializeField] string displayName;
    [TextArea] [SerializeField] string description;

    [Header("생김새")]
    [Tooltip("캐러셀/전투에 표시할 이 스테이지의 오브젝트(보드게임 등)")]
    [SerializeField] GameObject boardGamePrefab;

    [Header("적")]
    [Tooltip("이 스테이지 적의 덱(뽑을 더미 소스). 비우면 플레이어 덱 복사")]
    [SerializeField] List<CardData> enemyDeck = new List<CardData>();

    [Header("보상")]
    [Tooltip("최초 클리어 시 지급하는 카드")]
    [SerializeField] CardData firstClearReward;

    public string Id => id;
    public string DisplayName => displayName;
    public string Description => description;
    public GameObject BoardGamePrefab => boardGamePrefab;
    public IReadOnlyList<CardData> EnemyDeck => enemyDeck;
    public CardData FirstClearReward => firstClearReward;
}
