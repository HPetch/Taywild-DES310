using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DialogueSystem.ScriptableObjects;

public class TEMP_ObjectiveController : MonoBehaviour
{
    public static TEMP_ObjectiveController Instance { get; private set; }

    #region Variables
    private bool hasTalkedToRecluse = false;

    [SerializeField] private DialogueSystemDialogueSO recluseQuestIncompleteStartingNode;
    [SerializeField] private DialogueSystemDialogueSO recluseQuestCompleteStartingNode;

    private InteractableCharacter interactableCharacter;
    #endregion

    #region Functions
    #region Initialisation
    // Awake is only used for setting controller instances and referencing components
    private void Awake()
    {
        // If there already exists an Instance of this singleton then destroy this object, else this is the singleton instance
        if (Instance != null) Destroy(gameObject);
        else Instance = this;

        interactableCharacter = GetComponent<InteractableCharacter>();
    }

    private void Start()
    {
        DialogueController.Instance.OnConversationStart += OnConversationStarted;
        InventoryController.Instance.OnItemAdded += OnInventoryItemAdded;
    }
    #endregion

    private void OnConversationStarted()
    {
        if (!hasTalkedToRecluse)
        {
            hasTalkedToRecluse = true;
            interactableCharacter.StartingNode = InventoryController.Instance.ItemQuantity(InventoryController.ItemNames.QUEST_BLUEFLOWER) > 0 ? recluseQuestCompleteStartingNode : recluseQuestIncompleteStartingNode;
        }
    }

    private void OnInventoryItemAdded(InventoryController.ItemNames _item, int _quantity)
    {
        if (hasTalkedToRecluse)
        {
            interactableCharacter.StartingNode = InventoryController.Instance.ItemQuantity(InventoryController.ItemNames.QUEST_BLUEFLOWER) > 0 ? recluseQuestCompleteStartingNode : recluseQuestIncompleteStartingNode;
        }
    }
    #endregion


    private void Update()
    {
        print(interactableCharacter.StartingNode);
    }
}