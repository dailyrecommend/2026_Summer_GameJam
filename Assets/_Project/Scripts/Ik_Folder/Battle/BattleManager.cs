using System.Collections.Generic;
using UnityEngine;
using TweenKit;
using TMPro;

/// <summary>
/// 전투 라운드 루프.
/// 드로우(부족 시 리셔플) → 플레이어/AI 선택 → 공개(승부) → 숫자 비교 → 승점 → 먼저 3승 → 게임 종료.
/// 공개된 카드는 각자 버린 더미로 보낸다.
/// </summary>
public class BattleManager : MonoBehaviour
{
    [Header("필드")]
    [SerializeField] BattleField playerField;
    [SerializeField] BattleField enemyField;
    [SerializeField] BattleFieldInteractor playerInteractor;

    [Tooltip("전투 중 화면 이동을 잠그기 위한 참조(선택)")]
    [SerializeField] ScreenFlowController screenFlow;

    [Header("클리어/복귀/보상")]
    [SerializeField] StageProgress progress;
    [Tooltip("전투 종료 후 돌아갈 스테이지 화면 인덱스")]
    [SerializeField] int stageScreenIndex = 1;
    [Tooltip("결과 표시 후 스테이지로 복귀하기까지 대기(초)")]
    [SerializeField] float endReturnDelay = 1.5f;

    [Header("덱 소스")]
    [Tooltip("플레이어가 모은 덱(뽑을 더미 소스)")]
    [SerializeField] Deck playerDeck;
    [Tooltip("적 덱. 비우면 플레이어 덱을 복사해서 사용")]
    [SerializeField] List<CardData> enemyDeck = new List<CardData>();

    [Header("규칙")]
    [SerializeField] int handSize = 4;
    [SerializeField] int winTarget = 3;
    [Tooltip("특수룰: 이 낮은 숫자가 이 높은 숫자를 이긴다 (예: 1이 6을 이김)")]
    [SerializeField] int upsetLow = 1;
    [SerializeField] int upsetHigh = 6;

    [Header("타이밍")]
    [Tooltip("선택(커밋) 후 결과 판정까지 대기")]
    [SerializeField] float revealDelay = 0.6f;
    [Tooltip("결과 후 다음 라운드까지 대기")]
    [SerializeField] float nextRoundDelay = 1.2f;

    [Header("UI (선택)")]
    [SerializeField] TMP_Text playerScoreText;
    [SerializeField] TMP_Text enemyScoreText;
    [SerializeField] TMP_Text messageText;

    [Header("점수판 (선택)")]
    [SerializeField] ScoreBoard playerScoreBoard;
    [SerializeField] ScoreBoard enemyScoreBoard;

    readonly CardPile _playerPile = new CardPile();
    readonly CardPile _enemyPile = new CardPile();

    int _playerScore, _enemyScore;
    bool _waitingPlayer;
    bool _gameOver;
    FieldCard _playerChosen, _enemyChosen;
    StageData _activeStage; // 현재 스테이지(적 덱/보상 소스)
    int _activeStageIndex = -1;

    /// <summary>스테이지를 지정해 전투 시작. (적 덱/보상을 이 스테이지에서 가져옴)</summary>
    public void StartBattle(StageData stage) => StartBattle(stage, -1);

    public void StartBattle(StageData stage, int stageIndex)
    {
        _activeStage = stage;
        _activeStageIndex = stageIndex;
        StartBattle();
    }

    void OnEnable()
    {
        if (playerInteractor != null) playerInteractor.CardCommitted += OnPlayerCommit;
    }

    void OnDisable()
    {
        if (playerInteractor != null) playerInteractor.CardCommitted -= OnPlayerCommit;
    }

