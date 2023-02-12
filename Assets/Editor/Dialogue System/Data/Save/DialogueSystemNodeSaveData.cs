using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DialogueSystem.Data.Save
{
    using Types;

    [System.Serializable]
    public class DialogueSystemNodeSaveData
    {
        [field: SerializeField] public string ID { get; set; }
        [field: SerializeField] public string Name { get; set; }
        [field: SerializeField] public string Text { get; set; }
        [field: SerializeField] public List<DialogueSystemChoiceSaveData> Choices { get; set; }
        [field: SerializeField] public string GroupID { get; set; }
        [field: SerializeField] public DialogueTypes DialogueType { get; set; }
        [field: SerializeField] public Vector2 Position { get; set; }        
    }
}