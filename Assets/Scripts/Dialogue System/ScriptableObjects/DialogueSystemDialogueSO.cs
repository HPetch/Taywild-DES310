using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DialogueSystem.ScriptableObjects
{
    using Data;
    using Types;

    public class DialogueSystemDialogueSO : ScriptableObject
    {
        [field: SerializeField] public NodeTypes NodeType { get; set; }
        [field: SerializeField] public string NodeName { get; set; }
        [field: SerializeField] [field: TextArea()] public string Text { get; set; }
        [field: SerializeField] public List<DialogueSystemDialogueChoiceData> Choices { get; set; }
        [field: SerializeField] public DialogueTypes DialogueType { get; set; }
        [field: SerializeField] public bool IsStartingDialogue { get; set; }

        public void Initialise(string nodeName, NodeTypes nodeType, string text, List<DialogueSystemDialogueChoiceData> choices, DialogueTypes dialogueType, bool isStartingDialogue)
        {
            NodeName = nodeName;
            NodeType = nodeType;
            Text = text;
            Choices = choices;
            DialogueType = dialogueType;
            IsStartingDialogue = isStartingDialogue;
        }
    }
}