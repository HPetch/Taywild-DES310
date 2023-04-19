using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TestersInterface : MonoBehaviour
{
    #region Variables
    private Partition currentPartition;
    private Vector2 positionInPartition;
    [SerializeField] private TMP_Text partitionText;
    [SerializeField] private TMP_Text coordsText;
    [SerializeField] private TMP_Text playerControllerText;

    [SerializeField] private Canvas canvas;
    #endregion

    private void Update()
    {
        //Open close with backspace
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            if (!canvas.gameObject.activeInHierarchy)
            {
                canvas.gameObject.SetActive(true);
            }
            else { canvas.gameObject.SetActive(false); }
        }

        //Pass current partition's name into text
        currentPartition = TransitionController.Instance.CurrentPartition;
        partitionText.text = "Zone: " + currentPartition.gameObject.name.ToString();

        //Get co-ordinates relative to partition center and display
        positionInPartition = PlayerController.Instance.transform.position - currentPartition.transform.position; //Get difference between center and player pos
        coordsText.text = "Pos: " + positionInPartition.ToString("F2");

        //Show detailed state info
        if (PlayerController.Instance.IsLockedInput) playerControllerText.text = "Input is Locked.";
        else
            playerControllerText.text = "Facing right: " + PlayerController.Instance.IsFacingRight + "\n"
                + "Grounded: " + PlayerController.Instance.IsGrounded + "\n"
                + "Dashing: " + PlayerController.Instance.IsDashing + "\n"
                + "Gliding: " + PlayerController.Instance.IsGliding + "\n"
                + "Touching wall: " + PlayerController.Instance.IsTouchingWall + "\n"
                + "Sticky wall: " + PlayerController.Instance.IsWallStuck + "\n"
                + "Wall sliding: " + PlayerController.Instance.IsWallSliding + "\n";

    }

}