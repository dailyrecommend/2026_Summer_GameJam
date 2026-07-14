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

    private Coroutine typingCoroutine;
    private Tween popTween;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            if (dialogueBox == null && targetText != null) dialogueBox = targetText.transform.parent;
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

        if (matchedDialogue != null)
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
                    .OnComplete(() => typingCoroutine = StartCoroutine(TypeTextRoutine(matchedDialogue.dialogueText)));
            }
            else
            {
                typingCoroutine = StartCoroutine(TypeTextRoutine(matchedDialogue.dialogueText));
            }
        }
        else
        {
            Debug.LogWarning("������ ����Ʈ�� ��� �� ��");
        }
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
    }
}
