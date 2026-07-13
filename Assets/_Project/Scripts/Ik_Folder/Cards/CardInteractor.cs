using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 마우스 아래의 카드를 레이캐스트로 찾아 호버(확대) 상태를 넘기고, 클릭을 감지해 알린다.
/// 카드 프리팹에는 Collider가 있어야 한다. 씬에 하나만 두면 된다.
/// </summary>
[DisallowMultipleComponent]
public class CardInteractor : MonoBehaviour
{
    [Tooltip("레이캐스트 카메라. 비우면 Camera.main")]
    [SerializeField] Camera cam;
    [Tooltip("카드가 속한 레이어만 검사(선택). 기본 전체")]
    [SerializeField] LayerMask cardMask = ~0;

    /// <summary>카드에 마우스를 올렸을 때(툴팁 표시용).</summary>
    public event Action<CardData> CardHovered;
    /// <summary>카드에서 마우스가 벗어났을 때(툴팁 숨김용).</summary>
    public event Action CardUnhovered;
    /// <summary>카드를 클릭했을 때.</summary>
    public event Action<CardData> CardClicked;
    /// <summary>카드가 아닌 빈 곳을 클릭했을 때.</summary>
    public event Action BackgroundClicked;

    CardView _hovered;

    void Awake()
    {
        if (cam == null) cam = Camera.main;
    }

    void Update()
    {
        if (cam == null || Mouse.current == null) return;

        // 1) 커서 아래 카드 찾기
        CardView hit = null;
        Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit info, Mathf.Infinity, cardMask))
            hit = info.collider.GetComponentInParent<CardView>();

        // 2) 호버 상태 갱신
        if (hit != _hovered)
        {
            if (_hovered != null) _hovered.SetHovered(false);
            _hovered = hit;
            if (_hovered != null)
            {
                _hovered.SetHovered(true);
                CardHovered?.Invoke(_hovered.Data);
            }
            else CardUnhovered?.Invoke();
        }

        // 3) 클릭 처리
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (_hovered != null) CardClicked?.Invoke(_hovered.Data);
            else BackgroundClicked?.Invoke();
        }
    }

    void OnDisable()
    {
        // 화면 전환 등으로 비활성화되면 호버 해제.
        if (_hovered != null)
        {
            _hovered.SetHovered(false);
            _hovered = null;
            CardUnhovered?.Invoke();
        }
    }
}
