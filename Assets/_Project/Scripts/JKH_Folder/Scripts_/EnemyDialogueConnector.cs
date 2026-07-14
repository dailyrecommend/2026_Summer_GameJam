using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.Collections;

public class EnemyDialogueConnector : MonoBehaviour
{
    public static EnemyDialogueConnector Instance { get; private set; }

    [Header("[1. 하이어라키의 3D 텍스트 오브젝트]")]
    [SerializeField] private TextMeshPro targetText;

    [Header("[2. 프로젝트 뷰에 만든 SO 에셋 데이터들]")]
    [SerializeField] public List<Conversation> dialogueList = new List<Conversation>();

    [Header("[타이핑 속도 설정]")]
    [SerializeField] public float typingSpeed = 0.05f;

    private Coroutine typingCoroutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
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
            }

            typingCoroutine = StartCoroutine(TypeTextRoutine(matchedDialogue.dialogueText));
        }
        else
        {
            Debug.LogWarning("에셋이 리스트에 등록 안 됨");
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
