using System;
using UnityEngine;

namespace TweenKit
{
    /// <summary>루프 방식.</summary>
    public enum LoopType
    {
        /// <summary>끝나면 처음으로 되돌아가 다시 재생.</summary>
        Restart,
        /// <summary>앞으로 갔다가 뒤로 되돌아오기를 반복.</summary>
        Yoyo,
        /// <summary>매 루프마다 도착값이 누적(예: 계속 위로 이동).</summary>
        Incremental,
    }

    /// <summary>모든 트윈/시퀀스의 공통 베이스. TweenManager가 매 프레임 Tick을 호출한다.</summary>
    public abstract class Tween
    {
        // ── 설정값 ──
        protected float duration = 0.5f;
        protected float delay;
        protected Ease ease = Ease.OutQuad;
        protected AnimationCurve customCurve;   // null이 아니면 ease 대신 사용
        protected int loops = 1;                // -1 이면 무한
        protected LoopType loopType = LoopType.Restart;
        protected bool ignoreTimeScale;
        protected bool autoKill = true;

        // ── 콜백 ──
        protected Action onStart;
        protected Action onComplete;
        protected Action onKill;
        protected Action onStepComplete;
        protected Action<float> onUpdate;       // 인자는 전체 진행도(0~1)

        // ── 런타임 상태 ──
        protected float elapsed;                // 현재 루프에서 경과 시간
        protected float delayElapsed;
        protected int completedLoops;
        protected bool started;
        bool isPlaying = true;
        bool isAlive = true;
        bool _seqCompleted; // 시퀀스 구동 시 onComplete 1회 발동 가드

        public bool IsPlaying => isPlaying;
        public bool IsAlive => isAlive;
        public bool IgnoreTimeScale => ignoreTimeScale;

        /// <summary>딜레이 + (지속시간 × 루프)로 계산한 전체 재생 시간. 무한 루프는 1회로 간주.</summary>
        internal float FullDuration => delay + duration * (loops < 0 ? 1 : Mathf.Max(1, loops));

        internal bool OwnedBySequence;

        /// <summary>파생 클래스가 실제 값을 적용하는 지점. easedT는 이징이 적용된 0~1(또는 Incremental시 그 이상).</summary>
        protected abstract void ApplyProgress(float easedT, int loopCount);

        /// <summary>트윈이 시작될 때 시작값을 캡처하는 훅.</summary>
        protected virtual void OnBegin() { }

        // ─────────────────────────────────────────────────────────
        //  Manager 호출부
        // ─────────────────────────────────────────────────────────
        internal void Tick(float unscaledDelta, float scaledDelta)
        {
            if (!isAlive || !isPlaying) return;
            float dt = ignoreTimeScale ? unscaledDelta : scaledDelta;

            if (delayElapsed < delay)
            {
                delayElapsed += dt;
                if (delayElapsed < delay) return;
                dt = delayElapsed - delay; // 딜레이 초과분을 이월
            }

            if (!started)
            {
                started = true;
                OnBegin();
                onStart?.Invoke();
            }

            elapsed += dt;

            // 한 프레임에 여러 루프가 끝날 수 있으므로 while 처리.
            // 경계 포즈는 '방금 끝난 루프'의 끝(t=1)으로 보고해야 요요 방향이 맞다 → 증가 전에 EmitStep.
            while (duration > 0f && elapsed >= duration && (loops < 0 || completedLoops < loops - 1))
            {
                EmitStep();
                onStepComplete?.Invoke();
                elapsed -= duration;
                completedLoops++;
            }

            if (loops >= 0 && completedLoops >= loops - 1 && elapsed >= duration)
            {
                // 마지막 루프 완료 (EmitStep이 t=1 경계 포즈를 적용)
                elapsed = duration;
                EmitStep();
                onComplete?.Invoke();
                if (autoKill) Kill();
                else { isPlaying = false; }
                return;
            }

            Report(duration <= 0f ? 1f : elapsed / duration);
        }

        void EmitStep()
        {
            // 완전한 스텝 경계값을 확실히 적용(요요 방향 포함).
            Report(1f);
        }

