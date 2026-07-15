using UnityEngine;
using TMPro;
using TweenKit;

/// <summary>
/// 전투 필드 카드. 호버 시 확대, 클릭 시 상승, 재클릭 시 승부(지정 위치로 이동)를 위한 상태를 관리한다.
/// 호버/클릭 감지는 BattleFieldInteractor가 담당한다. (프리팹에 Collider 필요)
/// </summary>
[DisallowMultipleComponent]
public class FieldCard : MonoBehaviour
{
    [Header("텍스쳐")]
    [SerializeField] Renderer targetRenderer;
    [SerializeField] string textureProperty = "_MainTex";

    [Header("호버 (확대)")]
    [SerializeField] float hoverScale = 1.15f;
    [SerializeField] float hoverDuration = 0.15f;
    [SerializeField] Ease hoverEase = Ease.OutQuad;

    [Header("상승 (클릭)")]
    [Tooltip("클릭 시 상승 오프셋(로컬). 카드 크기의 약 20%에 맞춰 조절")]
    [SerializeField] Vector3 raiseOffset = new Vector3(0f, 0.2f, 0f);
    [SerializeField] float moveDuration = 0.18f;
    [SerializeField] Ease moveEase = Ease.OutQuad;

    [Header("딜 플립 (뒷면으로 나와 도착 시 앞면)")]
    [Tooltip("뒤집는 회전 축(로컬). 카드가 눕혀진 방향에 맞춰 X/Z 중 선택")]
    [SerializeField] Vector3 flipAxis = Vector3.right;
    [SerializeField] float flipDuration = 0.25f;
    [SerializeField] Ease flipEase = Ease.OutQuad;
    public float FlipDuration => flipDuration;

    [Header("호버 기울기 (커서 쪽으로 기움)")]
    [SerializeField] float tiltMaxAngle = 12f;
    [SerializeField] float tiltSmooth = 14f;

    [Header("승부 연출 - 특수카드 VFX (재생 → 1초 뒤 능력 적용)")]
    [Tooltip("VFX가 스폰될 위치(비우면 카드 위치 사용)")]
    [SerializeField] Transform vfxSpawnPoint;
    [Tooltip("VFX가 재생되고 나서 실제 능력이 적용되기까지의 대기 시간")]
    [SerializeField] float vfxToAbilityDelay = 1f;
    [Tooltip("VFX 프리팹에 자체 파괴 로직이 없을 때를 대비한 안전 파괴 시간(0 이하면 자동 파괴 안 함)")]
    [SerializeField] float vfxAutoDestroy = 3f;

    [Header("승부 연출 - 특수카드 판정 숫자 공개")]
    [Tooltip("확정된 판정 숫자를 보여줄 텍스트(카드의 자식, TextMeshPro). 비우면 연출 없이 대기만 함")]
    [SerializeField] TextMeshPro numberRevealText;
    [Tooltip("숫자가 팝인되는 시간")]
    [SerializeField] float numberPopDuration = 0.2f;
    [SerializeField] Ease numberPopEase = Ease.OutBack;
    [Tooltip("숫자가 나타난 뒤 승부 진행까지의 대기 시간")]
    [SerializeField] float numberToShowdownDelay = 1f;
    [Tooltip("승부 진행 시 숫자가 사라지는 시간")]
    [SerializeField] float numberFadeOutDuration = 0.2f;

    [Header("승부 연출 - 승리 카드 (들어올림 + 3축 회전 펀치)")]
    [SerializeField] float winLiftHeight = 0.15f;
    [SerializeField] float winLiftDuration = 0.2f;
    [SerializeField] Vector3 winPunchRotation = new Vector3(18f, 22f, 25f);
    [SerializeField] float winPunchDuration = 0.35f;
    [SerializeField] int winPunchVibrato = 14;

    [Header("승부 연출 - 무승부 카드 (들어올림 + 약한 3축 회전 펀치)")]
    [SerializeField] Vector3 drawPunchRotation = new Vector3(6f, 8f, 10f);
    [SerializeField] float drawPunchDuration = 0.3f;
    [SerializeField] int drawPunchVibrato = 10;

    CardData _data;
    MaterialPropertyBlock _mpb;
    Tween _moveTween;
    Tween _scaleTween;
    Tween _flipTween;
    Tween _punchRotTween;
    Tween _numberPopTween;