    /// <summary>전투 시작. (BattleEntry의 On Battle Entered에 연결)</summary>
    public void StartBattle()
    {
        _gameOver = false;
        _playerScore = 0;
        _enemyScore = 0;
        UpdateScoreUI();
        SetMessage("");

        // 전투 중에는 화면 이동 잠금(게임 종료 시 해제).
        if (screenFlow != null) screenFlow.SetNavigationLocked(true);

        List<CardData> pCards = new List<CardData>();
        if (playerDeck != null) pCards.AddRange(playerDeck.Cards);
        _playerPile.Init(pCards);

        // 적 덱 우선순위: 스테이지 데이터 > 인스펙터 enemyDeck > 플레이어 덱 복사
        List<CardData> eCards;
        if (_activeStage != null && _activeStage.EnemyDeck != null && _activeStage.EnemyDeck.Count > 0)
        {
            eCards = new List<CardData>(_activeStage.EnemyDeck);
            Debug.Log($"[Battle] 적 덱 = 스테이지 '{_activeStage.DisplayName}' ({eCards.Count}장)");
        }
        else if (enemyDeck != null && enemyDeck.Count > 0)
        {
            eCards = new List<CardData>(enemyDeck);
            Debug.Log($"[Battle] 적 덱 = 인스펙터 fallback ({eCards.Count}장)");
        }
        else
        {
            eCards = new List<CardData>(pCards);
            string why = _activeStage == null ? "스테이지 미지정(_activeStage=null)" : "스테이지 EnemyDeck 비어있음";
            Debug.Log($"[Battle] 적 덱 = 플레이어 덱 복사 — 이유: {why}");
        }
        _enemyPile.Init(eCards);

        playerField.Clear();
        enemyField.Clear();

        StartRound();
    }

    void StartRound()
    {
        if (_gameOver) return;

        // 뽑을 더미 부족 → 버린 더미에서 넘어오는 리셔플 연출 먼저.
        float wait = 0f;
        if (NeedReshuffle(playerField, _playerPile))
        {
            playerField.PlayReshuffle(_playerPile.DiscardCards);
            wait = Mathf.Max(wait, playerField.ReshuffleTime(_playerPile.DiscardCount));
        }
        if (NeedReshuffle(enemyField, _enemyPile))
        {
            enemyField.PlayReshuffle(_enemyPile.DiscardCards);
            wait = Mathf.Max(wait, enemyField.ReshuffleTime(_enemyPile.DiscardCount));
        }

        if (wait > 0f) Tw.Delay(wait, DealAndBeginTurn);
        else DealAndBeginTurn();
    }

    void DealAndBeginTurn()
    {
        if (_gameOver) return;

        RefillField(playerField, _playerPile);
        RefillField(enemyField, _enemyPile);

        _playerChosen = null;
        _enemyChosen = null;
        _waitingPlayer = true;
        if (playerInteractor != null) playerInteractor.SetLocked(false);
        SetMessage("카드를 선택하세요");
    }

    bool NeedReshuffle(BattleField field, CardPile pile)
    {
        int need = handSize - field.Cards.Count;
        return pile.DrawCount < need && pile.DiscardCount > 0;
    }

    void RefillField(BattleField field, CardPile pile)
    {
        int need = handSize - field.Cards.Count;
        List<CardData> drawn = new List<CardData>();
        for (int i = 0; i < need; i++)
        {
            CardData c = pile.Draw();
            if (c != null) drawn.Add(c);
        }
        if (drawn.Count > 0) field.AddCards(drawn);
    }

    // 플레이어가 승부에 올림 → 적도 선택 → 공개
    void OnPlayerCommit(FieldCard card)
    {
        if (!_waitingPlayer || _gameOver || card == null) return;
        if (!playerField.Contains(card)) return; // 적 카드 클릭 방지(레이어로도 막을 것)
        _waitingPlayer = false;
        if (playerInteractor != null) playerInteractor.SetLocked(true);

        _playerChosen = card;
        playerField.CommitCard(card);

        _enemyChosen = PickHighestCard(enemyField.Cards);
        if (_enemyChosen != null) enemyField.CommitCard(_enemyChosen);

        SetMessage("승부!");
        Tw.Delay(revealDelay, Resolve);
    }

    // TestAI: 필드 카드 중 숫자가 가장 높은 것을 선택.
    static FieldCard PickHighestCard(IReadOnlyList<FieldCard> cards)
    {
        FieldCard best = null;
        for (int i = 0; i < cards.Count; i++)
        {
            FieldCard c = cards[i];
            if (c == null || c.Data == null) continue;
            if (best == null || c.Data.Number > best.Data.Number) best = c;
        }
        return best;
    }

