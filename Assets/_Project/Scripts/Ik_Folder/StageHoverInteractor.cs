using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 마우스 아래의 스테이지(StageView)를 감지해 호버 이벤트를 낸다. (툴팁 표시용, 호버 전용)
/// 스테이지 오브젝트에는 Collider가 있어야 한다. 씬에 하나만 두면 된다.
/// </summary>
[DisallowMultipleComponent]
public class StageHoverInteractor : MonoBehaviour
{
    [Tooltip("레이캐스트 카메라. 비우면 Camera.main")]
    [SerializeField] Camera cam;
    [Tooltip("스테이지가 속한 레이어만 검사(선택). 기본 전체")]
    [SerializeField] LayerMask stageMask = ~0;

    public event Action<StageData> StageHovered;
    public event Action StageUnhovered;

    StageView _hovered;

    void Awake()
    {
        if (cam == null) cam = Camera.main;
    }

    void Update()
    {
        if (cam == null || Mouse.current == null) return;

        StageView hit = null;
        if (!UIPointer.IsOverUI)
        {
            Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit info, Mathf.Infinity, stageMask))
                hit = info.collider.GetComponentInParent<StageView>();
        }

        if (hit != _hovered)
        {
            _hovered = hit;
            if (_hovered != null && _hovered.Data != null) StageHovered?.Invoke(_hovered.Data);
            else StageUnhovered?.Invoke();
        }
    }

    void OnDisable()
    {
        if (_hovered != null)
        {
            _hovered = null;
            StageUnhovered?.Invoke();
        }
    }
}
