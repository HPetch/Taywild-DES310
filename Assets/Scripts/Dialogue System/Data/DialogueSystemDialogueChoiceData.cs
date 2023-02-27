using System;
using UnityEngine;

namespace DialogueSystem.Types
{
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