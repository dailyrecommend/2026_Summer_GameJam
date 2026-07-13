using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 스테이지 클리어 진행도. 이전 스테이지를 클리어해야 다음이 열린다(선형 잠금).
/// (저장은 아직 없음 — 세션 동안만 유지)
/// </summary>
public class StageProgress : MonoBehaviour
{
    readonly HashSet<int> _cleared = new HashSet<int>();

    /// <summary>해당 인덱스 스테이지가 열려 있는가. 0번은 항상 열림, 그 외는 이전이 클리어돼야 함.</summary>
    public bool IsUnlocked(int index) => index <= 0 || _cleared.Contains(index - 1);

    public bool IsCleared(int index) => _cleared.Contains(index);

    /// <summary>클리어 표시. 처음 클리어면 true(최초 클리어 → 보상 대상).</summary>
    public bool MarkCleared(int index) => _cleared.Add(index);
}
