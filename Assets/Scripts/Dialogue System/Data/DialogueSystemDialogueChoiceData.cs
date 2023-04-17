using System;
using UnityEngine;

namespace DialogueSystem.Types
{
    public enum NodeTypes { Dialogue, Edge, Delay, Audio, Quest, Graph }
    public enum DialogueTypes { SingleChoice, MultipleChoice }
    public enum Quests { WarsanQuest1, WarsanQuest2, LucanQuest1 }
}

namespace DialogueSystem.Data
{
    using ScriptableObjects;

    [Serializable]
    public class DialogueSystemDialogueChoiceData
    {
        [field: SerializeField] public string Text { get; set; }
        [field: SerializeField] public DialogueSystemDialogueSO NextDialogue { get; set; }
    }
}