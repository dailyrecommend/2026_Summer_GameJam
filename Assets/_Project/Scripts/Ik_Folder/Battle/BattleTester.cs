using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 전투 필드/상호작용 확인용 테스터.
///  - D 키: 플레이어 덱에서 4장 뽑아 필드에 배치
///  - 승부에 올린 카드는 로그로 출력
/// (라운드 루프 구현 전까지 임시)
/// </summary>
public class BattleTester : MonoBehaviour
{
    [SerializeField] BattleField field;
    [SerializeField] BattleFieldInteractor interactor;
    [SerializeField] Deck deck;
    [SerializeField] int drawCount = 4;

    void OnEnable()
    {
        if (interactor != null) interactor.CardCommitted += OnCommitted;
    }

    void OnDisable()
    {
        if (interactor != null) interactor.CardCommitted -= OnCommitted;
    }

    void Update()
    {
        if (Keyboard.current == null) return;
        if (Keyboard.current.dKey.wasPressedThisFrame) DealFromDeck();
    }

    void DealFromDeck()
    {
        if (field == null || deck == null) return;

        // 덱 앞에서 drawCount장 (테스트용 간단 추출)
        List<CardData> draw = new List<CardData>();
        for (int i = 0; i < deck.Cards.Count && draw.Count < drawCount; i++)
            draw.Add(deck.Cards[i]);

        field.Deal(draw);
        Debug.Log($"[BattleTester] 필드에 {draw.Count}장 배치");
    }

    void OnCommitted(FieldCard card)
    {
        string name = card != null && card.Data != null ? card.Data.DisplayName : "?";
        int num = card != null && card.Data != null ? card.Data.Number : 0;
        Debug.Log($"[BattleTester] 승부에 올림 → {name} (숫자 {num})");
    }
}
