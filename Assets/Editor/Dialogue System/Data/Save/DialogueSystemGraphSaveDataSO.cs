using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DialogueSystem.Data.Save
{    
    public class DialogueSystemGraphSaveDataSO : ScriptableObject
    {
        [field: SerializeField] public string FileName { get; set; }
        [field: SerializeField] public List<DialogueSystemGroupSaveData> Groups { get; set; }
        [field: SerializeField] public List<DialogueSystemNodeSaveData> Nodes { get; set; }
        [field: SerializeField] public List<string> OldGroupNames { get; set; }
        [field: SerializeField] public List<string> OldUngroupedNodeNames { get; set; }
        [field: SerializeField] public SerializableDictionary<string, List<string>> OldGroupedNodeNames { get; set; }

        public void Initialise(string fileName)
        {
            FileName = FileName;

            Groups = new List<DialogueSystemGroupSaveData>();
            Nodes = new List<DialogueSystemNodeSaveData>();
        }
    }
}