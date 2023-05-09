using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DialogueSystem.Types;

public class ObjectiveController : MonoBehaviour
{   
    public static ObjectiveController Instance { get; private set; }

    #region Variables
    public List<Quest> Quests { get; private set; } = new List<Quest>();
    #endregion

    #region Functions
    #region Initialisation
    private void Awake()
    {
        // If there already exists an Instance of this singleton then destroy this object, else this is the singleton instance
        if (Instance != null) Destroy(gameObject);
        else Instance = this;

        for (int quest = 0; quest < System.Enum.GetValues(typeof(QuestTypes)).Length; quest++)
        {
            Quests.Add(new Quest((QuestTypes)quest));
        }
    }

    private void Start()
    {
        DecorationController.Instance.OnPlaceDecoration += OnFurnitureObjectPlaced;
        InventoryController.Instance.OnItemAdded += OnItemAddedToInventory;
    }
    #endregion

    public Quest GetQuest(QuestTypes _quest)
    {
        return Quests[(int)_quest];
    }

    public void OnFurnitureObjectPlaced(FurnitureObject _object)
    {
        if (_object.UiName == "Hanging Flower Planter")
        {
            Quest warsanTutorial = GetQuest(QuestTypes.WarsanTutorial);

            if (warsanTutorial.State == QuestStates.NotAccepted)
            {
                warsanTutorial.questObjectiveCompleteBeforeQuestIssued = true;
            }
            else if (warsanTutorial.State == QuestStates.InProgress)
            {
                warsanTutorial.State = QuestStates.HandIn;
            }
        }

        if (_object.UiName == "Garden Bench")
        {
            Quest warsanBenchQ1 = GetQuest(QuestTypes.WarsanBenchQ1);

            if (warsanBenchQ1.State == QuestStates.NotAccepted)
            {
                warsanBenchQ1.questObjectiveCompleteBeforeQuestIssued = true;
            }
            else if (warsanBenchQ1.State == QuestStates.InProgress)
            {
                warsanBenchQ1.State = QuestStates.HandIn;
            }
        }

        if (_object.UiName == "Log Planter")
        {
            Quest warsanLogPlanterQ2 = GetQuest(QuestTypes.WarsanLogPlanterQ2);

            if (warsanLogPlanterQ2.State == QuestStates.NotAccepted)
            {
                warsanLogPlanterQ2.questObjectiveCompleteBeforeQuestIssued = true;
            }
            else if (warsanLogPlanterQ2.State == QuestStates.InProgress)
            {
                warsanLogPlanterQ2.State = QuestStates.HandIn;
            }
        }
    }    

    public void OnItemAddedToInventory(InventoryController.ItemNames _item, int _quantityAdded, Vector2 _position)
    {
        if (_item == InventoryController.ItemNames.QUEST_BLUEFLOWER)
        {
            Quest lucasFlowerQ1a = GetQuest(QuestTypes.LucasFlowerQ1a);

            if (lucasFlowerQ1a.State == QuestStates.NotAccepted)
            {
                lucasFlowerQ1a.questObjectiveCompleteBeforeQuestIssued = true;
            }
            else if (lucasFlowerQ1a.State == QuestStates.InProgress)
            {
                lucasFlowerQ1a.State = QuestStates.HandIn;
            }
        }

        if (_item == InventoryController.ItemNames.QUEST_BOOK && InventoryController.Instance.ItemQuantity(InventoryController.ItemNames.QUEST_BOOK) > 0)
        {
            Quest LucusBerriesQ2 = GetQuest(QuestTypes.LucusBerriesQ2);

            if (LucusBerriesQ2.State == QuestStates.NotAccepted)
            {
                LucusBerriesQ2.questObjectiveCompleteBeforeQuestIssued = true;
            }
            else if (LucusBerriesQ2.State == QuestStates.InProgress)
            {
                LucusBerriesQ2.State = QuestStates.HandIn;
            }
        }

        if (_item == InventoryController.ItemNames.QUEST_BERRYS && InventoryController.Instance.ItemQuantity(InventoryController.ItemNames.QUEST_BERRYS) > 0)
        {
            Quest LucusBerriesQ2 = GetQuest(QuestTypes.LucusBerriesQ2);

            if (LucusBerriesQ2.State == QuestStates.NotAccepted)
            {
                LucusBerriesQ2.questObjectiveCompleteBeforeQuestIssued = true;
            }
            else if (LucusBerriesQ2.State == QuestStates.InProgress)
            {
                LucusBerriesQ2.State = QuestStates.HandIn;
            }
        }
    }
    #endregion

    [System.Serializable]
    public class Quest
    {
        [field: SerializeField] public QuestTypes QuestType { get; set; }
        public QuestStates State { get; set; }

        public bool questObjectiveCompleteBeforeQuestIssued = false;

        public Quest(QuestTypes _questType)
        {
            QuestType = _questType;
            State = QuestStates.NotAccepted;
        }
    }
}
