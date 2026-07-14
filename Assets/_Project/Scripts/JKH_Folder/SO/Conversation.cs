using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Conversation", menuName = "Scriptable Objects/Conversation")]
public class Conversation : ScriptableObject
{
    public EnemyDialogueTrigger triggerType;

    [Header("출력 대사")]
    [TextArea(2, 5)]
    public List<string> dialogues;

    public string singleDialogue;
}
