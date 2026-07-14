/// <summary>
/// 한 번의 승부 판정 정보. 특수 카드가 여기에 개입해 결과를 바꾼다.
/// (숫자 보정, 무승부 강제, 승점 배수 등 — 필요한 필드를 계속 늘려간다)
/// </summary>
public class ShowdownResult
{
    public int PlayerNumber;
    public int EnemyNumber;

    /// <summary>true면 숫자와 무관하게 무승부 처리(조커 등).</summary>
    public bool ForceDraw;

    /// <summary>이번 승리 시 얻는 점수(기본 1). 다음 라운드 2배 예약이 소비되면 2가 됨.</summary>
    public int PlayerWinPoints = 1;
    public int EnemyWinPoints = 1;

    /// <summary>전 라운드에 각 측이 특수(고유) 카드를 냈는지. 조건부 효과용.</summary>
    public bool PlayerPlayedSpecialLast;
    public bool EnemyPlayedSpecialLast;

    /// <summary>이번 라운드에 각 측이 낸 카드가 특수(고유)인지. (리버스 등)</summary>
    public bool PlayerCardIsSpecial;
    public bool EnemyCardIsSpecial;

    /// <summary>true면 '다음 라운드' 그 측의 승점을 2배로 예약(드로우 2).</summary>
    public bool PlayerDoubleNextRound;
    public bool EnemyDoubleNextRound;

    /// <summary>true면 아래 ForcedCmp로 결과를 강제(1=플레이어 승, -1=적 승, 0=무승부).</summary>
    public bool ForceOutcome;
    public int ForcedCmp;

    /// <summary>true면 두 승부 카드를 교환(숫자는 이미 교환됨, 소유 더미도 서로 바꿈). 리버스.</summary>
    public bool SwapCards;

    /// <summary>true면 '다음 라운드' 그 측이 이겨도 승점을 얻지 못함(빗나감). 2배 예약보다 우선.</summary>
    public bool PlayerNoWinNextRound;
    public bool EnemyNoWinNextRound;

    /// <summary>필드/더미 참조. Bang처럼 상대 필드의 카드를 직접 조작하는 효과용(BattleManager가 채움).</summary>
    public BattleField PlayerField;
    public BattleField EnemyField;
    public CardPile PlayerPile;
    public CardPile EnemyPile;

    /// <summary>true면 상대 점수를 1점 깎는다(0점이면 그대로). 맥주.</summary>
    public bool PlayerStealPoint; // 플레이어가 상대(적) 점수를 깎음
    public bool EnemyStealPoint;  // 적이 상대(플레이어) 점수를 깎음
}
