using UnityEngine;

namespace DialogueSystem.ScriptableObjects
{
    public class DialogueSystemDialogueGroupSO : ScriptableObject
    {
        [field: SerializeField] public string GroupName { get; set; }

        public void Initialise(string groupName)
        {
            GroupName = groupName;
        }
    }
}