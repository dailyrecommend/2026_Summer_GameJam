using UnityEngine;

/// <summary>
/// 캐러셀에서 선택한 스테이지의 StageData를 골라 전투를 시작한다.
/// 이전 스테이지를 클리어하지 못했으면 전투를 시작하지 않고 스테이지 화면으로 되돌린다.
/// stages 배열의 순서를 캐러셀 자식(스테이지) 순서와 맞춘다.
/// </summary>
public class StageBattleLauncher : MonoBehaviour
{
    [SerializeField] BattleManager battleManager;
    [SerializeField] StageCarousel carousel;
    [SerializeField] StageProgress progress;
    [SerializeField] ScreenFlowController screenFlow;
    [Tooltip("잠겨서 되돌릴 스테이지 화면 인덱스")]
    [SerializeField] int stageScreenIndex = 1;
    [Tooltip("캐러셀 인덱스 순서와 맞춘 스테이지 데이터들")]
    [SerializeField] StageData[] stages;

    /// <summary>현재 선택된 스테이지로 전투 시작. (잠겨 있으면 시작 안 함)</summary>
    public void StartSelectedStage()
    {
        if (battleManager == null) return;

        int i = carousel != null ? carousel.CurrentIndex : 0;

        // 이전 스테이지 미클리어 → 잠김.
        if (progress != null && !progress.IsUnlocked(i))
        {
            Debug.Log($"[Stage] {i}번 스테이지 잠김 — 이전 스테이지를 먼저 클리어하세요.");
            if (screenFlow != null) screenFlow.GoTo(stageScreenIndex); // 전투 진입 취소, 스테이지로 복귀
            return;
        }

        StageData stage = (stages != null && i >= 0 && i < stages.Length) ? stages[i] : null;
        if (stage == null)
            Debug.LogWarning($"[StageBattleLauncher] 인덱스 {i}에 해당하는 StageData가 없습니다.");

        battleManager.StartBattle(stage, i);
    }
}
