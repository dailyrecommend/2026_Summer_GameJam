using System;

namespace TweenKit
{
    /// <summary>
    /// 임의의 값 T를 보간하는 트윈. getter로 시작값을 캡처하고 setter로 매 프레임 값을 적용한다.
    /// </summary>
    public sealed class Tweener<T> : Tween
    {
        readonly Func<T> getter;
        readonly Action<T> setter;
        readonly Func<T, T, float, T> lerp;   // (from, to, t) → 보간값 (unclamped 권장)
        readonly Func<T, T, T> add;           // relative 모드용 (a + b), 없으면 relative 불가

        T startValue;
        T endValue;
        bool relative;

        internal Tweener(Func<T> getter, Action<T> setter, T endValue, float duration,
                         Func<T, T, float, T> lerp, Func<T, T, T> add = null)
        {
            this.getter = getter;
            this.setter = setter;
            this.endValue = endValue;
            this.duration = duration;
            this.lerp = lerp;
            this.add = add;
        }

        protected override void OnBegin()
        {
            startValue = getter();
            if (relative && add != null)
                endValue = add(startValue, endValue);
        }

        protected override void ApplyProgress(float easedT, int loopCount)
        {
            setter(lerp(startValue, endValue, easedT));
        }

        /// <summary>도착값을 시작값 기준의 상대 오프셋으로 해석.</summary>
        public Tweener<T> SetRelative(bool value = true) { relative = value; return this; }
    }
}
