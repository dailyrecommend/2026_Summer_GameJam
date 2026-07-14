using UnityEngine.EventSystems;

/// <summary>
/// 마우스가 UI(설정창 같은 풀스크린 패널 등) 위에 있는지 확인하는 헬퍼.
/// 3D 월드를 Physics.Raycast로 직접 쏘는 인터랙터들(카드/캐러셀/화면전환 등)은
/// UI를 그냥 통과해버리므로, Update() 맨 앞에서 이걸로 먼저 걸러야 한다.
/// (UI 패널의 Image가 Raycast Target 체크돼 있어야 감지됨)
/// </summary>
public static class UIPointer
{
    public static bool IsOverUI => EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
}
