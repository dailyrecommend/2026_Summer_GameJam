using UnityEngine;

namespace TweenKit
{
    /// <summary>흔들기(Shake)·펀치(Punch) 같은 감쇠 진동 이펙트.</summary>
    public static class TweenEffects
    {
        /// <summary>위치를 무작위로 흔든 뒤 원위치로 감쇠. UI 히트/피격 연출 등에 유용.</summary>
        public static Tween DOShakePosition(this Transform t, float duration,
            float strength = 0.5f, int vibrato = 10)
        {
            Vector3 basePos = t.position;
            float seed = Random.value * 100f;

            var tw = Tw.To(() => 0f, val =>
            {
                float damper = 1f - val;                    // 시간이 지날수록 진폭 감소
                float ang = val * vibrato * Mathf.PI * 2f;
                Vector3 dir = new Vector3(
                    Mathf.Sin(ang + seed),
                    Mathf.Cos(ang * 1.3f + seed * 1.7f),
                    0f);
                t.position = basePos + dir * (strength * damper);
            }, 1f, duration).SetEase(Ease.Linear);

            tw.OnComplete(() => t.position = basePos);
            return tw;
        }

        /// <summary>스케일을 흔든 뒤 원상 복귀.</summary>
        public static Tween DOShakeScale(this Transform t, float duration,
            float strength = 0.3f, int vibrato = 10)
        {
            Vector3 baseScale = t.localScale;
            float seed = Random.value * 100f;

            var tw = Tw.To(() => 0f, val =>
            {
                float damper = 1f - val;
                float ang = val * vibrato * Mathf.PI * 2f;
                Vector3 d = new Vector3(
                    Mathf.Sin(ang + seed),
                    Mathf.Sin(ang * 1.2f + seed),
                    Mathf.Sin(ang * 1.4f + seed)) * (strength * damper);
                t.localScale = baseScale + d;
            }, 1f, duration).SetEase(Ease.Linear);

            tw.OnComplete(() => t.localScale = baseScale);
            return tw;
        }

        /// <summary>지정 방향으로 튕겼다가 감쇠 진동하며 원위치로 복귀.</summary>
        public static Tween DOPunchPosition(this Transform t, Vector3 punch, float duration, int vibrato = 10)
        {
            Vector3 basePos = t.position;
            var tw = Tw.To(() => 0f, val =>
            {
                float damper = 1f - val;
                float osc = Mathf.Sin(val * vibrato * Mathf.PI);
                t.position = basePos + punch * (osc * damper);
            }, 1f, duration).SetEase(Ease.Linear);

            tw.OnComplete(() => t.position = basePos);
            return tw;
        }

        /// <summary>스케일을 부풀렸다 감쇠하며 복귀. 버튼 클릭·팝업 등장 연출에 유용.</summary>
        public static Tween DOPunchScale(this Transform t, Vector3 punch, float duration, int vibrato = 10)
        {
            Vector3 baseScale = t.localScale;
            var tw = Tw.To(() => 0f, val =>
            {
                float damper = 1f - val;
                float osc = Mathf.Sin(val * vibrato * Mathf.PI);
                t.localScale = baseScale + punch * (osc * damper);
            }, 1f, duration).SetEase(Ease.Linear);

            tw.OnComplete(() => t.localScale = baseScale);
            return tw;
        }
    }
}
