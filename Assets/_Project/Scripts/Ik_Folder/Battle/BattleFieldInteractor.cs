using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 필드 카드 호버/클릭을 처리한다.
///  - 호버 → 확대 + 툴팁(CardHovered)
///  - 클릭 → 상승(다른 카드가 올라와 있으면 내림)
///  - 올라온 카드를 다시 클릭 → 승부(CardCommitted)
///  - 빈 곳 클릭 → 올라온 카드 원위치
/// </summary>
[DisallowMultipleComponent]
public class BattleFieldInteractor : MonoBehaviour
{
    [SerializeField] Camera cam;
    [Tooltip("필드 카드가 속한 레이어만 검사(선택). 기본 전체")]
    [SerializeField] LayerMask fieldMask = ~0;
    [Tooltip("체크하면 호버(툴팁/확대)만 하고 클릭·승부는 하지 않음 (에너미 필드용)")]
    [SerializeField] bool hoverOnly = false;

    /// <summary>카드를 승부에 올렸을 때(같은 카드 재클릭).</summary>
    public event Action<FieldCard> CardCommitted;
    /// <summary>카드에 마우스를 올렸을 때(툴팁 표시용). 두 번째 인자는 카드가 뒷면인지.</summary>
    public event Action<CardData, bool> CardHovered;
    /// <summary>카드에서 마우스가 벗어났을 때(툴팁 숨김용).</summary>
    public event Action CardUnhovered;

    FieldCard _raised;
    FieldCard _hovered;
    bool _locked;

    /// <summary>승부 진행 중 등 입력을 막고 싶을 때.</summary>
    public void SetLocked(bool locked) => _locked = locked;

    void Awake()
    {
        if (cam == null) cam = Camera.main;
    }

    void Update()
    {
        if (cam == null || Mouse.current == null) return;

        // 1) 커서 아래 필드 카드 찾기
        FieldCard hit = null;
        Vector3 hitPoint = default;
        if (!_locked)
        {
            Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit info, Mathf.Infinity, fieldMask))
            {
                hit = info.collider.GetComponentInParent<FieldCard>();
                hitPoint = info.point;
            }
        }

        // 2) 호버 갱신
        if (hit != _hovered)
        {
            if (_hovered != null) _hovered.SetHovered(false);
            _hovered = hit;
            if (_hovered != null) _hovered.SetHovered(true);
            else CardUnhovered?.Invoke();
        }

        if (_hovered != null)
        {
            // 매 프레임 갱신 → 딜 중 뒷면→앞면 전환도 툴팁에 바로 반영됨.
            CardHovered?.Invoke(_hovered.Data, _hovered.IsFaceDown);
            // 커서 위치 전달 → 커서 쪽으로 기울기.
            _hovered.SetHoverPoint(hitPoint);
        }

        // 3) 클릭 처리 (호버 전용이면 생략)
        if (_locked || hoverOnly) return;
        if (!Mouse.current.leftButton.wasPressedThisFrame) return;

        if (hit == null)
        {
            // 빈 곳 클릭 → 원위치
            if (_raised != null) { _raised.SetRaised(false); _raised = null; }
            return;
        }

        if (hit == _raised)
        {
            // 같은 카드 재클릭 → 승부
            FieldCard committed = _raised;
            _raised = null;
            CardCommitted?.Invoke(committed);
        }
        else
        {
            // 다른 카드 클릭 → 이전 내리고 새로 상승
            if (_raised != null) _raised.SetRaised(false);
            _raised = hit;
            hit.SetRaised(true);
        }
    }

    void OnDisable()
    {
        if (_raised != null) { _raised.SetRaised(false); _raised = null; }
        if (_hovered != null)
        {
            _hovered.SetHovered(false);
            _hovered = null;
            CardUnhovered?.Invoke();
        }
    }
}
