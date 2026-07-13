using System;
using System.Collections.Generic;
using UnityEngine;

namespace TweenKit
{
    /// <summary>
    /// 여러 트윈/콜백/대기를 하나의 타임라인으로 묶는다.
    /// Append(순차), Join(병렬), Insert(임의 위치), AppendInterval(대기), AppendCallback(콜백).
    /// </summary>
    public sealed class Sequence : Tween
    {
        enum Kind { Tween, Callback }

        struct Element
        {
            public Kind kind;
            public Tween tween;
            public float start;     // 시퀀스 로컬 시작 시간
            public Action callback;
            public bool fired;      // 콜백 발동 여부(루프마다 리셋)
        }

        readonly List<Element> _elements = new List<Element>(8);
        float _cursor;              // 다음 Append 위치(현재 타임라인 끝)
        float _lastStart;           // 마지막으로 추가한 요소의 시작(Join 기준)
        int _lastLoop = -1;

        internal Sequence()
        {
            ease = Ease.Linear;
            duration = 0f;
            TweenManager.Register(this);
        }

        void Adopt(Tween t)
        {
            t.OwnedBySequence = true;
            TweenManager.Unregister(t);
        }

        void Recalc()
        {
            // 전체 길이는 모든 요소의 끝 중 최댓값.
            float max = 0f;
            for (int i = 0; i < _elements.Count; i++)
            {
                var e = _elements[i];
                float end = e.kind == Kind.Tween ? e.start + e.tween.FullDuration : e.start;
                if (end > max) max = end;
            }
            if (_cursor > max) max = _cursor;
            duration = max;
        }

        // ── 빌드 API ──────────────────────────────────────────────

        /// <summary>현재 타임라인 끝에 이어서 재생.</summary>
        public Sequence Append(Tween tween)
        {
            Adopt(tween);
            _lastStart = _cursor;
            _elements.Add(new Element { kind = Kind.Tween, tween = tween, start = _cursor });
            _cursor += tween.FullDuration;
            Recalc();
            return this;
        }

        /// <summary>직전에 Append/Insert한 요소와 같은 시작 시각에 동시에 재생.</summary>
        public Sequence Join(Tween tween)
        {
            Adopt(tween);
            _elements.Add(new Element { kind = Kind.Tween, tween = tween, start = _lastStart });
            float end = _lastStart + tween.FullDuration;
            if (end > _cursor) _cursor = end;
            Recalc();
            return this;
        }

        /// <summary>지정한 절대 시각(atPosition)에 삽입.</summary>
        public Sequence Insert(float atPosition, Tween tween)
        {
            Adopt(tween);
            _lastStart = atPosition;
            _elements.Add(new Element { kind = Kind.Tween, tween = tween, start = atPosition });
            float end = atPosition + tween.FullDuration;
            if (end > _cursor) _cursor = end;
            Recalc();
            return this;
        }

        /// <summary>타임라인 끝에 빈 대기 시간을 추가.</summary>
        public Sequence AppendInterval(float seconds)
        {
            _cursor += Mathf.Max(0f, seconds);
            Recalc();
            return this;
        }

        /// <summary>타임라인 끝 지점에 콜백 추가.</summary>
        public Sequence AppendCallback(Action callback)
        {
            _elements.Add(new Element { kind = Kind.Callback, callback = callback, start = _cursor });
            Recalc();
            return this;
        }

        /// <summary>지정한 절대 시각에 콜백 삽입.</summary>
        public Sequence InsertCallback(float atPosition, Action callback)
        {
            _elements.Add(new Element { kind = Kind.Callback, callback = callback, start = atPosition });
            if (atPosition > _cursor) _cursor = atPosition;
            Recalc();
            return this;
        }

        // ── 구동 ──────────────────────────────────────────────────

        protected override void ApplyProgress(float easedT, int loopCount)
        {
            // 시퀀스는 Linear이므로 easedT == 정규화 진행도.
            if (loopCount != _lastLoop)
            {
                _lastLoop = loopCount;
                for (int i = 0; i < _elements.Count; i++)
                {
                    var e = _elements[i];
                    e.fired = false;
                    _elements[i] = e;
                }
            }

            float localTime = easedT * duration;

            for (int i = 0; i < _elements.Count; i++)
            {
                Element e = _elements[i];
                if (e.kind == Kind.Callback)
                {
                    if (!e.fired && localTime >= e.start)
                    {
                        e.fired = true;
                        _elements[i] = e;
                        e.callback?.Invoke();
                    }
                    continue;
                }

                if (localTime >= e.start)
                    e.tween.SampleAbsolute(localTime - e.start);
            }
        }
    }
}
