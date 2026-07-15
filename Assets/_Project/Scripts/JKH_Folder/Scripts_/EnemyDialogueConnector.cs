using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.Collections;
using TweenKit;

public class EnemyDialogueConnector : MonoBehaviour
{
    public static EnemyDialogueConnector Instance { get; private set; }

    [Header("[1. ���̾��Ű�� 3D �ؽ�Ʈ ������Ʈ]")]
    [SerializeField] private TextMeshPro targetText;

    [Header("[2. ������Ʈ �信 ���� SO ���� �����͵�]")]
    [SerializeField] public List<Conversation> dialogueList = new List<Conversation>();

    [Header("[Ÿ���� �ӵ� ����]")]
    [SerializeField] public float typingSpeed = 0.05f;

    [Header("[등장 팝인 연출]")]
    [Tooltip("커질 대화 박스(비우면 targetText의 부모를 자동 사용)")]
    [SerializeField] private Transform dialogueBox;
    [Tooltip("새 대화가 시작될 때 0→1로 커지는 시간")]
    [SerializeField] private float popDuration = 0.2f;
    [SerializeField] private Ease popEase = Ease.OutBack;

    [Header("[유지 - 타이핑 완료 후]")]
    [Tooltip("대사를 다 친 뒤, 이 시간 동안은 새 대사가 와도 끼어들지 않고 유지한다. 새 대사가 없으면 계속 유지됨")]
    [SerializeField] private float holdDuration = 1f;

    private Coroutine typingCoroutine;
    private Tween popTween;
    private Tween pendingSwitchTween; // 유지 시간이 끝난 뒤로 미뤄둔 '다음 대사 전환' 예약(재요청 시 취소 대상)
    private float _holdUntil = -1f; // 이 시각까지는 현재 대사를 끊지 않음(Time.time 기준)

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            if (dialogueBox == null && targetText != null) dialogueBox = targetText.transform.parent;
            // 아무 트리거도 없었던 최초 상태에서는 아무것도 보이면 안 됨.
            if (targetText != null) targetText.text = "";
            if (dialogueBox != null) dialogueBox.localScale = Vector3.zero;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void TriggerDialogueByCondition(EnemyDialogueTrigger trigger)
    {
        if (targetText == null) return;

        Conversation matchedDialogue = dialogueList.Find(so => so != null && so.triggerType == trigger);
        if (matchedDialogue == null)
        {
            Debug.LogWarning("������ ����Ʈ�� ��� �� ��");
            return;
        }

        pendingSwitchTween?.Kill();

        // 아직 타이핑 중이면 바로 끊고 새 대사로. 다 친 상태에서 유지 시간이 안 지났으면
        // 유지 시간이 끝난 뒤에 전환되도록 예약(그동안은 지금 대사 그대로 보여줌).
        float remainingHold = _holdUntil - Time.time;
        if (typingCoroutine == null && remainingHold > 0f)
        {
            pendingSwitchTween = Tw.Delay(remainingHold, () => StartDialogue(matchedDialogue));
        }
        else
        {
            StartDialogue(matchedDialogue);
        }
    }

    void StartDialogue(Conversation dialogue)
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }
        popTween?.Kill();

        if (dialogueBox != null)
        {
            // 박스가 0→1로 빠르게 커진 뒤에 타이핑 시작.
            dialogueBox.localScale = Vector3.zero;
            popTween = dialogueBox.DOScale(1f, popDuration)
                .SetEase(popEase)
                .OnComplete(() => typingCoroutine = StartCoroutine(TypeTextRoutine(dialogue.dialogueText)));
        }
        else
        {
            typingCoroutine = StartCoroutine(TypeTextRoutine(dialogue.dialogueText));
        }
    }

    /// <summary>대화창 초기화. 텍스트를 비우고 박스 크기를 0으로 되돌린다(전투 종료 시 등).</summary>
    public void ResetDialogue()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }
        popTween?.Kill();
        pendingSwitchTween?.Kill();
        _holdUntil = -1f;

        if (targetText != null) targetText.text = "";
        if (dialogueBox != null) dialogueBox.localScale = Vector3.zero;
    }

    public IEnumerator TypeTextRoutine(string fullText)
    {
        targetText.text = " ";

        for(int i=0; i<fullText.Length;i++)
        {
            targetText.text += fullText[i];

            yield return new WaitForSeconds(typingSpeed);
        }

        typingCoroutine = null;
        _holdUntil = Time.time + holdDuration;
    }
}
