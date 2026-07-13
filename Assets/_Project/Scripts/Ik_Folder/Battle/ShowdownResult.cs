/// <summary>
/// 한 번의 승부 판정 정보. 특수 카드가 여기에 개입해 결과를 바꾼다.
/// (숫자 보정, 무승부 강제 등 — 필요한 필드를 계속 늘려간다)
/// </summary>
public class ShowdownResult
{
    public int PlayerNumber;
    public int EnemyNumber;

    /// <summary>true면 숫자와 무관하게 무승부 처리(조커 등).</summary>
    public bool ForceDraw;
}
