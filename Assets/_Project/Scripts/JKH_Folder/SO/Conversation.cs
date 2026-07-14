using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Conversation", menuName = "Scriptable Objects/Conversation")]
public class Conversation : ScriptableObject
{
    [Header("[조건]")]
    public EnemyDialogueTrigger triggerType;

    [Header("[대사]")]
    public string dialogueText;
}
