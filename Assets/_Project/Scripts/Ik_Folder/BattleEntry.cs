using UnityEngine;

/// <summary>
/// 스테이지 → 전투 화면 전환. (보드게임 표시는 BattleManager.SpawnBoardGame이 스테이지별로 직접 생성/배치한다.
/// 예전엔 여기서 placeholder 박스를 이동시켰지만 이제 불필요해서 제거함)
/// </summary>
public class BattleEntry : MonoBehaviour
{
    [Header("화면 전환")]
    [SerializeField] ScreenFlowController screenFlow;
    [SerializeField] int battleScreenIndex = 2;

    [Tooltip("배틀 패널 전환이 끝난 뒤 호출. 여기에 BattleManager.StartBattle 등을 연결")]
    [SerializeField] UnityEngine.Events.UnityEvent onBattleEntered;

    bool _entered;

    /// <summary>전투 진입. (스테이지 더블클릭 등에서 호출)</summary>
    public void EnterBattle()
    {
        if (_entered) return;
        _entered = true;

        if (screenFlow != null)
            screenFlow.GoTo(battleScreenIndex, () => onBattleEntered?.Invoke());
        else
            onBattleEntered?.Invoke();
    }

    /// <summary>스테이지로 복귀.</summary>
    public void ExitBattle()
    {
        if (!_entered) return;
        _entered = false;

        if (screenFlow != null) screenFlow.GoToStage();
    }
}
