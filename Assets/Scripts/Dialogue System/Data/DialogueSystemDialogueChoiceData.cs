using System;
using UnityEngine;

namespace DialogueSystem.Types
{
    public enum NodeTypes { Dialogue, Edge, Delay, Audio, Quest, Graph }
    public enum DialogueTypes { SingleChoice, MultipleChoice }
    public enum Quests { WarsanTutorial, WarsanBenchQ1, WarsanLogPlanterQ2, LucasFlowerQ1a, LucusFlowerQ1b, LucusBerriesQ2, LuWaGreenhouseQ1a, LuWaGreenhouseQ1b, LuWaBakedQ2a, LuWaBakedQ2b, LuWaIngredientsQ3a, LuWaIngredientsQ3b }
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