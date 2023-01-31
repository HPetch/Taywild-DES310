using UnityEngine;

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
    public enum ConversationEventTypes { SPEECH, BRANCH, QUEST, CUTSCENE }
    public enum BranchTypes { SHALLOW, DEEP,}

    #region Variables
    [field: SerializeField] public ConversationEventTypes EventType { get; private set; } = ConversationEventTypes.SPEECH;

    #region Speech
    [field: SerializeField] public ConversationUITemplate.ConversationUITemplates UITemplate { get; private set; }
    [field: SerializeField] public Character Character { get; private set; }

    [field: TextArea(3, 10)]
    [field: SerializeField] public string Text { get; private set; }
    #endregion

    #region Branch
    [field: SerializeField] public BranchTypes BranchType { get; private set; } = BranchTypes.SHALLOW;
    [field: SerializeField] public BranchEvent[] BranchEvents { get; private set; } = new BranchEvent[0];
    [field: SerializeField] public BranchConversation[] BranchConversations { get; private set; }
    #endregion

    #region Quest
    [field: SerializeField] public GameObject Quest { get; private set; }
    #endregion

    #region Cutscene
    [field: SerializeField] public GameObject Cutscene { get; private set; }
    #endregion
    #endregion
}

[System.Serializable]
public class BranchEvent
{
    [field: SerializeField] public string ButtonText { get; private set; }
    [field: SerializeField] public ShallowBranchConversationEvent[] Conversation { get; private set; } = new ShallowBranchConversationEvent[0];
}

[System.Serializable]
public class BranchConversation
{
    [field: SerializeField] public string ButtonText { get; private set; }
    
    [field: SerializeField] public Conversation Conversation { get; private set; }
}

[System.Serializable]
public class ShallowBranchConversationEvent
{
    #region Variables
    [field: SerializeField] public ConversationEvent.ConversationEventTypes EventType { get; private set; } = ConversationEvent.ConversationEventTypes.SPEECH;

    #region Speech
    [field: SerializeField] public ConversationUITemplate.ConversationUITemplates UITemplate { get; private set; }
    [field: SerializeField] public Character Character { get; private set; }

    [field: TextArea(3, 10)]
    [field: SerializeField] public string Text { get; private set; }
    #endregion

    #region Branch
    [field: SerializeField] public BranchConversation[] BranchConversations { get; private set; }
    #endregion

    #region Quest
    [field: SerializeField] public GameObject Quest { get; private set; }
    #endregion

    #region Cutscene
    [field: SerializeField] public GameObject Cutscene { get; private set; }
    #endregion
    #endregion
}