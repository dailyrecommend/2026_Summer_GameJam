using UnityEngine;

/// <summary>카드 분류. 덱 정렬에서 일반=좌측, 특수=우측 기준으로 쓰인다.</summary>
public enum CardCategory { Normal, Special }

/// <summary>
/// 특수카드 로직이 참조할 컨텍스트. 전투 구현 시 필요한 정보(대상, 플레이어 상태 등)를 여기에 채운다.
/// 지금은 훅만 잡아둔 빈 껍데기.
/// </summary>
public class CardUseContext
{
    // TODO(전투 구현 시): caster, target, board 상태 등 추가.
}

/// <summary>
/// 모든 카드의 공통 데이터(ScriptableObject). 직접 에셋으로 만들지 않고
/// NormalCardData / SpecialCardData 를 통해 생성한다.
/// 카드 겉모습은 3D 오브젝트이며 <see cref="Texture"/> 를 카드 면에 입힌다.
/// </summary>
public abstract class CardData : ScriptableObject
{
    [Header("식별")]
    [Tooltip("저장/보상 매칭용 고유 ID (에셋마다 유일하게)")]
    [SerializeField] string id;
    [SerializeField] string displayName;
    [TextArea] [SerializeField] string description;

    [Header("표시 (3D 카드 면에 입힐 텍스쳐)")]
    [SerializeField] Texture texture;

    [Header("수치")]
    [Tooltip("승부에서 비교하는 카드 숫자")]
    [SerializeField] int number;

    public string Id => id;
    public string DisplayName => displayName;
    public string Description => description;
    public Texture Texture => texture;
    public int Number => number;

    /// <summary>일반/특수 분류. 파생 클래스가 결정.</summary>
    public abstract CardCategory Category { get; }
    public bool IsSpecial => Category == CardCategory.Special;
}
