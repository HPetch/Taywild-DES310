using System;
using UnityEngine;
using UnityEditor.Experimental.GraphView;

namespace DialogueSystem.Elements
{
    public class DialogueSystemGroup : Group
    {
        public string ID { get; set; }
        public string OldTitle { get; set; }

        private Color defaultBorderColor;
        private float defaultBorderWidth;

        public DialogueSystemGroup(string groupTitle, Vector2 position)
        {
            ID = Guid.NewGuid().ToString();
            title = groupTitle;
            OldTitle = title;

            SetPosition(new Rect(position, Vector2.one));

            defaultBorderColor = contentContainer.style.borderTopColor.value;
            defaultBorderWidth = contentContainer.style.borderTopWidth.value;
        }

        public void SetErrorStyle(Color errorColor)
        {
            contentContainer.style.borderTopColor = errorColor;
            contentContainer.style.borderTopWidth = 2f;
        }

        public void ResetStlye()
        {
            contentContainer.style.borderTopColor = defaultBorderColor;
            contentContainer.style.borderTopWidth = defaultBorderWidth;
        }
    }
}