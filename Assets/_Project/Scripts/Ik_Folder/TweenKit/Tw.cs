using System;
using UnityEngine;

namespace TweenKit
{
    /// <summary>
    /// TweenKit의 정적 진입점. 임의의 값을 트윈하거나 시퀀스/딜레이를 생성한다.
    /// 생성된 트윈은 자동으로 매니저에 등록되어 재생된다.
    /// </summary>
    public static class Tw
    {
        // 재사용을 위한 보간 함수 캐시 (unclamped: Incremental 루프 대비)
        static readonly Func<float, float, float, float> LerpFloat = Mathf.LerpUnclamped;
        static readonly Func<Vector2, Vector2, float, Vector2> LerpVec2 = Vector2.LerpUnclamped;
        static readonly Func<Vector3, Vector3, float, Vector3> LerpVec3 = Vector3.LerpUnclamped;
        static readonly Func<Color, Color, float, Color> LerpColor = Color.LerpUnclamped;
        static readonly Func<Quaternion, Quaternion, float, Quaternion> SlerpQuat = Quaternion.SlerpUnclamped;
        static readonly Func<int, int, float, int> LerpInt = (a, b, t) => Mathf.RoundToInt(Mathf.LerpUnclamped(a, b, t));

        static readonly Func<float, float, float> AddFloat = (a, b) => a + b;
        static readonly Func<Vector2, Vector2, Vector2> AddVec2 = (a, b) => a + b;
        static readonly Func<Vector3, Vector3, Vector3> AddVec3 = (a, b) => a + b;
        static readonly Func<Color, Color, Color> AddColor = (a, b) => a + b;
        static readonly Func<int, int, int> AddInt = (a, b) => a + b;

        static Tweener<T> Reg<T>(Tweener<T> t) { TweenManager.Register(t); return t; }

        // ── 값 트윈 ──────────────────────────────────────────────

        public static Tweener<float> To(Func<float> getter, Action<float> setter, float endValue, float duration)
            => Reg(new Tweener<float>(getter, setter, endValue, duration, LerpFloat, AddFloat));

        public static Tweener<Vector2> To(Func<Vector2> getter, Action<Vector2> setter, Vector2 endValue, float duration)
            => Reg(new Tweener<Vector2>(getter, setter, endValue, duration, LerpVec2, AddVec2));

        public static Tweener<Vector3> To(Func<Vector3> getter, Action<Vector3> setter, Vector3 endValue, float duration)
            => Reg(new Tweener<Vector3>(getter, setter, endValue, duration, LerpVec3, AddVec3));

        public static Tweener<Color> To(Func<Color> getter, Action<Color> setter, Color endValue, float duration)
            => Reg(new Tweener<Color>(getter, setter, endValue, duration, LerpColor, AddColor));

        public static Tweener<Quaternion> To(Func<Quaternion> getter, Action<Quaternion> setter, Quaternion endValue, float duration)
            => Reg(new Tweener<Quaternion>(getter, setter, endValue, duration, SlerpQuat));

        public static Tweener<int> To(Func<int> getter, Action<int> setter, int endValue, float duration)
            => Reg(new Tweener<int>(getter, setter, endValue, duration, LerpInt, AddInt));

        // ── 시퀀스 / 딜레이 ───────────────────────────────────────

        /// <summary>빈 시퀀스를 생성. Append/Join 등으로 채운다.</summary>
        public static Sequence Sequence() => new Sequence();

        /// <summary>seconds 후 callback 1회 실행. (스케일드 타임 기준)</summary>
        public static Sequence Delay(float seconds, Action callback)
            => Tw.Sequence().AppendInterval(seconds).AppendCallback(callback);

        // ── 전역 제어 ─────────────────────────────────────────────

        public static void KillAll(bool complete = false) => TweenManager.KillAll(complete);
        public static int ActiveCount => TweenManager.ActiveCount;
    }
}
