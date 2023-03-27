using UnityEngine;

public class CursorController : MonoBehaviour
{
    private void Start()
    {
        DialogueController.Instance.OnConversationStart += EnableCursor;
        DialogueController.Instance.OnConversationEnd += DisableCursor;

        DecorationController.Instance.OnEnterEditMode += EnableCursor;
        DecorationController.Instance.OnExitEditMode += DisableCursor;

        // Journal Here

        DisableCursor();
    }

    private void EnableCursor()
    {
        Cursor.visible = true;
    }

    private void DisableCursor()
    {
        Cursor.visible = false;
    }
}