        void Report(float normalized)
        {
            int currentLoop = Mathf.Max(0, completedLoops);
            float t = normalized;

            // Yoyo: 홀수 루프는 역방향
            if (loopType == LoopType.Yoyo && (currentLoop & 1) == 1)
                t = 1f - t;

            float eased = customCurve != null ? customCurve.Evaluate(t) : Easing.Evaluate(ease, t);
            // Incremental 루프는 도착값을 매 루프 누적(unclamped 보간에 loop 오프셋 전달).
            if (loopType == LoopType.Incremental && currentLoop > 0)
                eased += currentLoop;
            ApplyProgress(eased, currentLoop);
            onUpdate?.Invoke(normalized);
        }

        /// <summary>
        /// 시퀀스가 자식 트윈을 절대 로컬 시간으로 구동할 때 사용. (딜레이 시작 기준 0부터)
        /// </summary>
        internal void SampleAbsolute(float localTime)
        {
            if (!isAlive) return;

            if (localTime < delay) return; // 아직 시작 전 (시작값 유지)

            if (!started)
            {
                started = true;
                OnBegin();
                onStart?.Invoke();
            }

            float active = localTime - delay;
            int maxLoops = loops < 0 ? int.MaxValue : Mathf.Max(1, loops);
            int loop = duration <= 0f ? maxLoops : (int)(active / duration);

            if (loop >= maxLoops)
            {
                completedLoops = maxLoops - 1;
                Report(1f);
                if (!_seqCompleted)
                {
                    _seqCompleted = true;
                    onStepComplete?.Invoke();
                    onComplete?.Invoke();
                }
                return;
            }

            completedLoops = loop;
            elapsed = active - loop * duration;
            Report(duration <= 0f ? 1f : elapsed / duration);
        }

        // ─────────────────────────────────────────────────────────
        //  제어 API
        // ─────────────────────────────────────────────────────────
        public Tween Pause() { isPlaying = false; return this; }
        public Tween Play() { if (isAlive) isPlaying = true; return this; }
        public Tween TogglePause() { isPlaying = !isPlaying; return this; }

        /// <summary>처음 상태로 되돌리고 정지.</summary>
        public Tween Rewind()
        {
            elapsed = 0f; delayElapsed = 0f; completedLoops = 0; started = false; isPlaying = false;
            return this;
        }

        /// <summary>처음부터 다시 재생.</summary>
        public Tween Restart()
        {
            elapsed = 0f; delayElapsed = 0f; completedLoops = 0; started = false; isPlaying = true;
            return this;
        }

        /// <summary>즉시 끝 상태로 보내고 완료 처리.</summary>
        public Tween Complete()
        {
            if (!isAlive) return this;
            completedLoops = loops < 0 ? 0 : Mathf.Max(0, loops - 1);
            if (!started) { started = true; OnBegin(); onStart?.Invoke(); }
            Report(1f);
            onComplete?.Invoke();
            if (autoKill) Kill(); else isPlaying = false;
            return this;
        }

        /// <summary>트윈을 즉시 종료(더 이상 갱신 안 함).</summary>
        public void Kill()
        {
            if (!isAlive) return;
            isAlive = false;
            isPlaying = false;
            onKill?.Invoke();
        }

        // ─────────────────────────────────────────────────────────
        //  설정 체이닝 API
        // ─────────────────────────────────────────────────────────
        public Tween SetEase(Ease value) { ease = value; customCurve = null; return this; }
        public Tween SetEase(AnimationCurve curve) { customCurve = curve; return this; }
        public Tween SetDelay(float seconds) { delay = Mathf.Max(0f, seconds); return this; }
        public Tween SetLoops(int count, LoopType type = LoopType.Restart) { loops = count; loopType = type; return this; }
        public Tween SetUpdate(bool useUnscaledTime) { ignoreTimeScale = useUnscaledTime; return this; }
        public Tween SetAutoKill(bool value) { autoKill = value; return this; }

        public Tween OnStart(Action cb) { onStart += cb; return this; }
        public Tween OnComplete(Action cb) { onComplete += cb; return this; }
        public Tween OnKill(Action cb) { onKill += cb; return this; }
        public Tween OnStepComplete(Action cb) { onStepComplete += cb; return this; }
        public Tween OnUpdate(Action<float> cb) { onUpdate += cb; return this; }
    }
}
