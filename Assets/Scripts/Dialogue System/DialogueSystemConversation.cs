using UnityEngine;

namespace DialogueSystem
{
    using ScriptableObjects;

    public class DialogueSystemConversation : MonoBehaviour
    {
        /* Dialogue Scriptable Objects */
        [SerializeField] private DialogueSystemDialogueContainerSO dialogueContainer;
        [SerializeField] private DialogueSystemDialogueGroupSO dialogueGroup;
        [SerializeField] public DialogueSystemDialogueSO dialogue;

        /* Filters */
        [SerializeField] private bool groupedDialogues;
        [SerializeField] private bool startingDialoguesOnly;

        /* Indexes */
        [SerializeField] private int selectedDialogueGroupIndex;
        [SerializeField] private int selectedDialogueIndex;
    }
}