using UnityEngine;
using UnityEngine.InputSystem;
using TweenKit;

/// <summary>
/// 화면(패널)들을 하나의 컨테이너에 세로로 이어 붙여 놓고, 컨테이너만 움직여
/// 카메라 앞에 원하는 화면이 오도록 전환하는 '필름스트립' 방식 전환기.
/// 세로 드래그로 화면을 넘길 수 있다(스테이지 캐러셀처럼 손끝을 따라오다 놓으면 스냅).
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
    [Tooltip("패럴랙스를 켤 화면 인덱스. (아래 '모든 화면'이 꺼져 있을 때만 사용)")]
    [SerializeField] int parallaxScreenIndex = 0;
    [Tooltip("체크하면 모든 화면에서 카메라 마우스 추종을 켠다.")]
    [SerializeField] bool parallaxOnAllScreens = true;

    [Header("전환 연출")]
    [SerializeField] float duration = 0.7f;
    [SerializeField] Ease ease = Ease.InOutCubic;

    [Tooltip("체크하면 위 Ease 대신 아래 커브를 사용. 바운스/오버슛 세기를 직접 조절할 때.")]
    [SerializeField] bool useCustomCurve = false;
    [Tooltip("가로축=진행도(0~1), 세로축=값(0~1). 끝부분에 작은 언덕을 만들면 그만큼만 튕긴다.")]
    [SerializeField] AnimationCurve customCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("세로 드래그 내비게이션")]
    [SerializeField] bool enableDrag = true;
    [Tooltip("드래그 투영용 카메라. 비우면 Camera.main")]
    [SerializeField] Camera cam;
    [Tooltip("드래그로 오갈 수 있는 화면 범위 (배틀 진입은 별도이므로 보통 0~1)")]
    [SerializeField] int dragMinIndex = 0;
    [SerializeField] int dragMaxIndex = 1;
    [Tooltip("다음 화면으로 넘기는 데 필요한 드래그 비율(0.28 = 28%)")]
    [Range(0.05f, 0.5f)]
    [SerializeField] float swipeThreshold = 0.28f;
    [Tooltip("축(세로/가로)을 정하기 전 무시하는 픽셀 데드존")]
    [SerializeField] float axisDeadzone = 10f;
    [Tooltip("한 화면을 넘기는 데 필요한 세로 드래그 = 화면 높이의 이 비율 (0.5 = 화면 절반)")]
    [Range(0.2f, 1f)]
    [SerializeField] float dragScreenFraction = 0.5f;
    [Tooltip("패널이 손끝과 반대로 움직이면 체크")]
    [SerializeField] bool invertDrag = false;

    [Header("전투 진입")]
    [Tooltip("이 인덱스 화면(전투 패널)에 도착을 '완료'하면 아래 이벤트 호출")]
    [SerializeField] int battleIndex = 2;
    [Tooltip("전투 패널 도착 완료 시 호출. 여기에 BattleManager.StartBattle 등을 연결")]
    [SerializeField] UnityEngine.Events.UnityEvent onEnteredBattle;

    [Header("오디오")]
    [Tooltip("전투(battleIndex) 화면이 아닌 곳(메인/스테이지 등)에 도착하면 재생할 비전투 브금")]
    [SerializeField] AudioClip nonCombatBgm;

    Vector3 _baseRootPos;
    int _current;
    Tween _active;

    // 드래그 상태
    bool _dragging;
    bool _axisLocked;
    bool _isVertical;
    Vector2 _downScreen;
    int _dragStartIndex;
    float _dragF;

    bool _navLocked;

    /// <summary>현재 보고 있는 화면 인덱스.</summary>
    public int CurrentIndex => _current;

    /// <summary>드래그 내비게이션 잠금(전투 중 등). 코드로 부르는 GoTo는 계속 동작.</summary>
    public void SetNavigationLocked(bool locked)
    {
        _navLocked = locked;
        if (locked) _dragging = false;
    }

    void Awake()
    {
        _baseRootPos = screensRoot.localPosition;
        if (cam == null) cam = Camera.main;

        // 드래그 범위가 잘못(max<=min) 설정돼 있으면 최소한 한 칸(메인↔스테이지)은 되게 보정.
        if (screens != null && screens.Length > 1 && dragMaxIndex <= dragMinIndex)
            dragMaxIndex = Mathf.Min(dragMinIndex + 1, screens.Length - 1);

        SnapTo(0); // 시작은 Main
    }

    void Update()
    {
        if (!enableDrag || _navLocked || screens == null || screensRoot == null) return;
        if (Mouse.current == null) return;

        var lmb = Mouse.current.leftButton;
        if (lmb.wasPressedThisFrame) { if (!UIPointer.IsOverUI) BeginDrag(); }
        else if (_dragging && lmb.isPressed) UpdateDrag();
        else if (_dragging && lmb.wasReleasedThisFrame) EndDrag();
    }

    // ── 드래그 ────────────────────────────────────────────────
    void BeginDrag()
    {
        _dragging = true;
        _axisLocked = false;
        _isVertical = false;
        _downScreen = Mouse.current.position.ReadValue();
        _dragStartIndex = _current;
        _dragF = _current;
    }

    void UpdateDrag()
    {
        Vector2 now = Mouse.current.position.ReadValue();
        Vector2 d = now - _downScreen;

        if (!_axisLocked)
        {
            if (d.magnitude < axisDeadzone) return;
            _axisLocked = true;
            _isVertical = Mathf.Abs(d.y) >= Mathf.Abs(d.x);
            // 드래그 범위 밖 화면(예: 배틀)에서는 세로 드래그 비활성.
            if (_current < dragMinIndex || _current > dragMaxIndex) _isVertical = false;

            if (_isVertical)
            {
                _active?.Kill();
                _active = null;
                // 드래그 중에는 스와이프 범위 패널을 모두 켠다.
                for (int i = 0; i < screens.Length; i++)
                    SetPanelActive(i, i >= dragMinIndex && i <= dragMaxIndex);
            }
        }

        if (!_isVertical) return; // 가로 드래그는 캐러셀에 양보

        // 화면 세로 드래그 비율 → 연속 인덱스(f). 실제 패널 위치들 사이를 보간하므로
        // 축(Y/Z 등)에 상관없이 동작. 방향이 반대로 느껴지면 invertDrag로 뒤집는다.
        float screenDy = now.y - _downScreen.y;
        float span = Mathf.Max(1f, Screen.height * dragScreenFraction);
        float f = _dragStartIndex + (invertDrag ? -1f : 1f) * (screenDy / span);

        // 러버밴드(범위 밖은 저항)
        if (f < dragMinIndex) f = dragMinIndex + (f - dragMinIndex) * 0.35f;
        else if (f > dragMaxIndex) f = dragMaxIndex + (f - dragMaxIndex) * 0.35f;

        _dragF = f;
        screensRoot.localPosition = PositionForF(f);
    }

    // 연속 인덱스 f를 실제 컨테이너 위치로 변환(인접 패널 위치 보간, 범위 밖은 외삽).
    Vector3 PositionForF(float f)
    {
        int i0 = Mathf.Clamp(Mathf.FloorToInt(f), dragMinIndex, dragMaxIndex - 1);
        float frac = f - i0;
        return Vector3.LerpUnclamped(TargetRootPosFor(i0), TargetRootPosFor(i0 + 1), frac);
    }

    void EndDrag()
    {
        _dragging = false;
        if (!_axisLocked || !_isVertical) return; // 가로/미세 드래그였으면 무시

        int start = _dragStartIndex;
        float diff = _dragF - start;
        float bias = 1f - swipeThreshold;

        int target = start;
        if (diff > 0f) target = start + Mathf.FloorToInt(diff + bias);
        else if (diff < 0f) target = start + Mathf.CeilToInt(diff - bias);
        target = Mathf.Clamp(target, dragMinIndex, dragMaxIndex);

        AnimateRootTo(target, null); // 항상 스냅(제자리여도 되돌림)
    }

    // ── 이동 API ──────────────────────────────────────────────
    public void GoTo(int index) => GoTo(index, null);
    public void GoToMain() => GoTo(0, null);
    public void GoToStage() => GoTo(1, null);
    public void GoToBattle() => GoTo(2, null);

    /// <summary>인덱스로 해당 화면으로 전환. onArrived는 전환 완료 시 호출.</summary>
    public void GoTo(int index, System.Action onArrived)
    {
        if (screens == null || index < 0 || index >= screens.Length) return;
        if (index == _current && _active == null) { onArrived?.Invoke(); return; }
        AnimateRootTo(index, onArrived);
    }

    void AnimateRootTo(int index, System.Action onArrived)
    {
        _active?.Kill();
        int prev = _current;
        _current = index;
        UpdateParallax(index);

        // 실제로 다른 화면으로 바뀔 때만(제자리 스냅 되돌림은 제외) 전환 사운드.
        if (index != prev && AudioManager.instance != null)
            AudioManager.instance.PlaySfx(AudioManager.Sfx.PanelSwitch);

        // 전환 중에는 지나가는 패널이 보여야 하므로 출발~도착 구간을 켠다.
        // 이동이 끝날 때까지는 아무것도 끄지 않는다(완료 시 ActivateOnly로 정리).
        int lo = Mathf.Min(prev, index);
        int hi = Mathf.Max(prev, index);
        for (int i = lo; i <= hi; i++)
            SetPanelActive(i, true);

        Tween tw = screensRoot.DOLocalMove(TargetRootPosFor(index), duration);
        if (useCustomCurve) tw.SetEase(customCurve);
        else tw.SetEase(ease);
        tw.OnComplete(() =>
        {
            _active = null;
            ActivateOnly(index);
            onArrived?.Invoke();
            if (index == battleIndex) onEnteredBattle?.Invoke(); // 전투 패널 도착 → 전투 시작
        });
        _active = tw;
    }

    /// <summary>연출 없이 즉시 해당 화면으로 스냅.</summary>
    public void SnapTo(int index)
    {
        if (screens == null || index < 0 || index >= screens.Length) return;
        _current = index;
        screensRoot.localPosition = TargetRootPosFor(index);
        ActivateOnly(index);
        UpdateParallax(index);
    }

    // ── 헬퍼 ──────────────────────────────────────────────────
    void ActivateOnly(int index)
    {
        for (int i = 0; i < screens.Length; i++)
            SetPanelActive(i, i == index);

        // 전투 화면이 아닌 곳(메인/스테이지 등)에 도착하면 비전투 브금으로 전환.
        // 전투 브금은 BattleManager.StartBattle이 스테이지별로 직접 재생한다.
        if (index != battleIndex && AudioManager.instance != null)
            AudioManager.instance.PlayBgm(nonCombatBgm);
    }

    void SetPanelActive(int index, bool active)
    {
        if (screens[index] != null && screens[index].gameObject.activeSelf != active)
            screens[index].gameObject.SetActive(active);
    }

    void UpdateParallax(int index)
    {
        if (cameraParallax != null)
            cameraParallax.SetActive(parallaxOnAllScreens || index == parallaxScreenIndex);
    }

    Vector3 TargetRootPosFor(int index)
    {
        Vector3 delta = screens[index].localPosition - screens[0].localPosition;
        return _baseRootPos - delta;
    }
}
