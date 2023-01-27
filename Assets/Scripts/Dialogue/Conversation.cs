using TMPro;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "Conversation", menuName = "Dialogue/Conversation")]
public class Conversation : ScriptableObject
{
    [field: Range(0, 1)]
    [field: SerializeField] public float ConversationStartDelay { get; private set; } = 0.0f;
    [field: Range(0, 1)]
    [field: SerializeField] public float ConversationEndDelay { get; private set; } = 0.2f;

    [field: Space(20)]
    [field: SerializeField] public ConversationEvent[] ConversationEvents { get; private set; }
}

[System.Serializable]
public class ConversationEvent
{
    public enum ConversationEventType { SPEECH, BRANCH, QUEST, CUTSCENE }

    #region Variables
    [field: SerializeField] public ConversationEventType EventType { get; private set; } = ConversationEventType.SPEECH;

    #region Speech
    [field: SerializeField] public ConversationUITemplate.ConversationUITemplates UITemplate { get; private set; }
    [field: SerializeField] public Character Character { get; private set; }

    [field: TextArea(3, 10)]
    [field: SerializeField] public string Text { get; private set; }
    #endregion

    #region Branch
    //[field: SerializeField] public ConversationEvent[] BranchEvents { get; private set; }
    #endregion

    #region Quest
    [field: SerializeField] public GameObject Quest { get; private set; }
    #endregion

    #region Cutscene
    [field: SerializeField] public GameObject Cutscene { get; private set; }
    #endregion
    #endregion
}