using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;

namespace TweenKit
{
    /// <summary>
    /// 활성 트윈을 매 프레임 갱신하는 정적 러너.
    /// GameObject/MonoBehaviour 없이 Unity PlayerLoop의 Update 단계에 콜백을 주입한다.
    /// </summary>
    public static class TweenManager
    {
        // PlayerLoop에 꽂을 우리 시스템을 식별하는 마커 타입.
        struct TweenKitLoop { }

        static readonly List<Tween> _tweens = new List<Tween>(64);
        static readonly List<Tween> _pending = new List<Tween>(16); // Tick 중 추가된 트윈 버퍼
        static bool _ticking;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Init()
        {
            // 도메인 리로드가 꺼져 있어도 매 플레이마다 상태를 초기화하고 훅을 다시 건다.
            _tweens.Clear();
            _pending.Clear();
            _ticking = false;
            InstallPlayerLoop();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged -= OnPlayModeChanged;
            UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeChanged;
#endif
        }

        static void InstallPlayerLoop()
        {
            PlayerLoopSystem root = PlayerLoop.GetCurrentPlayerLoop();
            var sys = new PlayerLoopSystem
            {
                type = typeof(TweenKitLoop),
                updateDelegate = Tick,
            };

            var subs = root.subSystemList;
            for (int i = 0; i < subs.Length; i++)
            {
                if (subs[i].type != typeof(Update)) continue;

                var list = new List<PlayerLoopSystem>(subs[i].subSystemList ?? Array.Empty<PlayerLoopSystem>());
                list.RemoveAll(s => s.type == typeof(TweenKitLoop)); // 중복 방지
                list.Add(sys);
                subs[i].subSystemList = list.ToArray();
                break;
            }

            root.subSystemList = subs;
            PlayerLoop.SetPlayerLoop(root);
        }

        static void Tick()
        {
            float unscaled = Time.unscaledDeltaTime;
            float scaled = Time.deltaTime;

            _ticking = true;
            for (int i = _tweens.Count - 1; i >= 0; i--)
            {
                Tween t = _tweens[i];
                if (t == null || !t.IsAlive)
                {
                    _tweens.RemoveAt(i);
                    continue;
                }

                try
                {
                    t.Tick(unscaled, scaled);
                }
                catch (Exception e)
                {
                    // 대상 오브젝트 파괴 등으로 예외가 나도 나머지 트윈은 계속 돌게 한다.
                    Debug.LogException(e);
                    t.Kill();
                    _tweens.RemoveAt(i);
                }
            }
            _ticking = false;

            if (_pending.Count > 0)
            {
                _tweens.AddRange(_pending);
                _pending.Clear();
            }
        }

        internal static void Register(Tween tween)
        {
            if (_ticking) _pending.Add(tween);
            else _tweens.Add(tween);
        }

        /// <summary>매니저 갱신 목록에서 제거. (시퀀스에 편입된 자식 트윈용)</summary>
        internal static void Unregister(Tween tween)
        {
            _tweens.Remove(tween);
            _pending.Remove(tween);
        }

        /// <summary>모든 활성 트윈을 즉시 종료.</summary>
        public static void KillAll(bool complete = false)
        {
            // 순회 중 변경에 안전하도록 스냅샷.
            for (int i = _tweens.Count - 1; i >= 0; i--)
            {
                Tween t = _tweens[i];
                if (t == null || !t.IsAlive) continue;
                if (complete) t.Complete();
                else t.Kill();
            }
        }

        /// <summary>현재 살아있는 트윈 개수.</summary>
        public static int ActiveCount => _tweens.Count;

#if UNITY_EDITOR
        static void OnPlayModeChanged(UnityEditor.PlayModeStateChange state)
        {
            if (state == UnityEditor.PlayModeStateChange.ExitingPlayMode)
            {
                _tweens.Clear();
                _pending.Clear();
                RemovePlayerLoop();
            }
        }

        static void RemovePlayerLoop()
        {
            PlayerLoopSystem root = PlayerLoop.GetCurrentPlayerLoop();
            var subs = root.subSystemList;
            for (int i = 0; i < subs.Length; i++)
            {
                if (subs[i].type != typeof(Update) || subs[i].subSystemList == null) continue;
                var list = new List<PlayerLoopSystem>(subs[i].subSystemList);
                if (list.RemoveAll(s => s.type == typeof(TweenKitLoop)) > 0)
                    subs[i].subSystemList = list.ToArray();
            }
            root.subSystemList = subs;
            PlayerLoop.SetPlayerLoop(root);
        }
#endif
    }
}
