using UnityEngine;
using TweenKit;

/// <summary>
/// 점수판. 슬롯(구슬) 렌더러들의 머티리얼을 빈/채움으로 바꿔 점수를 표시한다.
/// 플레이어용/적용으로 각각 하나씩 둔다.
/// </summary>
public class ScoreBoard : MonoBehaviour
{
    [Tooltip("점수 슬롯 렌더러들. 채워지는 순서대로(왼→오 등)")]
    [SerializeField] Renderer[] slots;

    [Header("머티리얼")]
    [SerializeField] Material emptyMaterial;
    [SerializeField] Material filledMaterial;

    [Header("득점 연출 (선택)")]
    [Tooltip("새로 채워진 슬롯이 살짝 튀는 연출")]
    [SerializeField] bool punchOnGain = true;
    [SerializeField] float punchScale = 0.3f;
    [SerializeField] float punchDuration = 0.3f;

    int _score;

    void Awake()
    {
        _score = 0;
        Apply(0, false); // 시작은 전부 빈 슬롯
    }

    /// <summary>점수를 지정 값으로 설정하고 슬롯 머티리얼을 갱신.</summary>
    public void SetScore(int score)
    {
        score = Mathf.Clamp(score, 0, slots != null ? slots.Length : 0);
        bool gained = score > _score;
        _score = score;
        Apply(score, gained);
    }

    public void ResetScore() => SetScore(0);

    void Apply(int score, bool gained)
    {
        if (slots == null) return;

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null) continue;
            slots[i].sharedMaterial = (i < score) ? filledMaterial : emptyMaterial;
        }

        // 방금 채워진 슬롯 튀기기
        if (gained && punchOnGain && score >= 1 && score <= slots.Length)
        {
            Renderer justFilled = slots[score - 1];
            if (justFilled != null)
                justFilled.transform.DOPunchScale(Vector3.one * punchScale, punchDuration);
        }
    }
}
