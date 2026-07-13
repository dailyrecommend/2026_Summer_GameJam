using UnityEngine;

namespace TweenKit
{
    /// <summary>
    /// TweenKit 사용 예제. 아무 오브젝트에 붙이고 재생하면 데모가 실행된다.
    /// (실제 프로젝트에서는 지워도 됨)
    /// </summary>
    public class TweenKitExample : MonoBehaviour
    {
        [SerializeField] Transform target;

        void Start()
        {
            if (target == null) target = transform;

            // 1) 기본: 2초 동안 위로 이동 + 이징 + 콜백
            target.DOMoveY(3f, 2f)
                  .SetEase(Ease.OutBounce)
                  .OnComplete(() => Debug.Log("이동 완료!"));

            // 2) 무한 요요 회전
            target.DOLocalRotate(new Vector3(0, 360f, 0), 3f)
                  .SetEase(Ease.Linear)
                  .SetLoops(-1, LoopType.Incremental);

            // 3) 딜레이 후 실행
            Tw.Delay(1f, () => Debug.Log("1초 뒤 호출"));

            // 4) 시퀀스: 커졌다가 → 흔들고 → 작아지기
            Tw.Sequence()
              .Append(target.DOScale(1.5f, 0.4f).SetEase(Ease.OutBack))
              .AppendInterval(0.2f)
              .Append(target.DOShakePosition(0.5f, 0.3f))
              .Append(target.DOScale(1f, 0.3f))
              .AppendCallback(() => Debug.Log("시퀀스 끝"))
              .SetLoops(2, LoopType.Restart);

            // 5) 임의 값 트윈 (예: 점수 카운트업)
            int score = 0;
            Tw.To(() => score, v => { score = v; /* scoreText.text = v.ToString(); */ }, 100, 1.5f)
              .SetEase(Ease.OutCubic);
        }
    }
}
