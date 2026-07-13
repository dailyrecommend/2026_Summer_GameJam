using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 덱 자동정렬 확인용 테스트. Space를 누를 때마다 grantOnKey의 카드를 하나씩 덱에 추가한다.
/// (동작 확인용이므로 완성 후 지워도 됨)
/// </summary>
public class DeckTester : MonoBehaviour
{
    [SerializeField] Deck deck;
    [Tooltip("Space 누를 때마다 순서대로 지급할 카드")]
    [SerializeField] List<CardData> grantOnKey = new List<CardData>();

    int _next;

    void Update()
    {
        if (deck == null || Keyboard.current == null) return;
        if (!Keyboard.current.spaceKey.wasPressedThisFrame) return;
        if (_next >= grantOnKey.Count) return;

        CardData card = grantOnKey[_next++];
        if (deck.AddCard(card))
            Debug.Log($"[DeckTester] '{card.DisplayName}' 획득");
    }
}
