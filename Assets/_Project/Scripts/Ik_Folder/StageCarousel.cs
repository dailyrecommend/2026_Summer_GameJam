using UnityEngine;
using UnityEngine.InputSystem;
using TweenKit;

/// <summary>
/// 가로로 늘어선 스테이지들을 마우스 드래그로 넘기고, 놓으면 가장 가까운 스테이지로 스냅한다.
/// 스테이지 개수는 content의 자식 수로 자동 인식 → 자식만 늘리면 스테이지가 추가된다.
/// 각 자식은 서로 다른 오브젝트(스테이지 표시물)를 넣으면 된다.
/// </summary>
[DisallowMultipleComponent]
public class StageCarousel : MonoBehaviour
{
    [Header("참조")]
    [Tooltip("스테이지 표시물들의 부모. 이 오브젝트가 좌우로 움직인다.")]
    [SerializeField] Transform content;
    [Tooltip("드래그 레이캐스트용 카메라. 비우면 Camera.main")]
    [SerializeField] Camera cam;

    [Header("레이아웃")]
    [Tooltip("스테이지 간 간격(로컬 X)")]
    [SerializeField] float spacing = 4f;

    [Header("스냅 연출")]
    [SerializeField] float snapDuration = 0.35f;
    [SerializeField] Ease snapEase = Ease.OutCubic;
    [Tooltip("스테이지를 넘기는 데 필요한 드래그 비율. 0.28 = 한 칸의 28%만 끌어도 넘어감.")]
    [Range(0.05f, 0.5f)]
    [SerializeField] float swipeThreshold = 0.28f;

    [Header("포커스 스케일 (중앙=크게, 옆=작게)")]
    [SerializeField] bool scaleFocused = true;
    [SerializeField] float centerScale = 1f;
    [SerializeField] float sideScale = 0.7f;

    int _count;
    int _index;
    bool _dragging;
    Vector3 _dragStartWorld;
    float _contentStartX;
    Plane _dragPlane;
    Tween _snapTween;
    Vector3[] _baseScales;

    /// <summary>현재 선택된 스테이지 인덱스.</summary>
    public int CurrentIndex => _index;
    /// <summary>현재 선택된 스테이지 Transform (없으면 null).</summary>
    public Transform CurrentStage => (_count > 0) ? content.GetChild(_index) : null;
    /// <summary>선택 스테이지가 바뀔 때 호출 (인자 = 새 인덱스).</summary>
    public System.Action<int> OnStageChanged;

    void Awake()
    {
        if (cam == null) cam = Camera.main;
        Layout();
    }

    /// <summary>자식들을 가로로 정렬. 런타임에 스테이지를 추가/제거했다면 다시 호출.</summary>
    public void Layout()
    {
        if (content == null) return;
        _count = content.childCount;
        _baseScales = new Vector3[_count];

        for (int i = 0; i < _count; i++)
        {
            Transform child = content.GetChild(i);
            child.localPosition = new Vector3(i * spacing, child.localPosition.y, child.localPosition.z);
            _baseScales[i] = child.localScale;
        }

        _index = Mathf.Clamp(_index, 0, Mathf.Max(0, _count - 1));
        SetContentX(-_index * spacing);
    }

    void Update()
    {
        if (cam == null || Mouse.current == null || _count == 0) return;

        var lmb = Mouse.current.leftButton;
        if (lmb.wasPressedThisFrame) BeginDrag();
        else if (_dragging && lmb.isPressed) UpdateDrag();
        else if (_dragging && lmb.wasReleasedThisFrame) EndDrag();
    }

    void LateUpdate()
    {
        if (!scaleFocused || _count == 0 || _baseScales == null) return;

        float contentX = content.localPosition.x;
        for (int i = 0; i < _count; i++)
        {
            Transform child = content.GetChild(i);
            // 중앙(카메라 정면)에서 몇 칸 떨어져 있는지 (0=정중앙, 1=한 칸 옆)
            float distInStages = Mathf.Abs((child.localPosition.x + contentX) / spacing);
            float s = Mathf.Lerp(centerScale, sideScale, Mathf.Clamp01(distInStages));
            child.localScale = _baseScales[i] * s;
        }
    }

    // ── 드래그 ────────────────────────────────────────────────
    void BeginDrag()
    {
        // 콘텐츠 위치를 지나며 카메라를 향하는 평면에 마우스를 투영.
        _dragPlane = new Plane(-cam.transform.forward, content.position);
        if (!TryGetPlanePoint(out Vector3 world)) return;

        _dragging = true;
        _snapTween?.Kill();
        _snapTween = null;
        _dragStartWorld = world;
        _contentStartX = content.localPosition.x;
    }

    void UpdateDrag()
    {
        if (!TryGetPlanePoint(out Vector3 world)) return;

        Vector3 delta = world - _dragStartWorld;
        Vector3 axis = content.parent != null ? content.parent.right : Vector3.right;
        float localDeltaX = Vector3.Dot(delta, axis);

        // 양 끝을 넘어가면 저항감(러버밴드).
        float x = _contentStartX + localDeltaX;
        float min = -(_count - 1) * spacing;
        float max = 0f;
        if (x > max) x = max + (x - max) * 0.35f;
        else if (x < min) x = min + (x - min) * 0.35f;

        SetContentX(x);
    }

    void EndDrag()
    {
        _dragging = false;

        // 현재 인덱스에서 얼마나 끌었는지(스테이지 단위). 임계값(swipeThreshold)만 넘으면 넘어감.
        float pos = -content.localPosition.x / spacing;
        float diff = pos - _index;
        float bias = 1f - swipeThreshold;

        int target = _index;
        if (diff > 0f) target = _index + Mathf.FloorToInt(diff + bias);
        else if (diff < 0f) target = _index + Mathf.CeilToInt(diff - bias);

        SnapTo(target);
    }

    // ── 이동 API ──────────────────────────────────────────────
    public void SnapTo(int index)
    {
        index = Mathf.Clamp(index, 0, Mathf.Max(0, _count - 1));
        bool changed = index != _index;
        _index = index;

        _snapTween?.Kill();
        _snapTween = content.DOLocalMoveX(-index * spacing, snapDuration).SetEase(snapEase);

        if (changed) OnStageChanged?.Invoke(_index);
    }

    public void Next() => SnapTo(_index + 1);
    public void Prev() => SnapTo(_index - 1);

    // ── 헬퍼 ──────────────────────────────────────────────────
    void SetContentX(float x)
    {
        Vector3 p = content.localPosition;
        p.x = x;
        content.localPosition = p;
    }

    bool TryGetPlanePoint(out Vector3 point)
    {
        Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (_dragPlane.Raycast(ray, out float enter))
        {
            point = ray.GetPoint(enter);
            return true;
        }
        point = default;
        return false;
    }
}
