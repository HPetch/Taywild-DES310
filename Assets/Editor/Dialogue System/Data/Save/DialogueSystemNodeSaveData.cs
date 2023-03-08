using System.Collections.Generic;
using UnityEngine;

namespace DialogueSystem.Data.Save
{
    using Types;
    using Elements;
    using Utilities;

    [System.Serializable]
    public class DialogueSystemNodeSaveData
    {
        [field: SerializeField] public string ID { get; set; }
        [field: SerializeField] public string Name { get; set; }
        [field: SerializeField] public NodeTypes NodeType { get; set; }

        [field: SerializeField] public string GroupID { get; set; }
        [field: SerializeField] public List<DialogueSystemChoiceSaveData> Choices { get; set; }
        [field: SerializeField] public Vector2 Position { get; set; }

        [field: SerializeField] public DialogueTypes DialogueType { get; set; }
        [field: SerializeField] public DialogueCharacter Character { get; set; }
        [field: SerializeField] public string Text { get; set; }

        [field: SerializeField] public AudioClip SoundEffect { get; set; }
        [field: SerializeField] public float Delay{ get; set; }


        public DialogueSystemNodeSaveData(DialogueSystemNode _node)
        {
            ID = _node.ID;
            NodeType = _node.NodeType;

            Name = _node.NodeName;
            GroupID = _node.Group?.ID;
            Choices = DialogueSystemIOUtility.CloneNodeChoices(_node.Choices);
            Position = _node.GetPosition().position;

            DialogueType = _node.DialogueType;
            Character = _node.Character;
            Text = _node.DialogueText;

            SoundEffect = _node.SoundEffect;
            Delay = _node.Delay;
        }
    }
}