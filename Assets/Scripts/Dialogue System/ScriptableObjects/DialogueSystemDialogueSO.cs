using System.Collections.Generic;
using UnityEngine;

namespace DialogueSystem.ScriptableObjects
{    
    using Data;
    using Types;

    public class DialogueSystemDialogueSO : ScriptableObject
    {
        [field: SerializeField] public string NodeName { get; set; }
        [field: SerializeField] public NodeTypes NodeType { get; set; }
        [field: SerializeField] public List<DialogueSystemDialogueChoiceData> Choices { get; set; }

        [field: SerializeField] public DialogueCharacter Character { get; set; }
        [field: SerializeField] public DialogueTypes DialogueType { get; set; }
        [field: SerializeField] [field: TextArea()] public string Text { get; set; }
        [field: SerializeField] public bool IsStartingDialogue { get; set; }

        [field: SerializeField] public AudioClip SoundEffect { get; set; }
        [field: SerializeField] public float Delay { get; set; }

        public void Initialise(string _name, NodeTypes _nodeType, List<DialogueSystemDialogueChoiceData> _choices, DialogueCharacter _character, DialogueTypes _dialogueType, string _text, bool _isStartingDialogue, AudioClip _soundEffect, float _delay)
        {
            NodeName = _name;
            NodeType = _nodeType;
            Choices = _choices;

            Character = _character;
            DialogueType = _dialogueType;
            Text = _text;
            IsStartingDialogue = _isStartingDialogue;

            SoundEffect = _soundEffect;
            Delay = _delay;
        }
    }
}