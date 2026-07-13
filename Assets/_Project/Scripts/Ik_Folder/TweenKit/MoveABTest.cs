using UnityEngine;

namespace TweenKit
{
    /// <summary>
    /// TweenKit 동작 확인용 간단 테스트. 오브젝트를 A 지점에서 B 지점으로 이동시킨다. (3D)
    /// 빈 오브젝트 두 개를 A/B로 두고 이 스크립트를 이동할 오브젝트(예: Cube)에 붙인다.
    /// </summary>
    public class MoveABTest : MonoBehaviour
    {
        [Header("이동 지점 (Transform이 없으면 아래 좌표값 사용)")]
        [SerializeField] Transform pointA;
        [SerializeField] Transform pointB;

        [Header("Transform 미지정 시 사용할 월드 좌표")]
        [SerializeField] Vector3 posA = new Vector3(-3f, 0f, 0f);
        [SerializeField] Vector3 posB = new Vector3(3f, 0f, 0f);

        [Header("트윈 설정")]
        [SerializeField] float duration = 1.5f;
        [SerializeField] float holdTime = 0.5f;   // 양 끝에서 멈추는 시간(초)
        [SerializeField] Ease ease = Ease.InOutQuad;
        [SerializeField] bool pingPong = true;    // A↔B 무한 왕복

        void Start()
        {
            Vector3 a = pointA != null ? pointA.position : posA;
            Vector3 b = pointB != null ? pointB.position : posB;

            // 시작은 A 지점에서.
            transform.position = a;

            if (pingPong)
            {
                // A → B → (멈춤) → A → (멈춤) 을 무한 반복.
                Tw.Sequence()
                  .Append(transform.DOMove(b, duration).SetEase(ease))
                  .AppendInterval(holdTime)
                  .Append(transform.DOMove(a, duration).SetEase(ease))
                  .AppendInterval(holdTime)
                  .SetLoops(-1, LoopType.Restart);
            }
            else
            {
                transform.DOMove(b, duration).SetEase(ease)
                         .OnComplete(() => Debug.Log("[MoveABTest] B 지점 도착!"));
            }
        }
    }
}
