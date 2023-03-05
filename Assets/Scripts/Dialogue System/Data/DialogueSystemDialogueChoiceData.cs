using System;
using UnityEngine;

namespace DialogueSystem.Types
{
    public enum NodeTypes { Dialogue, Edge }
    public enum DialogueTypes { SingleChoice, MultipleChoice }
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