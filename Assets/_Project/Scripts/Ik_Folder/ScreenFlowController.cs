using UnityEngine;
using TweenKit;

/// <summary>
/// 화면(패널)들을 하나의 컨테이너에 세로로 이어 붙여 놓고, 컨테이너만 움직여
/// 카메라 앞에 원하는 화면이 오도록 전환하는 '필름스트립' 방식 전환기.
///
/// 배치 예) ScreensRoot 의 자식으로 Main(y=0), Stage(y=-10), Battle(y=-20) ...
/// Stage를 Main보다 아래에 두면 "메인이 위로 올라가며 스테이지가 올라오는" 연출이 된다.
/// </summary>
[DisallowMultipleComponent]
public class ScreenFlowController : MonoBehaviour
{
    [Tooltip("실제로 움직일 컨테이너 (화면 패널들의 부모)")]
    [SerializeField] Transform screensRoot;

    [Tooltip("화면 패널들. 0번이 시작 화면(Main). 순서대로 Stage, Battle ...")]
    [SerializeField] Transform[] screens;

    [Header("카메라 패럴랙스")]
    [Tooltip("연결하면 아래 지정한 화면에서만 마우스 추종이 켜진다. (비워두면 무시)")]
    [SerializeField] CameraMouseParallax cameraParallax;
    [Tooltip("패럴랙스를 켤 화면 인덱스. 기본 0(Main)만.")]
    [SerializeField] int parallaxScreenIndex = 0;

    [Header("전환 연출")]
    [SerializeField] float duration = 0.7f;
    [SerializeField] Ease ease = Ease.InOutCubic;

    [Tooltip("체크하면 위 Ease 대신 아래 커브를 사용. 바운스/오버슛 세기를 직접 조절할 때.")]
    [SerializeField] bool useCustomCurve = false;
    [Tooltip("가로축=진행도(0~1), 세로축=값(0~1). 끝부분에 작은 언덕을 만들면 그만큼만 튕긴다.")]
    [SerializeField] AnimationCurve customCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    Vector3 _baseRootPos;
    int _current;
    Tween _active;

    /// <summary>현재 보고 있는 화면 인덱스.</summary>
    public int CurrentIndex => _current;

    void Awake()
    {
        _baseRootPos = screensRoot.localPosition;
        SnapTo(0); // 시작은 Main
    }

    // ── 인스펙터(UnityEvent)에서 연결 가능한 파라미터 없는 메서드 ──
    public void GoTo(int index) => GoTo(index, null);
    public void GoToMain() => GoTo(0, null);
    public void GoToStage() => GoTo(1, null);
    public void GoToBattle() => GoTo(2, null);

    /// <summary>인덱스로 해당 화면으로 전환. onArrived는 전환 완료 시 호출 (코드에서 사용).</summary>
    public void GoTo(int index, System.Action onArrived)
    {
        if (screens == null || index < 0 || index >= screens.Length) return;
        if (index == _current && _active == null) { onArrived?.Invoke(); return; }

        _active?.Kill();
        _current = index;
        UpdateParallax(index);

        Tween tw = screensRoot.DOLocalMove(TargetRootPosFor(index), duration);
        if (useCustomCurve) tw.SetEase(customCurve);
        else tw.SetEase(ease);
        tw.OnComplete(() => { _active = null; onArrived?.Invoke(); });
        _active = tw;
    }

    /// <summary>연출 없이 즉시 해당 화면으로 스냅.</summary>
    public void SnapTo(int index)
    {
        if (screens == null || index < 0 || index >= screens.Length) return;
        _current = index;
        screensRoot.localPosition = TargetRootPosFor(index);
        UpdateParallax(index);
    }

    void UpdateParallax(int index)
    {
        if (cameraParallax != null)
            cameraParallax.SetActive(index == parallaxScreenIndex);
    }

    /// <summary>screens[index]가 screens[0]의 원래 자리에 오도록 컨테이너가 있어야 할 위치.</summary>
    Vector3 TargetRootPosFor(int index)
    {
        Vector3 delta = screens[index].localPosition - screens[0].localPosition;
        return _baseRootPos - delta;
    }
}
