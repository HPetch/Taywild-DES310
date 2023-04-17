using System.Collections.Generic;
using UnityEngine;

namespace DialogueSystem.ScriptableObjects
{
    public class DialogueSystemDialogueContainerSO : ScriptableObject
    {
        [field: SerializeField] public string FileName { get; set; }
        [field: SerializeField] public SerializableDictionary<DialogueSystemDialogueGroupSO, List<DialogueSystemDialogueSO>> DialogueGroups { get; set; }
        [field: SerializeField] public List<DialogueSystemDialogueSO> UngroupedDialogues { get; set; }
        [field: SerializeField] public DialogueSystemDialogueSO StartingNode { get; private set; }

        public void Initialize(string fileName)
        {
            FileName = fileName;

            DialogueGroups = new SerializableDictionary<DialogueSystemDialogueGroupSO, List<DialogueSystemDialogueSO>>();
            UngroupedDialogues = new List<DialogueSystemDialogueSO>();
        }

        public List<string> GetDialogueGroupNames()
        {
            List<string> dialogueGroupNames = new List<string>();

            foreach (DialogueSystemDialogueGroupSO dialogueGroup in DialogueGroups.Keys)
            {
                dialogueGroupNames.Add(dialogueGroup.GroupName);
            }

            return dialogueGroupNames;
        }

        public List<string> GetGroupedDialogueNames(DialogueSystemDialogueGroupSO dialogueGroup, bool startingDialoguesOnly)
        {
            List<DialogueSystemDialogueSO> groupedDialogues = DialogueGroups[dialogueGroup];

            List<string> groupedDialogueNames = new List<string>();

            foreach (DialogueSystemDialogueSO groupedDialogue in groupedDialogues)
            {
                if (startingDialoguesOnly && !groupedDialogue.IsStartingDialogue)
                {
                    continue;
                }

                groupedDialogueNames.Add(groupedDialogue.NodeName);
            }

            return groupedDialogueNames;
        }

        public List<string> GetUngroupedDialogueNames(bool startingDialoguesOnly)
        {
            List<string> ungroupedDialogueNames = new List<string>();

            foreach (DialogueSystemDialogueSO ungroupedDialogue in UngroupedDialogues)
            {
                if (startingDialoguesOnly && !ungroupedDialogue.IsStartingDialogue)
                {
                    continue;
                }

                ungroupedDialogueNames.Add(ungroupedDialogue.NodeName);
            }

            return ungroupedDialogueNames;
        }
    }
}