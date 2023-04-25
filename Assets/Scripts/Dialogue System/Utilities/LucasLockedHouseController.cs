using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LucasLockedHouseController : MonoBehaviour
{
    [SerializeField] private GameObject LockedHouse;
    [SerializeField] private GameObject UnLockedHouse;

    private void Start()
    {
        LockedHouse.SetActive(true);
        UnLockedHouse.SetActive(false);

        DialogueController.Instance.OnConversationEnd += OnConversationEnd;
    }

    private void OnConversationEnd()
    {
        if (DialogueController.Instance.CurrentGraph.FileName == "LucusFlowerQ1CompleteA")
        {
            LockedHouse.SetActive(false);
            UnLockedHouse.SetActive(true);
        }
    }
}
