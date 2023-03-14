using System;
using UnityEngine;

public class TreeLevelController : MonoBehaviour
{
    public static TreeLevelController Instance { get; private set; }

    #region Events
    public event Action OnExpTotalChanged;
    public event Action OnTreeLevelUp;
    #endregion

    #region Variables
    public int QuestExp { get; private set; }
    public int CurrentFurnitureExp { get; private set; } // Combined tree level of all placed furniture. Ignored for total level.
    public int HighestFurnitureExp { get; private set; } // Highest furniture tree level reached. Used for total level
    public int PickupCleanExp { get; private set; } // Added to first time a non-quest pickup item is destroyed
    public int TotalExp { get; private set; }

    public int CurrentTreeLevel { get; private set; }

    [SerializeField] private int[] treeLevelExpRequirements;
    #endregion

    #region Functions

    private void Awake()
    {
        Instance = this;
    }

    public void AddQuestExp(int _Exp)
    {
        QuestExp += _Exp;
        TotalExpChanged();
    }

    public void AddFurnitureExp(int _Exp)
    {
        CurrentFurnitureExp += _Exp;
        if (CurrentFurnitureExp > HighestFurnitureExp)
        {
            HighestFurnitureExp = CurrentFurnitureExp;
            TotalExpChanged();
        }
    }

    public void RemoveFurnitureExp(int _Exp)
    {
        CurrentFurnitureExp -= _Exp;
        if (CurrentFurnitureExp < 0)
        {
            Debug.LogWarning("Current Furniture Exp reached negative: " + CurrentFurnitureExp);
            CurrentFurnitureExp = 0;
        }
    }

    public void AddCleanExp(int _Exp)
    {
        PickupCleanExp += _Exp;
        TotalExpChanged();
    }

    public void TotalExpChanged()
    {
        TotalExp = QuestExp + HighestFurnitureExp + PickupCleanExp;
        OnExpTotalChanged?.Invoke();
        Debug.Log("Tree Exp: " + TotalExp);
        if (treeLevelExpRequirements.Length > CurrentTreeLevel)
        {
            if (treeLevelExpRequirements[CurrentTreeLevel] <= TotalExp)
            {
                CurrentTreeLevel++;
                OnTreeLevelUp?.Invoke();
                Debug.Log("Level up!");
            }
        }
    }
    #endregion
}
