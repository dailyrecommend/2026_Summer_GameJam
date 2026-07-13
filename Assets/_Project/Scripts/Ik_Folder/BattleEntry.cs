using UnityEngine;
using TweenKit;

/// <summary>
/// 스테이지 → 전투 전환. 배경/카드더미는 ScreenFlowController로 아래에서 올라오게 하고,
/// 선택된 보드게임 오브젝트는 전투 화면 우측 자리로 이동+축소시킨다. (이미지 3 연출)
/// </summary>
public class BattleEntry : MonoBehaviour
{
    [Header("화면 전환")]
    [SerializeField] ScreenFlowController screenFlow;
    [SerializeField] int battleScreenIndex = 2;

    [Tooltip("배틀 패널 전환이 끝난 뒤 호출. 여기에 BattleField.DealFromDeck 등을 연결")]
    [SerializeField] UnityEngine.Events.UnityEvent onBattleEntered;

    [Header("보드게임 이동 (스테이지 중앙 → 전투 우측)")]
    [Tooltip("이동할 보드게임 오브젝트. 필름스트립(ScreensRoot)의 자식이 아니어야 독립적으로 움직임")]
    [SerializeField] Transform boardGame;
    [Tooltip("전투 화면에서 보드게임이 놓일 고정 위치(월드). 움직이는 패널의 자식이 아니어야 함")]
    [SerializeField] Transform battleBoardAnchor;
    [SerializeField] float boardMoveDuration = 0.7f;
    [SerializeField] Ease boardMoveEase = Ease.InOutCubic;
    [Tooltip("전투 화면에서의 보드게임 스케일(작게)")]
    [SerializeField] float boardTargetScale = 0.6f;

    Vector3 _boardStartPos;
    Vector3 _boardStartScale;
    bool _entered;

    void Awake()
    {
        if (boardGame != null)
        {
            _boardStartPos = boardGame.position;
            _boardStartScale = boardGame.localScale;
        }
    }

    /// <summary>전투 진입. (선택된 보드게임 클릭 등에서 호출)</summary>
    public void EnterBattle()
    {
        if (_entered) return;
        _entered = true;

        // 1) 배경/카드더미: 아래에서 올라오는 전환 → 완료되면 onBattleEntered 호출
        if (screenFlow != null)
            screenFlow.GoTo(battleScreenIndex, () => onBattleEntered?.Invoke());
        else
            onBattleEntered?.Invoke();

        // 2) 보드게임: 우측 고정 자리로 이동 + 축소
        if (boardGame != null && battleBoardAnchor != null)
        {
            boardGame.DOMove(battleBoardAnchor.position, boardMoveDuration).SetEase(boardMoveEase);
            boardGame.DOScale(boardTargetScale, boardMoveDuration).SetEase(boardMoveEase);
        }
    }

    /// <summary>스테이지로 복귀.</summary>
    public void ExitBattle()
    {
        if (!_entered) return;
        _entered = false;

        if (screenFlow != null) screenFlow.GoToStage();

        if (boardGame != null)
        {
            boardGame.DOMove(_boardStartPos, boardMoveDuration).SetEase(boardMoveEase);
            boardGame.DOScale(_boardStartScale.x, boardMoveDuration).SetEase(boardMoveEase);
        }
    }
}
