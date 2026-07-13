using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

/// <summary>
/// 카드에 마우스를 올리면 그 카드의 정보를 보여주고, 마우스를 따라다니는 툴팁.
/// 마우스가 카드에서 벗어나면 숨는다. UI 캔버스(Screen Space - Overlay 권장) 아래에 둔다.
/// </summary>
public class CardInfoPanel : MonoBehaviour
{
    [SerializeField] CardInteractor interactor;

    [Tooltip("표시/숨김 및 이동할 툴팁 루트 (RectTransform)")]
    [SerializeField] RectTransform tooltipRoot;

    [Header("마우스 추종")]
    [Tooltip("커서로부터의 픽셀 오프셋. Y+가 위쪽(커서 위에 표시)")]
    [SerializeField] Vector2 offset = new Vector2(0f, 40f);
    [Tooltip("화면 밖으로 나가지 않게 가장자리 여백(픽셀)")]
    [SerializeField] float screenPadding = 8f;

    [Header("텍스트 (선택 연결)")]
    [SerializeField] TMP_Text nameText;
    [SerializeField] TMP_Text categoryText;
    [SerializeField] TMP_Text statsText;
    [SerializeField] TMP_Text descriptionText;

    bool _visible;

    void OnEnable()
    {
        if (interactor != null)
        {
            interactor.CardHovered += Show;
            interactor.CardUnhovered += Hide;
        }
        Hide();
    }

    void OnDisable()
    {
        if (interactor != null)
        {
            interactor.CardHovered -= Show;
            interactor.CardUnhovered -= Hide;
        }
    }

    void LateUpdate()
    {
        if (_visible) FollowMouse();
    }

    public void Show(CardData card)
    {
        if (card == null) { Hide(); return; }

        if (nameText != null) nameText.text = card.DisplayName;
        if (categoryText != null) categoryText.text = card.IsSpecial ? "특수" : "일반";
        if (statsText != null) statsText.text = $"코스트 {card.Cost}   파워 {card.Power}";
        if (descriptionText != null) descriptionText.text = card.Description;

        _visible = true;
        if (tooltipRoot != null) tooltipRoot.gameObject.SetActive(true);
        FollowMouse(); // 뜨자마자 위치 맞춤(한 프레임 튐 방지)
    }

    public void Hide()
    {
        _visible = false;
        if (tooltipRoot != null) tooltipRoot.gameObject.SetActive(false);
    }

    void FollowMouse()
    {
        if (tooltipRoot == null || Mouse.current == null) return;

        Vector2 mouse = Mouse.current.position.ReadValue();
        Vector2 pos = mouse + offset;

        // 툴팁이 화면 밖으로 나가지 않도록 클램프.
        Vector2 size = tooltipRoot.rect.size * tooltipRoot.lossyScale;
        float halfW = size.x * 0.5f;
        float halfH = size.y * 0.5f;
        // 피벗을 고려하지 않고 대략 중심 기준으로 클램프(피벗 0.5 가정, 아니어도 큰 문제 없음).
        pos.x = Mathf.Clamp(pos.x, halfW + screenPadding, Screen.width - halfW - screenPadding);
        pos.y = Mathf.Clamp(pos.y, halfH + screenPadding, Screen.height - halfH - screenPadding);

        tooltipRoot.position = pos;
    }
}