    Vector3 _homePos;
    Vector3 _baseScale = Vector3.one;
    Quaternion _baseRot;
    float _flipAngle; // 0=앞면, 180=뒷면
    Vector3 _punchRot; // 승부 연출용 회전 펀치 오프셋(오일러, 감쇠하며 0으로 수렴)
    bool _raised;
    bool _hovered;

    Vector2 _tiltTarget;
    Vector2 _tilt;
    Vector2 _halfExtent = new Vector2(0.5f, 0.5f);

    bool _faceDown;

    public CardData Data => _data;
    public bool IsRaised => _raised;
    public bool IsFaceDown => _faceDown;

    void Awake()
    {
        _baseScale = transform.localScale;
        _baseRot = transform.localRotation;
        if (targetRenderer != null)
        {
            Vector3 ext = targetRenderer.localBounds.extents;
            _halfExtent = new Vector2(Mathf.Max(0.0001f, ext.x), Mathf.Max(0.0001f, ext.y));
        }
        if (numberRevealText != null) numberRevealText.gameObject.SetActive(false);
    }

    void LateUpdate()
    {
        if (!_hovered) _tiltTarget = Vector2.zero;
        float t = 1f - Mathf.Exp(-tiltSmooth * Time.deltaTime);
        _tilt = Vector2.Lerp(_tilt, _tiltTarget, t);

        // 기준회전 × 호버기울기 × 플립 × 승부 펀치(회전) 를 매 프레임 합성.
        transform.localRotation = _baseRot
            * Quaternion.Euler(_tilt.x, _tilt.y, 0f)
            * Quaternion.AngleAxis(_flipAngle, flipAxis)
            * Quaternion.Euler(_punchRot);
    }

    /// <summary>커서가 카드에 닿은 월드 지점 → 중심 거리로 기울기 목표 설정.</summary>
    public void SetHoverPoint(Vector3 worldPoint)
    {
        if (!_hovered) return;
        Vector3 local = transform.InverseTransformPoint(worldPoint);
        float nx = Mathf.Clamp(local.x / _halfExtent.x, -1f, 1f);
        float ny = Mathf.Clamp(local.y / _halfExtent.y, -1f, 1f);
        _tiltTarget = new Vector2(-ny * tiltMaxAngle, nx * tiltMaxAngle);
    }

    /// <summary>즉시 뒷면 상태로. (실제 회전 합성은 LateUpdate)</summary>
    public void SetFaceDown()
    {
        _flipTween?.Kill();
        _flipAngle = 180f;
        _faceDown = true;
    }

    /// <summary>앞면으로 뒤집기(애니메이션). 각도만 갱신, 합성은 LateUpdate.</summary>
    public void FlipUp()
    {
        _flipTween?.Kill();
        _faceDown = false;
        _flipTween = Tw.To(() => _flipAngle, v => _flipAngle = v, 0f, flipDuration).SetEase(flipEase);
        if (AudioManager.instance != null) AudioManager.instance.PlaySfx(AudioManager.Sfx.CardFlip);
    }

    /// <summary>뒷면으로 뒤집기(애니메이션, 제자리). 완료 시 onComplete 호출 — 뒤집은 뒤 이동시키는 용도.</summary>
    public void FlipDown(System.Action onComplete = null)
    {
        _flipTween?.Kill();
        _faceDown = true;
        _flipTween = Tw.To(() => _flipAngle, v => _flipAngle = v, 180f, flipDuration).SetEase(flipEase);
        if (onComplete != null) _flipTween.OnComplete(onComplete);
        if (AudioManager.instance != null) AudioManager.instance.PlaySfx(AudioManager.Sfx.CardFlip);
    }

    public void Bind(CardData data)
    {
        _data = data;
        if (targetRenderer != null && data != null && data.Texture != null)
        {
            if (_mpb == null) _mpb = new MaterialPropertyBlock();
            targetRenderer.GetPropertyBlock(_mpb);
            _mpb.SetTexture(textureProperty, data.Texture);
            targetRenderer.SetPropertyBlock(_mpb);
        }
    }


    /// <summary>지정 위치(로컬)로 즉시 스냅. (딜 시작 전 더미 위에 올려둘 때)</summary>
    public void SnapTo(Vector3 localPos)
    {
        _homePos = localPos;
        _raised = false;
        transform.localPosition = localPos;
    }

