using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

/// <summary>
/// 카드에 마우스를 올리면 그 카드의 정보를 보여주고, 마우스를 따라다니는 툴팁.
/// 마우스가 카드에서 벗어나면 숨는다. UI 캔버스(Screen Space - Overlay 권장) 아래에 둔다.
/// </summary>
public class CardInfoPanel : MonoBehaviour
{
    [Tooltip("덱 카드 호버 소스(선택)")]
    [SerializeField] CardInteractor interactor;
    [Tooltip("플레이어 필드 카드 호버 소스(선택)")]
    [SerializeField] BattleFieldInteractor fieldInteractor;
    [Tooltip("에너미 필드 카드 호버 소스(선택)")]
    [SerializeField] BattleFieldInteractor enemyFieldInteractor;
    [Tooltip("스테이지 호버 소스(선택)")]
    [SerializeField] StageHoverInteractor stageHover;

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

    void Awake()
    {
        // 툴팁은 표시용이라 절대 레이캐스트를 막으면 안 됨(막으면 UIPointer.IsOverUI가
        // 툴팁 자신을 '가리는 UI'로 오인해 카드 호버가 꺼지고 → 툴팁이 사라지는 깜빡임이 생김).
        if (tooltipRoot == null) return;
        CanvasGroup cg = tooltipRoot.GetComponent<CanvasGroup>();
        if (cg == null) cg = tooltipRoot.gameObject.AddComponent<CanvasGroup>();
        cg.blocksRaycasts = false;
        cg.interactable = false;
    }

    void OnEnable()
    {
        if (interactor != null)
        {
            interactor.CardHovered += Show;
            interactor.CardUnhovered += Hide;
        }
        if (fieldInteractor != null)
        {
            fieldInteractor.CardHovered += Show;
            fieldInteractor.CardUnhovered += Hide;
        }
        if (enemyFieldInteractor != null)
        {
            enemyFieldInteractor.CardHovered += Show;
            enemyFieldInteractor.CardUnhovered += Hide;
        }
        if (stageHover != null)
        {
            stageHover.StageHovered += ShowStage;
            stageHover.StageUnhovered += Hide;
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
        if (fieldInteractor != null)
        {
            fieldInteractor.CardHovered -= Show;
            fieldInteractor.CardUnhovered -= Hide;
        }
        if (enemyFieldInteractor != null)
        {
            enemyFieldInteractor.CardHovered -= Show;
            enemyFieldInteractor.CardUnhovered -= Hide;
        }
        if (stageHover != null)
        {
            stageHover.StageHovered -= ShowStage;
            stageHover.StageUnhovered -= Hide;
        }
    }

    void LateUpdate()
    {
        if (_visible) FollowMouse();
    }

    public void Show(CardData card, bool faceDown)
    {
        if (card == null) { Hide(); return; }

        if (faceDown)
        {
            ShowContent("???", "???", "???", "???");
            return;
        }
        ShowContent(card.DisplayName, card.IsSpecial ? "특수" : "일반", $"숫자 {card.Number}", card.Description);
    }

    public void ShowStage(StageData stage)
    {
        if (stage == null) { Hide(); return; }
        ShowContent(stage.DisplayName, "스테이지", "", stage.Description);
    }

    void ShowContent(string name, string category, string stats, string desc)
    {
        if (nameText != null) nameText.text = name;
        if (categoryText != null) categoryText.text = category;
        if (statsText != null) statsText.text = stats;
        if (descriptionText != null) descriptionText.text = desc;

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

        // 툴팁이 화면 밖으로 나가지 않도록 클램프. (rect.size는 Vector2, lossyScale은 Vector3 → 성분별 곱)
        Vector3 scale = tooltipRoot.lossyScale;
        Vector2 size = tooltipRoot.rect.size;
        float halfW = size.x * scale.x * 0.5f;
        float halfH = size.y * scale.y * 0.5f;
        // 피벗을 고려하지 않고 대략 중심 기준으로 클램프(피벗 0.5 가정, 아니어도 큰 문제 없음).
        pos.x = Mathf.Clamp(pos.x, halfW + screenPadding, Screen.width - halfW - screenPadding);
        pos.y = Mathf.Clamp(pos.y, halfH + screenPadding, Screen.height - halfH - screenPadding);

        tooltipRoot.position = pos;
    }
}