    void Resolve()
    {
        if (_gameOver) return;

        int p = (_playerChosen != null && _playerChosen.Data != null) ? _playerChosen.Data.Number : -1;
        int e = (_enemyChosen != null && _enemyChosen.Data != null) ? _enemyChosen.Data.Number : -1;

        // 특수 카드 효과 적용(조커 = 무승부 강제 등).
        ShowdownResult sr = new ShowdownResult { PlayerNumber = p, EnemyNumber = e };
        if (_playerChosen != null && _playerChosen.Data is SpecialCardData ps) ps.OnShowdown(sr, true);
        if (_enemyChosen != null && _enemyChosen.Data is SpecialCardData es) es.OnShowdown(sr, false);

        int cmp = sr.ForceDraw ? 0 : CompareCards(sr.PlayerNumber, sr.EnemyNumber);
        string result;
        if (cmp > 0) { _playerScore++; result = "플레이어 승"; SetMessage("승리!"); }
        else if (cmp < 0) { _enemyScore++; result = "적 승"; SetMessage("패배..."); }
        else { result = "무승부"; SetMessage("무승부"); }

        Debug.Log($"[Battle] 플레이어 {p} vs 적 {e} → {result} | 점수 {_playerScore} : {_enemyScore}");
        UpdateScoreUI();

        // 승부에 올린 카드 + 필드에 남은 모든 카드를 각자 버린 더미로.
        playerField.DiscardCard(_playerChosen, _playerPile);
        enemyField.DiscardCard(_enemyChosen, _enemyPile);
        playerField.DiscardAll(_playerPile);
        enemyField.DiscardAll(_enemyPile);
        _playerChosen = null;
        _enemyChosen = null;

        if (_playerScore >= winTarget) { EndGame(true); return; }
        if (_enemyScore >= winTarget) { EndGame(false); return; }

        Tw.Delay(nextRoundDelay, StartRound);
    }

    void EndGame(bool playerWon)
    {
        _gameOver = true;
        SetMessage(playerWon ? "게임 승리!" : "게임 패배...");
        if (playerInteractor != null) playerInteractor.SetLocked(true);

        // 승리면 클리어 표시(처음이면 최초 클리어 = 보상 대상).
        bool firstClear = playerWon && progress != null && _activeStageIndex >= 0
                          && progress.MarkCleared(_activeStageIndex);

        // 결과를 잠깐 보여준 뒤 스테이지로 복귀 → 이동이 멈추면 보상 지급.
        Tw.Delay(endReturnDelay, () =>
        {
            if (screenFlow != null)
            {
                screenFlow.GoTo(stageScreenIndex, () =>
                {
                    screenFlow.SetNavigationLocked(false); // 복귀 완료 후 이동 허용
                    if (firstClear) GrantReward();
                });
            }
            else if (firstClear)
            {
                GrantReward();
            }
        });
    }

    // 승부 비교: a가 이기면 +1, b가 이기면 -1, 무승부 0. 특수룰(낮은 수가 특정 높은 수를 이김) 반영.
    int CompareCards(int a, int b)
    {
        if (a == b) return 0;
        if (a == upsetLow && b == upsetHigh) return 1;
        if (b == upsetLow && a == upsetHigh) return -1;
        return a > b ? 1 : -1;
    }

    void GrantReward()
    {
        if (_activeStage == null || _activeStage.FirstClearReward == null || playerDeck == null) return;
        if (playerDeck.AddCard(_activeStage.FirstClearReward))
            Debug.Log($"[Battle] 보상 획득: {_activeStage.FirstClearReward.DisplayName}");
    }

    void UpdateScoreUI()
    {
        if (playerScoreText != null) playerScoreText.text = _playerScore.ToString();
        if (enemyScoreText != null) enemyScoreText.text = _enemyScore.ToString();
        if (playerScoreBoard != null) playerScoreBoard.SetScore(_playerScore);
        if (enemyScoreBoard != null) enemyScoreBoard.SetScore(_enemyScore);
    }

    void SetMessage(string msg)
    {
        if (messageText != null) messageText.text = msg;
    }
}
