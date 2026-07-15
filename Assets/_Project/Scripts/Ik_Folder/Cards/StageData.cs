using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 스테이지 정의(ScriptableObject). 스테이지마다 적 덱/생김새/보상/AI 성향이 다르다.
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
    [Tooltip("이 스테이지 전투에서 카드 뒷면에 입힐 텍스쳐(보드게임마다 카드 생김새가 다를 때)")]
    [SerializeField] Texture cardBackTexture;

    [Header("적")]
    [Tooltip("이 스테이지 적의 덱(뽑을 더미 소스). 비우면 플레이어 덱 복사")]
    [SerializeField] List<CardData> enemyDeck = new List<CardData>();
    [Tooltip("이 스테이지 적의 카드 선택 성향. 개별 로직에 해당 없으면 공통 로직으로 대체")]
    [SerializeField] AIStyle aiStyle = AIStyle.None;
    [Tooltip("이 스테이지 적의 고정 패시브(라운드 흐름에 개입하는 규칙)")]
    [SerializeField] EnemyPassive enemyPassive = EnemyPassive.None;

    [Header("보상")]
    [Tooltip("최초 클리어 시 지급하는 카드")]
    [SerializeField] CardData firstClearReward;

    [Header("오디오")]
    [Tooltip("이 스테이지 전투 시작 시 재생할 브금")]
    [SerializeField] AudioClip stageBgm;

    public string Id => id;
    public string DisplayName => displayName;
    public string Description => description;
    public GameObject BoardGamePrefab => boardGamePrefab;
    public Texture CardBackTexture => cardBackTexture;
    public IReadOnlyList<CardData> EnemyDeck => enemyDeck;
    public AIStyle AiStyle => aiStyle;
    public EnemyPassive EnemyPassive => enemyPassive;
    public CardData FirstClearReward => firstClearReward;
    public AudioClip StageBgm => stageBgm;
}
