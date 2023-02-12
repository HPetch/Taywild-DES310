using UnityEngine;
using UnityEditor.Experimental.GraphView;

namespace DialogueSystem.Elements
{
    public class DialogueSystemGroup : Group
    {
        public string oldTitle;
        private Color defaultBorderColor;
        private float defaultBorderWidth;

        public DialogueSystemGroup(string groupTitle, Vector2 position)
        {
            title = groupTitle;
            oldTitle = title;

            SetPosition(new Rect(position, Vector2.one));

            defaultBorderColor = contentContainer.style.borderBottomColor.value;
            defaultBorderWidth = contentContainer.style.borderBottomWidth.value;
        }

        public void SetErrorStyle(Color errorColor)
        {
            contentContainer.style.borderBottomColor = errorColor;
            contentContainer.style.borderBottomWidth = 2f;
        }

        public void ResetStlye()
        {
            contentContainer.style.borderBottomColor = defaultBorderColor;
            contentContainer.style.borderBottomWidth = defaultBorderWidth;
        }
    }
}