    /// <summary>
    /// 배치 위치(로컬)로 이동. 상승 상태는 해제. delay만큼 늦게 출발, 도착 시 onArrived 호출.
    /// 딜/재정렬/승부이동/버림/리셔플/교환 등 '카드 위치가 옮겨지는' 모든 경로가 이걸 거쳐가므로,
    /// 실제 이동이 시작되는 시점(delay 이후)에 공통으로 드로우 사운드를 재생한다.
    /// </summary>
    public void PlaceAt(Vector3 localPos, float duration, Ease ease, float delay = 0f, System.Action onArrived = null)
    {
        _homePos = localPos;
        _raised = false;
        ApplyMove(duration, ease, delay, onArrived);
        ApplyScale(duration, ease);

        if (AudioManager.instance != null)
            Tw.Delay(delay, () => AudioManager.instance.PlaySfx(AudioManager.Sfx.CardDraw));
    }

    public void SetHovered(bool value)
    {
        if (_hovered == value) return;
        _hovered = value;
        ApplyScale(hoverDuration, hoverEase);
        if (_hovered && AudioManager.instance != null) AudioManager.instance.PlaySfx(AudioManager.Sfx.CardHover);
    }

    public void SetRaised(bool value)
    {
        if (_raised == value) return;
        _raised = value;
        ApplyMove(moveDuration, moveEase);
        if (_raised && AudioManager.instance != null) AudioManager.instance.PlaySfx(AudioManager.Sfx.CardSelect);
    }

    void ApplyMove(float duration, Ease ease, float delay = 0f, System.Action onArrived = null)
    {
        Vector3 target = _homePos + (_raised ? raiseOffset : Vector3.zero);
        _moveTween?.Kill();
        _moveTween = transform.DOLocalMove(target, duration).SetEase(ease).SetDelay(delay);
        if (onArrived != null) _moveTween.OnComplete(onArrived);
    }

    void ApplyScale(float duration, Ease ease)
    {
        Vector3 target = _baseScale * (_hovered ? hoverScale : 1f);
        _scaleTween?.Kill();
        _scaleTween = transform.DOScale(target, duration).SetEase(ease);
    }

    /// <summary>
    /// 회전 펀치: 지정 오일러각만큼 확 꺾였다가 감쇠 진동하며 원래 회전으로 복귀.
    /// X/Y/Z 축이 서로 다른 위상·주파수로 흔들려서(DOShakeScale과 같은 방식) 대각선으로만
    /// 딱딱하게 움직이지 않고 진짜 3차원적으로 통통 구르는 느낌이 난다.
    /// 스케일이 아니라 회전을 흔든다 — LateUpdate 합성식의 _punchRot 항으로 반영되므로
    /// 기울기/플립 회전과 안 부딪힌다.
    /// </summary>
    void PlayRotationPunch(Vector3 punchEuler, float duration, int vibrato, System.Action onDone)
    {
        _punchRotTween?.Kill();
        float seed = Random.value * 100f;

        _punchRotTween = Tw.To(() => 0f, val =>
        {
            float damper = 1f - val;
            float ang = val * vibrato * Mathf.PI;
            // 축마다 다른 주파수 배율 + 위상(seed)을 줘서 서로 어긋나게 진동시킴.
            float oscX = Mathf.Sin(ang + seed);
            float oscY = Mathf.Sin(ang * 1.3f + seed * 1.7f);
            float oscZ = Mathf.Sin(ang * 0.8f + seed * 2.3f);
            _punchRot = new Vector3(
                punchEuler.x * oscX,
                punchEuler.y * oscY,
                punchEuler.z * oscZ) * damper;
        }, 1f, duration).SetEase(Ease.Linear);
        _punchRotTween.OnComplete(() =>
        {
            _punchRot = Vector3.zero;
            onDone?.Invoke();
        });
    }

    // 승부 슬롯에서 살짝 들어올림. 완료 시 onDone 호출.
    void PlayLift(System.Action onDone)
    {
        Vector3 lifted = _homePos + new Vector3(0f, winLiftHeight, 0f);
        _moveTween?.Kill();
        _moveTween = transform.DOLocalMove(lifted, winLiftDuration).SetEase(Ease.OutQuad);
        _moveTween.OnComplete(() => onDone?.Invoke());
    }

