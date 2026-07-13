using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 마우스 위치에 따라 카메라를 미세하게 움직이는 패럴랙스(스웨이) 효과.
/// 카메라(또는 카메라 리그의 부모 오브젝트)에 직접 붙여서 사용한다. 3D 기준.
/// 시작 시점의 로컬 위치/회전을 기준으로 오프셋을 더하므로, 다른 카메라 이동 로직과 함께 쓰려면
/// 이 스크립트를 카메라의 '부모' 오브젝트에 붙이는 것을 권장한다.
/// </summary>
[DisallowMultipleComponent]
public class CameraMouseParallax : MonoBehaviour
{
    [Header("위치 스웨이 (미세 이동)")]
    [SerializeField] bool usePositionSway = true;
    [Tooltip("좌우/상하로 움직이는 최대 거리")]
    [SerializeField] Vector2 positionAmount = new Vector2(0.4f, 0.25f);

    [Header("회전 틸트 (카메라 기울임)")]
    [SerializeField] bool useRotationSway = true;
    [Tooltip("상하/좌우로 기울이는 최대 각도(도)")]
    [SerializeField] Vector2 rotationAmount = new Vector2(3f, 5f);

    [Header("공통 설정")]
    [Tooltip("목표로 따라가는 부드러움. 클수록 빠르게 반응")]
    [SerializeField] float smooth = 5f;
    [Tooltip("마우스 방향과 반대로 움직이게 함")]
    [SerializeField] bool invert = false;
    [Tooltip("Time.timeScale 무시 (일시정지 메뉴 등에서 계속 동작)")]
    [SerializeField] bool ignoreTimeScale = true;

    Vector3 _startLocalPos;
    Quaternion _startLocalRot;
    bool _active = true;

    /// <summary>패럴랙스 추종 on/off. 끄면 부드럽게 원위치로 복귀한다.</summary>
    public void SetActive(bool value) => _active = value;

    void Awake()
    {
        _startLocalPos = transform.localPosition;
        _startLocalRot = transform.localRotation;
    }

    void LateUpdate()
    {
        // 비활성이거나 마우스가 없으면 중립(원위치)을 목표로 삼아 부드럽게 복귀.
        Vector2 m = Vector2.zero;
        if (_active && Mouse.current != null)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();

            // 화면 중심 기준 마우스 위치를 -1~1로 정규화.
            m = new Vector2(
                (mousePos.x / Screen.width) * 2f - 1f,
                (mousePos.y / Screen.height) * 2f - 1f);

            // 화면 밖으로 커서가 나가도 과하게 움직이지 않도록 제한.
            m.x = Mathf.Clamp(m.x, -1f, 1f);
            m.y = Mathf.Clamp(m.y, -1f, 1f);

            if (invert) m = -m;
        }

        float dt = ignoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime;
        float t = 1f - Mathf.Exp(-smooth * dt); // 프레임레이트 독립적인 감쇠 보간

        // 위치 오프셋
        Vector3 targetPos = _startLocalPos;
        if (usePositionSway)
            targetPos += new Vector3(m.x * positionAmount.x, m.y * positionAmount.y, 0f);
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPos, t);

        // 회전 오프셋 (마우스 상하 → X축 pitch, 좌우 → Y축 yaw)
        Quaternion targetRot = _startLocalRot;
        if (useRotationSway)
            targetRot *= Quaternion.Euler(-m.y * rotationAmount.x, m.x * rotationAmount.y, 0f);
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRot, t);
    }
}
