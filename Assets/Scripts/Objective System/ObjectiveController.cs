using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DialogueSystem.Types;

public class ObjectiveController : MonoBehaviour
{   
    public static ObjectiveController Instance { get; private set; }

    #region Variables
    private List<Quest> quests = new List<Quest>();
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
            quests.Add(new Quest((QuestTypes)quest));
        }

        GetQuest(QuestTypes.LucusFlowerQ1b).State = QuestStates.HandIn;
        GetQuest(QuestTypes.LuWaBakedQ2a).State = QuestStates.HandIn;
        GetQuest(QuestTypes.LuWaBakedQ2b).State = QuestStates.HandIn;
        GetQuest(QuestTypes.LuWaIngredientsQ3a).State = QuestStates.HandIn;
        GetQuest(QuestTypes.LuWaIngredientsQ3b).State = QuestStates.HandIn;
        GetQuest(QuestTypes.LuWaGreenhouseQ1b).State = QuestStates.HandIn;
    }

    private void Start()
    {
        DecorationController.Instance.OnPlaceDecoration += OnFurnitureObjectPlaced;
    }
    #endregion

    public Quest GetQuest(QuestTypes _quest)
    {
        return quests[(int)_quest];
    }

    public void OnFurnitureObjectPlaced(FurnitureObject _object)
    {
        if (GetQuest(QuestTypes.WarsanTutorial).State == QuestStates.InProgress && _object.UiName == "Hanging Flower Planter")
        {
            GetQuest(QuestTypes.WarsanTutorial).State = QuestStates.HandIn;
        }

        if (_object.UiName == "Hanging Flower Planter")
        {
            GetQuest(QuestTypes.WarsanBenchQ1).State = QuestStates.HandIn;
        }
    }

    
    #endregion

    [System.Serializable]
    public class Quest
    {
        [field: SerializeField] public QuestTypes QuestType { get; set; }
        public QuestStates State { get; set; }

        public Quest(QuestTypes _questType)
        {
            QuestType = _questType;
            State = QuestStates.NotAccepted;
        }
    }
}
