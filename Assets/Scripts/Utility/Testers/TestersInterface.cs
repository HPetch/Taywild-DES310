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
    [SerializeField] private TMP_Text versionText;

    [SerializeField] private Canvas canvas;
    [SerializeField] private TMP_Text graphInfoText;
    [SerializeField] private TMP_Text nodeInfoText;
    #endregion

    private void Awake()
    {
        versionText.text = "Version: " + Application.version;
    }

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
        if (PlayerController.Instance.IsLockedInput)
        {
            playerControllerText.text = "Input is Locked.";
            //Show dialogue info if its happening
            if (DialogueController.Instance.IsConversing)
            {
                //As long as dialogue exists show the current node and etc.
                if (DialogueController.Instance.CurrentGraph != null)
                {
                    graphInfoText.text = "Graph name: " + DialogueController.Instance.CurrentGraph.FileName;
                    nodeInfoText.text = "Node name: " + DialogueController.Instance.GetCurrentNodeName();
                }
                else
                {
                    graphInfoText.text = "Graph name: cannot find! This is a big bug, report this please!";
                    nodeInfoText.text = "Node name: null";
                }
            }
            else //If dialogue is not happening just show it as null
            {
                graphInfoText.text = "Graph name: null";
                nodeInfoText.text = "Node name: null";
            }
        }
        else
        {
            //Say graph text = null, etc
            graphInfoText.text = "Graph name: null";
            nodeInfoText.text = "Node name: null";

            //Detailed state info for player controller if not locked in place.
            playerControllerText.text = "Facing right: " + PlayerController.Instance.IsFacingRight + "\n"
                + "Grounded: " + PlayerController.Instance.IsGrounded + "\n"
                + "Dashing: " + PlayerController.Instance.IsDashing + "\n"
                + "Gliding: " + PlayerController.Instance.IsGliding + "\n"
                + "Touching walljump wall: " + PlayerController.Instance.IsTouchingWall + "\n"
                + "Sticky wall: " + PlayerController.Instance.IsWallStuck + "\n"
                + "Wall sliding: " + PlayerController.Instance.IsWallSliding + "\n";
        }
    }

}