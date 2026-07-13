using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

/// <summary>
/// 마우스로 클릭 가능한 3D 오브젝트. Collider가 반드시 있어야 한다.
/// 클릭되면 onClick 이벤트를 호출한다. (인스펙터에서 ScreenFlowController.GoToStage 등을 연결)
/// 새 Input System 기반.
/// </summary>
[RequireComponent(typeof(Collider))]
[DisallowMultipleComponent]
public class ClickableObject3D : MonoBehaviour
{
    [Tooltip("레이캐스트에 사용할 카메라. 비워두면 Camera.main 사용")]
    [SerializeField] Camera cam;

    [Tooltip("한 번 클릭되면 더 이상 클릭을 받지 않음")]
    [SerializeField] bool oneShot = false;

    public UnityEvent onClick;

    Collider _col;
    bool _consumed;

    void Awake()
    {
        _col = GetComponent<Collider>();
        if (cam == null) cam = Camera.main;
    }

    void Update()
    {
        if (_consumed) return;
        if (Mouse.current == null || cam == null) return;
        if (!Mouse.current.leftButton.wasPressedThisFrame) return;

        Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity) && hit.collider == _col)
        {
            if (oneShot) _consumed = true;
            onClick?.Invoke();
        }
    }
}
