using UnityEngine;

/// <summary>
/// 캐러셀의 스테이지 오브젝트에 붙여, 그 스테이지의 StageData를 들고 있게 한다.
/// 호버 툴팁(StageHoverInteractor)이 이 데이터를 읽는다. (오브젝트에 Collider 필요)
/// </summary>
public class StageView : MonoBehaviour
{
    [SerializeField] StageData data;
    public StageData Data => data;
}