    /// <summary>
    /// 특수카드 고유 VFX를 카드 위치에서 재생. VFX가 재생된 뒤 vfxToAbilityDelay만큼 지나면
    /// onAbilityApply를 호출한다(실제 능력이 이 시점에 적용됨) — VFX가 없어도 지연은 그대로 적용.
    /// </summary>
    public void PlaySpecialSymbolEffect(GameObject vfxPrefab, System.Action onAbilityApply)
    {
        if (AudioManager.instance != null) AudioManager.instance.PlaySfx(AudioManager.Sfx.SpecialActivate);
        SpawnVfx(vfxPrefab);
        Tw.Delay(vfxToAbilityDelay, () => onAbilityApply?.Invoke());
    }

    /// <summary>자체 파괴 로직이 없는 프리팹을 대비해 vfxAutoDestroy 후 안전하게 파괴.</summary>
    void SpawnVfx(GameObject prefab)
    {
        if (prefab == null) return;
        Transform origin = vfxSpawnPoint != null ? vfxSpawnPoint : transform;
        GameObject vfx = Instantiate(prefab, origin.position, origin.rotation);
        if (vfxAutoDestroy > 0f) Destroy(vfx, vfxAutoDestroy);
    }

    /// <summary>
    /// 특수카드 능력이 '숫자로 판정'되는 결과로 이어질 때(다빈치 조커의 랜덤 숫자, 혹은 드로우2/Bang!/
    /// 맥주 등이 조건 불충족으로 고정 숫자로 간주되는 경우) 확정된 판정 숫자를 카드 위에 표시.
    /// 숫자가 나타난 뒤 numberToShowdownDelay만큼 지나면 onShowdown(승부 진행)을 호출한다.
    /// </summary>
    public void PlayNumberRevealEffect(int number, System.Action onShowdown)
    {
        if (numberRevealText != null)
        {
            numberRevealText.text = number.ToString();
            numberRevealText.gameObject.SetActive(true);
            numberRevealText.transform.localScale = Vector3.zero;
            _numberPopTween?.Kill();
            _numberPopTween = numberRevealText.transform.DOScale(Vector3.one, numberPopDuration).SetEase(numberPopEase);
            if (AudioManager.instance != null) AudioManager.instance.PlaySfx(AudioManager.Sfx.SpecialActivate);
        }

        Tw.Delay(numberToShowdownDelay, () =>
        {
            onShowdown?.Invoke();

            if (numberRevealText != null)
            {
                _numberPopTween?.Kill();
                _numberPopTween = numberRevealText.transform.DOScale(Vector3.zero, numberFadeOutDuration);
                Tw.Delay(numberFadeOutDuration, () =>
                {
                    if (numberRevealText != null) numberRevealText.gameObject.SetActive(false);
                });
            }
        });
    }

    /// <summary>
    /// 승리 카드 연출: 살짝 들어올려진 뒤 회전 펀치(발라트로 느낌). 완료 시 onDone 호출.
    /// sfx는 플레이어 기준 승/패 사운드(PlayerRoundWin/PlayerRoundLose)를 호출부(BattleManager)가 결정해 넘긴다
    /// — 이 카드가 물리적으로 어느 쪽 자리에 있는지가 아니라 '누가 이겼는지' 기준이라서.
    /// </summary>
    public void PlayWinEffect(AudioManager.Sfx sfx, System.Action onDone = null)
    {
        PlayLift(() =>
        {
            if (AudioManager.instance != null) AudioManager.instance.PlaySfx(sfx);
            PlayRotationPunch(winPunchRotation, winPunchDuration, winPunchVibrato, onDone);
        });
    }

    /// <summary>무승부 카드 연출: 살짝 들어올려진 뒤 약한 회전 펀치. 완료 시 onDone 호출.</summary>
    public void PlayDrawEffect(System.Action onDone = null)
    {
        PlayLift(() =>
        {
            if (AudioManager.instance != null) AudioManager.instance.PlaySfx(AudioManager.Sfx.RoundDraw);
            PlayRotationPunch(drawPunchRotation, drawPunchDuration, drawPunchVibrato, onDone);
        });
    }
}
